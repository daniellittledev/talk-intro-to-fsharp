module Program

open Serilog
open CompositionRoot
open Infra.Mediation
open Infra.Mediation.MessageContracts
open Types.Security.Web
open DomainModel
open Handlers.ApplicationHandlers.SubmitApplicationCommandHandler

[<EntryPoint>]
let main args = Async.RunSynchronously <| async {
    
    let log =
        LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger()

    let mediator = composeModules log

    let send = send mediator.commandHandlers

    let! result = send {
        context =
            {
                log = log
                principal = Anonymous
            }
        payload =
            ({
                Application = {
                    Order = PlanOnly Small
                    PersonDetails = {
                        GivenName = GivenName "Daniel"
                        LastName = LastName "Little"
                        DateOfBirth = DateOfBirth { Day = 3; Month = 10; Year = 1988 }
                        ContactNumber = PhoneNumber "0400000000"
                        EmailAddress = EmailAddress "daniellittle@hey.com"
                    }
                    AddressAndDelivery = {
                        HomeAddress = {
                            AddressLine = AddressLine "01 Main St"
                            Suburb = Suburb "Brisbane City"
                            State = Queensland
                            Postcode = Postcode "4000"
                        }
                        DeliveryPreference = HomeAddress
                    }
                    CreditAssessment = {
                        Identification = AustralianDriversLicense {
                            GivenName = GivenName "Daniel"
                            FamilyName = LastName "Little"
                            State = Queensland
                            LicenseNumber = DriversLicenseNumber "00000000"
                            DateOfExpiry = DateOfExpiry { Day = 12; Month = 12; Year = 2022 }
                        }
                        PrimarySourceOfIncome = PerminateFullTimeEmployment
                    }
                    BillingAndPayment = {
                        PaymentMethod = {
                            CardType = Mastercard
                            CardNumber = CardNumber "0000 0000 0000 0000"
                            Expiry = DateOfExpiry { Day = 1; Month = 1; Year = 2022 }
                            CCV = CCV "000"
                        }
                        PaymentDetailsStorage = RememberPaymentDetails
                    }
                }
            } : SubmitApplicationCommand)
    }

    log.Information("{@Result}",
        match result with
        | Ok -> "Success!"
        | Error e -> sprintf "%A" e
    )

    return 0
}