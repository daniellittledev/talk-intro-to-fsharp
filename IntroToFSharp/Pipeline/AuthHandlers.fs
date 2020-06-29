[<AutoOpen>]
module Pipeline.AuthHandlers

open Microsoft.Extensions.Caching.Memory
open Types.Application
open Types.Security
open Types.Security.App
open Infra.Guids
open Infra.Mediation.MessageContracts
open Database.Users

let authHandler
    (cache: MemoryCache)
    (getUser: UserQueries.GetUserByAuthIdDbQuery)
    (insertUser: UserCommands.InsertUserDbCommand)
    handler
    (m: Context<WebHandlerContext, 'a>) = async {

    let mapMessage (principal: App.AppSecurityPrincipal) =
        { payload = m.payload; context = { log = m.context.log; principal = principal } }

    match m.context.principal with
    | Web.Anonymous ->
        return! handler (mapMessage App.Anonymous)
    | Web.System ->
        return! handler (mapMessage App.System)
    | Web.User auth ->
        let! user =
            cache.GetOrCreateAsync(auth.id, (fun _ -> Async.StartAsTask <| async {
                let! userId = getUser auth.id
                match userId with
                | Some id ->
                    return {
                        id = id
                        name = auth.name
                        permissions = auth.permissions
                    }
                | None ->
                    let userId = Deterministic.toGuid auth.id
                    do! insertUser {
                            userId = userId
                            authId = auth.id
                        }
                    return {
                        id = userId
                        name = auth.name
                        permissions = auth.permissions
                    }
            })) |> Async.AwaitTask

        let message = mapMessage (App.User user)
        return! handler message
}
