module Infra.Async.Async

let map mapper asyncValue = async {
    let! value = asyncValue
    return mapper value
}

let lift value = async {
    return value
}