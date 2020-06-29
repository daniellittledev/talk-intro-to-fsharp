namespace Types.Security

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