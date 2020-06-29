module Infra.Serializers.SerializerSettings

open NodaTime
open NodaTime.Serialization.JsonNet
open Infra.Serializers.JsonConverters
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let defaultSettings =
    let settings = new Newtonsoft.Json.JsonSerializerSettings()
    settings.ContractResolver <-
        DefaultContractResolver(
            NamingStrategy = new CamelCaseNamingStrategy(
                ProcessDictionaryKeys = false,
                OverrideSpecifiedNames = true
            )
        )
    settings.Converters.Add(new OptionConverter())
    settings.Converters.Add(new UnionConverter())
    settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb) |> ignore

    settings

let serialize (settings: JsonSerializerSettings) (x: 't) = JsonConvert.SerializeObject(x :> obj, settings)
let deserialize (settings: JsonSerializerSettings) (x: string) = JsonConvert.DeserializeObject(x, typeof<'t>, settings) :?> 't

