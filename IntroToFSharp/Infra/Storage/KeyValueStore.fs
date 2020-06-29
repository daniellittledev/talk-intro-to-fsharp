module Infra.Storage.KeyValueStore

type MutableList<'value> = System.Collections.Generic.List<'value>
type ConcurrentDictionary<'key, 'value> = System.Collections.Concurrent.ConcurrentDictionary<'key, 'value>

type Document =
    {
        body: string
    }

type Effect =
    | Add of string * Document
    | Remove of string

type Store =
    {
        documents: ConcurrentDictionary<string, Document>
    }

type Transaction =
    {
        pending: Effect System.Collections.Generic.List
        store: Store
    }

let connect (connectionString: string) =
    {
        documents = ConcurrentDictionary<string, Document>()
    }

let add key value transaction = async {
    transaction.pending.Add(Add (key, { body = value }))
}

let remove key transaction = async {
    transaction.pending.Add(Remove key)
}

let beginTransaction store =
    {
        pending = MutableList()
        store = store
    }

let commit transaction store =
    for effect in transaction.pending do
        match effect with
        | Add (key, doc) -> store.documents.AddOrUpdate(key, doc, fun _ _ -> doc) |> ignore
        | Remove key -> store.documents.TryRemove(key) |> ignore
    transaction.pending.Clear()

let rollback transaction store =
    transaction.pending.Clear()

let get id transaction = async {
    let found, document = transaction.store.documents.TryGetValue id
    return if found then Some document else None
}
