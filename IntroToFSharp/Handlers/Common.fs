[<AutoOpen>]
module Handlers.Common

open Types.Application
open Infra.Http
open Database

type HandlerError = 
    | ValidationErrors of ValidationErrors
    | DatabaseWriteError of DatabaseWriteError
    | DatabaseReadError of DatabaseReadError
    | HttpError of HttpError
