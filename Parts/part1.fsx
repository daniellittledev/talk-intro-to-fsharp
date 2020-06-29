type LocalDate = int

let x = 10
let add x y = x + y
let add = fun x y -> x + y

let x = 
	"Daniel"
	"Daniel"

type ValidDOB = ValidDOB of LocalDate

let parseDOB (now: LocalDate) (date: LocalDate) : Result<ValidDOB, string> =
	if date.PlusYears(18) < now then
		ValidDOB date |> Ok
    else
    	MustBe18YearsOld |> Error

isOfAge today
