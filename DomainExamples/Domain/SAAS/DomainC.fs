module DomainC

type Cost = Cost of decimal
type FreemiumPerUser =
    | Free
    | Premium of costPerUser: Cost

let azureDevops = UserBasedFreemium (UserCount 6, { freeUsers = 5; costPerUser = Cost 8m })
let slack = PerUser (Cost 6.67m)
let netlify = Tiered [Tier ]
let twillio = FlatRate


(*
devops = FreemiumPerUser (FreeUsers 5, Cost 8m, Users 5)
slack = PerUser (Cost 6m, Users 10)
twillio = CostPerTransaction (Cost 0.10m, Messages 1000)
mixpannel = Free
figma = PerUser (Cost 12m, Users 2)
hubspot = Tier (Cost 75m)
xero = Tier (Cost 25m)
*)