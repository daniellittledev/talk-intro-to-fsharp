﻿namespace Types.Application

open System
open Serilog
open NodaTime
open Types.Security

type WebHandlerContext =
    {
        log: ILogger
        principal: Web.WebSecurityPrincipal
    }

type AppHandlerContext =
    {
        log: ILogger
        principal: App.AppSecurityPrincipal
    }

type Clock = unit -> Instant

(*
type HandlerError = 
    | NotAuthenticated of string
    | NotAuthorised of string
    | NotFound of string
    | Conflict of string
    | NotImplemented
*)

type AccessType =
    | Read
    | Write

type ValidationErrors =
    {
        errors: string list
    }
