[<AutoOpen>]
module Database.Core

type DatabaseWriteError =
    | ConnectionError
    | DatabaseWriteError

type DatabaseReadError =
    | ConnectionFailed
    | RecordNotFound
