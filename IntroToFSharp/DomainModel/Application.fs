namespace DomainModel

// Common //

type LocalDate =
    {
        Day: int
        Month: int
        Year: int
    }

// Order //

type PhoneModelCode = PhoneModelCode of string

type Plan =
    | Small
    | Medium
    | Large

// Plan - Extra Large, $100 month to month
// Phone - iPhone 11 Pro 256GB Space Grey, $83.29/mth over 24 months

type Order =
    | PlanOnly of Plan
    | PhoneAndPlan of PhoneModelCode * Plan

// Personal Details //

type GivenName = GivenName of string
type LastName = LastName of string
type DateOfBirth = DateOfBirth of LocalDate
type PhoneNumber = PhoneNumber of string
type EmailAddress = EmailAddress of string
type VerifiedEmailAddress = VerifiedEmailAddress of EmailAddress

type PersonDetails =
    {
        GivenName: GivenName
        LastName: LastName
        DateOfBirth: DateOfBirth
        ContactNumber: PhoneNumber
        EmailAddress: EmailAddress
    }

// Address & Delivery

type AddressLine = AddressLine of string
type Suburb = Suburb of string
type AustralianState =
    | NewSouthWales
    | Queensland
    | SouthAustralia
    | Tasmania
    | Victoria
    | WesternAustralia
type Postcode = Postcode of string

type Address =
    {
        AddressLine: AddressLine
        Suburb: Suburb
        State: AustralianState
        Postcode: Postcode
    }

type DeliveryPreference = 
    | HomeAddress
    | DifferentAddress of Address

type AddressAndDelivery =
    {
        HomeAddress: Address
        DeliveryPreference: DeliveryPreference
    }

// Credit Assessment

type DateOfExpiry = DateOfExpiry of LocalDate
type DateOfIssue = DateOfIssue of LocalDate

type DriversLicenseNumber = DriversLicenseNumber of string

type DriversLicense =
    {
        GivenName: GivenName
        FamilyName: LastName
        State: AustralianState
        LicenseNumber: DriversLicenseNumber
        DateOfExpiry: DateOfExpiry
    }

type PassportNumber = PassportNumber of string

type Passport =
    {
        GivenName: GivenName
        FamilyName: LastName
        PassportNumber: PassportNumber
        DateOfIssue: DateOfIssue
        DateOfExpiry: DateOfExpiry
    }

type Identification =
    | AustralianDriversLicense of DriversLicense
    | AustralianPassport of Passport

type SourceOfIncome =
    | PerminateFullTimeEmployment
    | PerminatePartTimeEmployment
    | CasualEmplopyment
    | AnotherFamilyMember
    | Investments

type CreditAssessment =
    {
        Identification: Identification
        PrimarySourceOfIncome: SourceOfIncome
    }

// Billing and Payment
type CardNumber = CardNumber of string

type CardType = Visa | Mastercard
type CCV = CCV of string

type CreditCardInfo =
    {
        CardType: CardType
        CardNumber: CardNumber
        Expiry: DateOfExpiry
        CCV: CCV
    }

type PaymentDetailsStorage =
    | RememberPaymentDetails
    | ForgetPaymentDetails

type BillingAndPayment =
    {
        PaymentMethod: CreditCardInfo
        PaymentDetailsStorage: PaymentDetailsStorage
    }

type Application =
    {
        Order: Order
        PersonDetails: PersonDetails
        AddressAndDelivery: AddressAndDelivery
        CreditAssessment: CreditAssessment
        BillingAndPayment: BillingAndPayment
    }

type CreditScore = CreditScore of int

type CreditCheck =
    | Scored of CreditScore
    | Unscored

type ApplicationSubmission =
    {
        Application: Application
        CreditCheck: CreditCheck
    }
