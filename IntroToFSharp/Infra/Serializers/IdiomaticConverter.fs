namespace Infra.Serializers.JsonConverters

open System
open System.Linq
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.Serialization
open FSharp.Reflection
open System.Collections.Generic
open System.Collections.Concurrent

exception ExpectedObjectWithSinglePropertyMatchingAUnionCase of string

type UnionConverter() =
    inherit JsonConverter()

    static let unionCheckCache = ConcurrentDictionary<Type, bool>()
    static let unionCaseCache = ConcurrentDictionary<Type, UnionCaseInfo array>()

    let getUnionBuilder (unionType: Type) =
        unionCaseCache.GetOrAdd(unionType, (fun _ -> FSharpType.GetUnionCases unionType))

    member private __.castAs<'T when 'T : null> o : 'T option =
        match (o :> obj) with
        | :? 'T as res -> Some res
        | _ -> None

    override this.WriteJson(writer, value, serializer) =
        let unwrapedValue =
            if isNull value then
                null
            else
                let resolver = serializer.ContractResolver |> this.castAs<DefaultContractResolver>
                let unionType = value.GetType()
                let caseInfo, values = FSharpValue.GetUnionFields (value, unionType)

                let renderCaseProperty name value =
                    let jObj = JObject()
                    let propertyValue = if isNull value then JValue.CreateNull() :> JToken else JToken.FromObject(value, serializer)
                    let name = resolver |> Option.bind (fun x -> x.GetResolvedPropertyName(name) |> Some) |> Option.defaultValue name
                    jObj.Add(name, propertyValue)
                    jObj :> obj

                match values with
                | [||] -> caseInfo.Name :> obj
                | [|x|] -> renderCaseProperty caseInfo.Name x
                | fields -> renderCaseProperty caseInfo.Name fields

        serializer.Serialize(writer, unwrapedValue);

    override __.ReadJson(reader, destinationType, existingValue, serializer) = 
        let unionCases = getUnionBuilder destinationType
        let jToken = JToken.Load(reader)

        let hasCaseProp (o: JObject) = 
            o.ContainsKey("case") || o.ContainsKey("Case")

        let getMatchingUnionCase (jObject: JObject) = 
            let prop =
                try jObject.Properties().Single()
                with _ -> raise <| ExpectedObjectWithSinglePropertyMatchingAUnionCase (jObject.ToString())
            let caseName = prop.Name
            let caseOrNone = unionCases |> Array.tryFind (fun x -> x.Name.Equals(caseName, StringComparison.InvariantCultureIgnoreCase))
            match caseOrNone with
            | Some case -> (prop.Value, case)
            | _ -> KeyNotFoundException(sprintf "The key %s could not be found in the Union %s" caseName destinationType.FullName) |> raise  


        match jToken.Type with
        | JTokenType.Object when hasCaseProp (jToken :?> JObject) ->
            let reader = jToken.CreateReader()
            reader.Read() |> ignore
            let defaultDUConverter = Newtonsoft.Json.Converters.DiscriminatedUnionConverter()
            defaultDUConverter.ReadJson(reader, destinationType, existingValue, serializer)
        
        | JTokenType.Null -> null

        | JTokenType.String ->
            let caseName = jToken.Value<string>()
            let unionCase = unionCases |> Array.find (fun x -> x.Name = caseName)
            FSharpValue.MakeUnion (unionCase, [| |])

        | JTokenType.Object ->
            let valueToken, matchingCase = getMatchingUnionCase (jToken :?> JObject)
            let expectedFields = matchingCase.GetFields()

            match expectedFields with
            | [||] ->
                FSharpValue.MakeUnion (matchingCase, [||])
            | [|fieldType|] ->
                let object = valueToken.ToObject(fieldType.PropertyType, serializer)
                FSharpValue.MakeUnion (matchingCase, [|object|])
            | expectedFields ->
                let jsonFields =
                    match valueToken.Type with
                    | JTokenType.Array -> valueToken :?> JArray |> Seq.toArray
                    | _ -> [|valueToken|]
                let fields =
                    Array.zip expectedFields jsonFields
                    |> Array.map (fun (fieldType, token) ->
                        token.ToObject(fieldType.PropertyType, serializer))
                FSharpValue.MakeUnion (matchingCase, fields)
        
        | _ -> failwithf "While reading a Union an unknown token of type %s was encoundered while expecting a Union Case" (jToken.Type.ToString())

    override __.CanConvert(objectType) =
        unionCheckCache.GetOrAdd(objectType, (fun t ->
            (not <| typeof<System.Collections.IEnumerable>.IsAssignableFrom(objectType)) &&
            FSharpType.IsUnion objectType
        ))

type OptionConverter() = 
    inherit JsonConverter()
    
    override this.WriteJson(writer, value, serializer) =
        let unwrapedValue =
            match value with
            | null -> null
            | _ ->
                let unionType = value.GetType()
                let _, values = FSharpValue.GetUnionFields(value, unionType)
                values.[0]
        serializer.Serialize(writer, unwrapedValue);

    override __.ReadJson(reader, destinationType, obje, serializer) = 
        let unionCases = FSharpType.GetUnionCases destinationType
        match reader.TokenType with
        | JsonToken.Null | JsonToken.Undefined ->
            null :> obj
        | _ ->
            let typeArg = destinationType.GenericTypeArguments.[0]
            let value = serializer.Deserialize(reader, typeArg)
            FSharpValue.MakeUnion (unionCases.[1], [| value |])

    override __.CanConvert(objectType) =
        objectType.IsGenericType
        && objectType.GetGenericTypeDefinition() = typedefof<Option<_>>

