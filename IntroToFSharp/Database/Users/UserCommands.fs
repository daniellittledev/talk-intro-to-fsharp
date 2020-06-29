module Database.Users.UserCommands

open System
open Infra.Storage

type InsertUserParameters =
    {
        authId: string
        userId: Guid
    }

type InsertUserDbCommand = InsertUserParameters -> Async<unit>

let insertUser transaction : InsertUserDbCommand =
    fun parameters -> async {
        let userIdStr = parameters.userId.ToString()
        return! transaction |> KeyValueStore.add parameters.authId userIdStr
    }

