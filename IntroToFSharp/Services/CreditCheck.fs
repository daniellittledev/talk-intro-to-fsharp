module Services.CreditCheck

open Serilog
open Infra.Async
open Infra.Http
open DomainModel
open System.Net

type CreditCheckError =
    | HttpError of HttpError
    | ParsingError of string
    | UnexpectedResponse of HttpStatusCode

type CheckCreditFunction = CreditAssessment -> AsyncResult<CreditCheck, CreditCheckError>

let checkCredit: ILogger -> Fetch -> CheckCreditFunction =
    fun log fetch details ->
        asyncResult {
        
            let! response =
                fetch ({ defaultHttpRequest "https://api.creditchecks.com.x" with Method = Some Post}) // TODO use details
                |> mapError HttpError

            match response with
            | Success r ->
                let value = r.Content.ReadAsStreamAsync() |> Async.AwaitTask |> Async.map Ok
                // TODO parse value
                return Scored (CreditScore 700)

            | _ ->
                return! Error (UnexpectedResponse response.StatusCode)

        } |> teeError (fun x -> log.ForContext("Error", x).Error("An error occured while running credit check"))

