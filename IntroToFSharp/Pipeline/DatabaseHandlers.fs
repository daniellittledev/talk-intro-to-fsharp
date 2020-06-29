[<AutoOpen>]
module Pipeline.DatabaseHandlers

open Infra.Storage

let startTransaction store handler m = async {
    let transaction = store |> KeyValueStore.beginTransaction
    let! result = handler transaction m
    match result with
    | Ok _ -> store |> KeyValueStore.commit transaction
    | Error _ -> store |> KeyValueStore.rollback transaction
    return result
}