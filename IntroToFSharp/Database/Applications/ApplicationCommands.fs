module Database.Aplications.SaveApplication

open System
open Infra.Storage
open DomainModel
open Infra.Async
open Database

type InsertApplicationSubmissionParameters =
    {
        Id: Guid
        CreditCheck: CreditCheck
        Application: Application
    }

type InsertApplicationSubmissionDbCommand = InsertApplicationSubmissionParameters -> AsyncResult<unit, DatabaseWriteError>

let insertApplicationSubmission transaction : InsertApplicationSubmissionDbCommand =
    fun parameters -> asyncResult {
        return ()
    }

