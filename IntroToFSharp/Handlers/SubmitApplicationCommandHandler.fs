module Handlers.ApplicationHandlers.SubmitApplicationCommandHandler

open System
open Infra.Async
open Infra.Mediation.MessageContracts
open Infra.Mediation
open Types.Application

open Database
open Database.Aplications.ApplicationCommands
open Services.CreditCheck
open DomainModel
open Handlers

type SubmitApplicationCommand =
    {
        Application: Application
    }
    interface ICommand

let validateModel application : Result<unit, ValidationErrors> =
    Ok ()

let errorsAreUnscored result =
    result |> AsyncResult.bindError (fun _ ->
        Ok CreditCheck.Unscored
    )

let handle
    (newGuid: unit -> Guid)
    (checkCredit: CheckCreditFunction)
    (insertApplicationSubmission: InsertApplicationSubmissionDbCommand)
    =
    commandHandler<AppHandlerContext, SubmitApplicationCommand, HandlerError> (fun command -> asyncResult {
    let c = command.payload
    let application = c.Application

    // here we're "emulating" the symantics of exception handling but we have a few benifits
    // We know what functions can fail, and how they could fail

    do! validateModel application |> Result.mapError ValidationErrors

    (*

    let result =
        try
            checkCredit application.CreditAssessment
        with
            _ -> CreditCheck.Unscored

    *)

    let! creditCheck =
        checkCredit application.CreditAssessment |> errorsAreUnscored

    // Saving the credit check in the application and the error would require a try catch otherwise.
    // A map or a match is much nicer because it can be composed instead of wrapped, because the error flows down!

    let submission =
        {
            Id = newGuid ()
            CreditCheck = creditCheck
            Application = application
        }

    return! insertApplicationSubmission submission
        |> AsyncResult.mapError DatabaseWriteError

})
