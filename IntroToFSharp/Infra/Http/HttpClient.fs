module Infra.Http

open System.Net.Http
open System.Net
open System.Threading.Tasks
open Infra.Async

type HttpMethod =
    | Get
    | Post
    | Put
    | Delete
    | Head
    | Options
    | Patch
    | Trace
    | OtherMethod of method: string
    member this.ToHttpMethod() = 
        match this with
        | Get -> Http.HttpMethod.Get
        | Post -> Http.HttpMethod.Post
        | Put -> Http.HttpMethod.Put
        | Delete -> Http.HttpMethod.Delete
        | Head -> Http.HttpMethod.Head
        | Options -> Http.HttpMethod.Options
        | Patch -> Http.HttpMethod.Patch
        | Trace -> Http.HttpMethod.Trace
        | OtherMethod method -> Http.HttpMethod(method)

type HttpError =
    | Cancelled
    | NetworkError of string

type HttpRequest =
    {
        Url: string
        Content: HttpContent option
        Headers: Map<string, string list> option
        Method: HttpMethod option
    }

type HttpResponse =
    {
        Headers: Map<string, string list>
        StatusCode: HttpStatusCode
        Content: HttpContent
    }

let defaultHttpRequest url =
    {
        Url = url
        Content = None
        Headers = None
        Method = None
    }

let (|Informational|_|) (response: HttpResponse) =
    match (LanguagePrimitives.EnumToValue response.StatusCode) with
    | x when x <= 100 -> Some response
    | _ -> None

let (|Success|_|) (response: HttpResponse) =
    match (LanguagePrimitives.EnumToValue response.StatusCode) with
    | x when x > 100 && x <= 200 -> Some response
    | _ -> None

let (|Redirection|_|) (response: HttpResponse) =
    match (LanguagePrimitives.EnumToValue response.StatusCode) with
    | x when x > 200 && x <= 300 -> Some response
    | _ -> None

let (|ClientError|_|) (response: HttpResponse) =
    match (LanguagePrimitives.EnumToValue response.StatusCode) with
    | x when x > 300 && x <= 400 -> Some response
    | _ -> None

let (|ServerError|_|) (response: HttpResponse) =
    match (LanguagePrimitives.EnumToValue response.StatusCode) with
    | x when x > 400 && x <= 500 -> Some response
    | _ -> None

let fetch (client: HttpClient) (request: HttpRequest) = async {
    let method = request.Method |> Option.defaultValue HttpMethod.Get
    let content = request.Content |> Option.defaultValue (new StringContent("") :> HttpContent)
    let headers = request.Headers |> Option.defaultValue (Map [])

    use request =
        let request =
            new HttpRequestMessage(
                method.ToHttpMethod(),
                request.Url
            )
        request.Content <- content
        for header in headers do request.Headers.Add(header.Key, header.Value)
        request

    try
        let! response = client.SendAsync(request) |> Async.AwaitTask
        let headers = response.Headers |> Seq.map (fun x -> x.Key, x.Value |> Seq.toList) |> Map
        let payload = { Headers = headers; Content = response.Content; StatusCode = response.StatusCode}
        return Ok payload

    with
    | :? System.Net.Sockets.SocketException as e -> return NetworkError e.Message |> Error
    | :? HttpRequestException as e -> return NetworkError e.Message |> Error
    | :? TaskCanceledException as e -> return Cancelled |> Error
    | e -> return NetworkError e.Message |> Error
}

type Fetch = HttpRequest -> AsyncResult<HttpResponse, HttpError>

    //ArgumentNullException The request was null.
    //InvalidOperationException The request message was already sent by the HttpClient instance.
    //HttpRequestException The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
    //TaskCanceledException The request timed-out or the user canceled the request's Task.