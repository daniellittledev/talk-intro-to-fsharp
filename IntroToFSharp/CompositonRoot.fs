module CompositionRoot

open System
open Microsoft.Extensions.Caching.Memory
open Serilog

open Infra.Mediation
open Infra.Storage
open Infra.Http

open Pipeline
open Types.Application
open Database.Users
open Database.Aplications
open Services.CreditCheck

open Handlers
open Handlers.ApplicationHandlers

let composeModules (log: ILogger) : MediatorState<WebHandlerContext, HandlerError> =

    let newGuid = Guid.NewGuid
    let store = KeyValueStore.connect "in-memory"
    let cache = new MemoryCache(MemoryCacheOptions())
    let httpClient = new System.Net.Http.HttpClient()
    let fetch = fetch httpClient
    let checkCredit = checkCredit log fetch

    let withTransaction handler =
        startTransaction store handler

    let withAuthentication handler transaction message =
        let getUser = UserQueries.getUser transaction
        let insertUser = UserCommands.insertUser transaction
        authHandler cache getUser insertUser (handler transaction) message

    let commandHandlers =
        [
            (fun transaction ->
                let insertApplicationSubmission = ApplicationCommands.insertApplicationSubmission transaction
                SubmitApplicationCommandHandler.handle newGuid checkCredit insertApplicationSubmission
            ) |> withAuthentication |> withTransaction |> toHandlerAsync
        ]

    {
        commandHandlers = commandHandlers
        queryHandlers = []
        eventHandlers = []
    }

