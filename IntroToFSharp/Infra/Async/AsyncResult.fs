[<AutoOpen>]
module Infra.Async.AsyncResult

open System.Threading.Tasks
open System

type AsyncResult<'s, 'e> = Async<Result<'s, 'e>>

let map mapper asyncResult =
    async {
        match! asyncResult with
        | Ok v -> return Ok (mapper v)
        | Error err -> return Error err
    }

let bind binder asyncResult =
    async {
        match! asyncResult with
        | Ok v -> return! binder v
        | Error err -> return Error err
    }

let mapError mapper asyncResult = async {
    match! asyncResult with
    | Ok s -> return Ok s
    | Error e -> return Error (mapper e)
}

let bindError binder asyncResult = async {
    match! asyncResult with
    | Ok s -> return Ok s
    | Error e -> return (binder e)
}

let tee action asyncResult =
    asyncResult |> map (fun x -> action x; x)

let teeError action asyncResult =
    asyncResult |> mapError (fun x -> action x; x)

let zero () = async { return Ok () }

type AsyncResultBuilder() =

    // return value

    member __.Return(value: 's) =
        async { return Ok value }

    // return! asyncResult

    member __.ReturnFrom(asyncResult: Async<Result<'s, 'e>>) : Async<Result<'s, 'e>> =
        asyncResult

    // return! Result

    member __.ReturnFrom(result: Result<'s, 'e>) : Async<Result<'s, 'e>> =
        async { return result }

    // return! Task<T>

    member __.ReturnFrom(task: Task<'s>) : Async<Result<'s, 'e>> =
        async {
            let! result = task |> Async.AwaitTask
            return Ok result
        }

    // no return

    member __.Zero() = zero ()

    member __.Combine(expr1, expr2) =
        bind (fun () -> expr2) expr1

    member __.Bind(asyncResult: Async<Result<'a, 'e>>, binder: 'a -> Async<Result<'s, 'e>>) =
        bind binder asyncResult

    member __.Bind(result: Result<'a, 'e>, binder: 'a -> Async<Result<'s, 'e>>) =
        match result with
        | Ok v -> binder v
        | Error err -> async { return Error err }

    member __.Bind(task: Task, binder: unit -> Async<Result<'s, 'e>>) =
        async {
            do! task |> Async.AwaitTask
            return! binder ()
        }

    member __.Bind(task: Task<'T>, binder: 'T -> Async<Result<'s, 'e>>) : Async<Result<'s, 'e>> =
        async {
            let! result = task |> Async.AwaitTask
            return! binder result
        }

    member __.Using(result: #IDisposable, body) =
        __.TryFinally(
            body result,
            fun () ->
                match result with
                | null -> ()
                | disp -> disp.Dispose()
        )

    member __.Delay(f: unit -> Async<Result<_, _>>) = f()

    member __.TryWith(m: Async<Result<_, _>>, h) =
        async {
            try
                let! x = m
                return x
            with
                e -> return! (h e)
        }

    member __.TryFinally(m: Async<Result<_, _>>, compensation) =
        async {
            try
                let! x = m
                return x
            finally
                compensation()
        }

    member this.While(guard, body) =
        if not (guard())
        then this.Zero()
        else
            bind (fun () -> this.While(guard, body)) (body())

    member this.For(sequence:seq<_>, body) =
        this.Using(sequence.GetEnumerator(), fun enum ->
             this.While(enum.MoveNext,
                 fun () -> this.Delay(fun () -> body enum.Current)))

let asyncResult = new AsyncResultBuilder()

let toAsync x = async { return x }
