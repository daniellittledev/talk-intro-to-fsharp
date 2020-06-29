module CompositionRoot

open System
open Microsoft.Extensions.Caching.Memory

open Infra.Mediation
open Infra.Storage

open Types
open Pipeline
open Database.Users
open Handlers.ApplicationHandlers

let composeModules () : MediatorState<WebHandlerContext, HandlerError> =
    let newGuid () = Guid.NewGuid()
    let clock () = NodaTime.SystemClock.Instance.GetCurrentInstant()
    let store = KeyValueStore.connect "in-memory"
    let cache = new MemoryCache(MemoryCacheOptions())

    let withTransaction handler =
        startTransaction store handler

    let withAppAuth handler transaction message =
        let getUser = UserQueries.getUser transaction
        let insertUser = UserCommands.insertUser transaction
        authHandler cache getUser insertUser (handler transaction) message

    (*
    (validateModel: Application -> Result<unit, ValidationErrors>)
    (creditCheck: Application -> Async<Result<CreditCheckResult, RemoteCallError>>)
    (saveApplication: Application -> Async<Result<unit, DatabaseWriteError>>)
    *)

    let commandHandlers =
        [
            (fun transaction -> SubmitApplicationCommandHandler.handle clock newGuid _ _ _ _ _) |> withAppAuth |> withTransaction |> toHandlerAsync
        ]

    {
        commandHandlers = commandHandlers
        queryHandlers = []
        eventHandlers = []
    }

