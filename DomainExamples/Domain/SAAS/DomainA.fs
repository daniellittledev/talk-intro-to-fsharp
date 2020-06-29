module DomainA

[<AbstractClass>]
type Service() =
    abstract member GetCost: unit -> decimal

