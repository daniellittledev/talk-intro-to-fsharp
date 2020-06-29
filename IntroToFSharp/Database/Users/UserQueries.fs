module Database.Users.UserQueries

open System
open Infra.Storage

type GetUserByAuthIdDbQuery = string -> Guid option Async

let getUser transaction : GetUserByAuthIdDbQuery =
    fun authId -> async {
        let! document = transaction |> KeyValueStore.get authId
        return document |> Option.map (fun x -> Guid x.body)
    }

