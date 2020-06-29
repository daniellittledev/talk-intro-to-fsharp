namespace Infra.Mediation.MessageContracts

type IQuery<'Query, 'Result> when 'Query :> IQuery<'Query, 'Result> =
    interface end

type IEvent =
    interface end

type ICommand =
    interface end

type IContext<'Context, 'Payload> =
    abstract member context: 'Context with get
    abstract member payload: 'Payload with get

type Context<'Context, 'Payload> =
    {
        context: 'Context
        payload: 'Payload
    }
    interface IContext<'Context, 'Payload> with
        member this.context with get () = this.context
        member this.payload with get () = this.payload

[<AutoOpen>]
module MessageContractHelpers =
    open Microsoft.FSharp.Reflection

    let private contextTypeDef = typedefof<Context<_, _>>
    let makeContextRecord (metadata: 'Metadata) (data: 'Data) =
        let typeParameters = [| typeof<'Metadata>; data.GetType() |]
        let arguments: obj array = [| metadata; data |]
        let recordType = contextTypeDef.MakeGenericType(typeParameters);
        FSharpValue.MakeRecord(recordType, arguments)

    let makeContext (context: 'Context) (payload: 'Payload) : Context<'Context, 'Payload> =
        { context = context; payload = payload }
