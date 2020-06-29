module Payments

(*
IPaymentMethod
Cash
Check(int checkNo)
Card(string cardType, string cardNo)
*)

type CheckNumber = int
type CardNumber = string

type CardType = Visa | Mastercard
type CreditCardInfo =
    {
        CardType: CardType
        CardNumber: CardNumber
    }
type PaymentMethod =
    | Cash
    | Check of CheckNumber
    | Card of CreditCardInfo

type PaymentAmount = decimal
type Currency = AUD | USD

type Payment =
    {
        Amount: PaymentAmount
        Currency: Currency
        Method: PaymentMethod
    }
