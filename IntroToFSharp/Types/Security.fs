namespace Types.Security.Web

open System

type WebSecurityPrincipal =
    | Anonymous
    | System
    | User of WebUserIdentity

and WebUserIdentity =
    {
        id: string
        name: string
        permissions: string list
    }

namespace Types.Security.App
    
open System

type AppSecurityPrincipal =
    | Anonymous
    | System
    | User of AppUserIdentity

and AppUserIdentity =
    {
        id: Guid
        name: string
        permissions: string list
    }