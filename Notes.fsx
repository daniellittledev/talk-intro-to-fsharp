module Domain

// Put, Get and Get All

// Start with the domain of the application

// Then composition root

// Everything is about composition


// Budget app?
// Calc internet speed?

let name = "Daniel"


// Commands and Events

// track software for a startup

// Pricing type:
// User pricing
// Tiered User Pricing
// Per Storage Pricing
// Feature Based Pricing
// Pay as You Go (Time, Transactions)
// Flat Rate
// Free

type PricingType =
  | PerUser
  | PerStorage
  | PerFeature
  | Tiered
  | PayAsYouGo //(Time, Transactions)
  | FlatRate
  | Free

type PricingType2 =
  | PerUser of decimal
  | PerStorage
  | PerFeature
  | Tiered
  | PayAsYouGo //(Time, Transactions)
  | FlatRate
  | Free

let azureDevops = Free * (PerUser 8m)

let slack = Free * (PerUser 6.67m)

(*
Auth0
Netlify
Mixpanel
Figma
Hubspot
Office365
Zoom
Twilio
AzureDevOps
Xero
Zendesk
Cloudflare
Pulumi
Seq
Octopus Deploy
Notion
*)

// Auto... no we might have different users/levels per app

