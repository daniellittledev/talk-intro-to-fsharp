namespace Infra.Mediation

open System
open Infra.Mediation.MessageContracts

type Handler<'context, 'r, 'e> = Type * (Context<'context, obj> -> Async<Result<'r, 'e>>)
type QueryHandler<'context, 'e> = Handler<'context, obj, 'e>
type EventHandler<'context, 'e> = Handler<'context, obj, 'e>
type CommandHandler<'context, 'e> = Handler<'context, obj, 'e>

type MediatorState<'context, 'e> = 
    {
        queryHandlers: QueryHandler<'context, 'e> list
        eventHandlers: EventHandler<'context, 'e> list
        commandHandlers: CommandHandler<'context, 'e> list
    }

[<AutoOpen>]
module Mediator =

    let private mapContext<'context, 't, 'm> (m: Context<'context, 't>) =
        { context = m.context; payload = m.payload :> obj :?> 'm }

    let toHandlerAsync<'context, 'm, 'r, 'e> (func: Context<'context, 'm> -> Async<Result<'r, 'e>>) =
        typeof<'m>, (fun (message: Context<'context, obj>) -> async {
            let! result = func (message |> mapContext<_, _, 'm>)
            return result |> Result.map (fun x -> x :> obj)
        })
        
    let toHandler<'context, 'm, 'r, 'e> (func: Context<'context, 'm> -> Result<'r, 'e>) =
        toHandlerAsync<'context, 'm, 'r, 'e> (fun m -> async { return func m })

    let rec private execute
        (handlers: Handler<'context, obj, 'e> list)
        (messageType: Type)
        (message: Context<'context, 'm>) = async {
        let matchingHandlers = handlers |> List.filter (fun (t, _) -> messageType = t)
        let! result =
            matchingHandlers
            |> List.map (fun (_, handler) ->
                handler (message |> mapContext<_, _, obj>))
            |> Async.Sequential
        return result
    }

    let query<'context, 'q, 'r, 'e when 'q :> IQuery<'q, Result<'r, 'e>>> (queryHandlers: QueryHandler<'context, 'e> list) (query: Context<'context, 'q>) : Async<Result<'r, 'e>> = async {
        let queryType = query.payload.GetType()
        let! result = execute queryHandlers queryType query
        return result |> Array.tryHead |> Option.defaultWith (fun () -> failwithf "No query handler found for %s" (typeof<'q>.ToString())) |> Result.map (fun s -> s :?> 'r)
    }

    let send<'context, 'c, 'e when 'c :> ICommand> (commandHandlers: CommandHandler<'context, 'e> list) (command: Context<'context, 'c>) : Async<Result<unit, 'e>> = async {
        let commandType = command.payload.GetType()
        let! result = execute commandHandlers commandType command
        return result |> Array.tryHead |> Option.defaultWith (fun () -> failwithf "No command handler found for %s" (typeof<'c>.ToString())) |> Result.map (fun _ -> ())
    }

    let publish<'context, 'event, 'e when 'event :> IEvent> (eventHandlers: EventHandler<'context, 'e> list) (event: Context<'context, 'event>) : Async<Result<unit, 'e> list> = async {
        let eventType = event.payload.GetType()
        let! result = execute eventHandlers eventType event
        return result |> Array.toList |> List.map (fun r -> r |> Result.map (fun _ -> ()))
    }

    let handler<'context, 'c, 'r, 'e> : (Context<'context, 'c> -> Async<Result<'r, 'e>>) -> Context<'context, 'c> -> Async<Result<'r, 'e>> =
        fun handler command -> handler command

    let commandHandler<'context, 'm, 'e when 'm :> ICommand> = handler<'context, 'm, unit, 'e>
    let eventHandler<'context, 'm, 'e when 'm :> IEvent> = handler<'context, 'm, unit, 'e>
    let queryHandler<'context, 'm, 'r, 'e when 'm :> IQuery<'m, Result<'r, 'e>>> = handler<'context, 'm, 'r, 'e>
