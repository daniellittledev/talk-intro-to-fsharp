module Domain

type Name = Name of string
type Cost = Cost of decimal
type Tier = Tier of Name * Cost
type UserCount = UserCount of int

type UserBasedFreemium =
    {
        freeUsers: int
        costPerUser: Cost
    }

type PayAsYouGoType =
    | Time of Cost
    | Transaction of Cost

type PricingType =
  | PerUser of UserCount * Cost
  //| PerStorage
  //| PerFeature
  //| Tiered of Tier list
  | PayAsYouGo of PayAsYouGoType
  | FlatRate of Cost
  | UserBasedFreemium of UserCount * UserBasedFreemium
  | Free

let azureDevops = UserBasedFreemium (UserCount 6, { freeUsers = 5; costPerUser = Cost 8m })
let slack = PerUser (Cost 6.67m)
let netlify = Tiered [Tier ("A", 10m); Tier ("B", 10m); Tier ("C", 10m)]
let twillio = FlatRates

type Service =
    {
        name: string
        PricingType: PricingType
    }
    
let calulateCosts (services: Service list) : Cost =
    List.sum (fun service ->
        match service with
        | Free -> Cost 0m
        | azureDevops -> 

    )


