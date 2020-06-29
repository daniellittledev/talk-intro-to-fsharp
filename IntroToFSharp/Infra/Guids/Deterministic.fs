module Infra.Guids.Deterministic

open System

open System
open System.Text
open System.Security.Cryptography

type HashMethod =
    | MD5Method = 3
    | SHA1Method = 5

let private swapBytes (guid: byte array) (left: int) (right: int) =
    let temp = guid.[left]
    guid.[left] <- guid.[right]
    guid.[right] <- temp;

// Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
let private swapByteOrder (guid: byte array) =
    swapBytes guid 0 3
    swapBytes guid 1 2
    swapBytes guid 4 5
    swapBytes guid 6 7

/// <summary>
/// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
/// </summary>
/// <param name="namespaceId">The ID of the namespace.</param>
/// <param name="name">The name (within that namespace).</param>
/// <param name="version">The version of the UUID to create.</param>
/// <returns>A UUID derived from the namespace and name.</returns>
/// <remarks>See <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>.</remarks>
let createGuid2 (namespaceId: Guid) (name: string) (hashMethod: HashMethod) : Guid =

    if (String.IsNullOrEmpty(name)) then raise (ArgumentNullException "name")

    // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
    // ASSUME: UTF-8 encoding is always appropriate
    let nameBytes = Encoding.UTF8.GetBytes(name)

    // convert the namespace UUID to network order (step 3)
    let namespaceBytes = namespaceId.ToByteArray()
    swapByteOrder namespaceBytes

    // comput the hash of the name space ID concatenated with the name (step 4)
    let algorithm =
        match hashMethod with
        | HashMethod.MD5Method -> MD5.Create() :> HashAlgorithm
        | HashMethod.SHA1Method -> SHA1.Create() :> HashAlgorithm
        | _ -> failwith "Bad enum value for method"

    algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0) |> ignore
    algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length) |> ignore
    let hash = algorithm.Hash

    // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
    let newGuid = Array.create 16 Byte.MinValue
    Array.Copy(hash, 0, newGuid, 0, 16)

    let toByte (x: int) = System.Convert.ToByte x
    let version = LanguagePrimitives.EnumToValue hashMethod

    // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
    newGuid.[6] <- ((newGuid.[6] &&& 0x0Fuy) ||| (toByte (version <<< 4)))

    // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
    newGuid.[8] <- (byte) ((newGuid.[8] &&& 0x3Fuy) ||| 0x80uy)

    // convert the resulting UUID to local byte order (step 13)
    swapByteOrder newGuid

    Guid(newGuid)

let createGuid (namespaceId: Guid) (name: string) : Guid =
    createGuid2 namespaceId name HashMethod.SHA1Method

/// <summary>
/// The namespace for fully-qualified domain names (from RFC 4122, Appendix C).
/// </summary>
let dnsNamespace = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8")

/// <summary>
/// The namespace for URLs (from RFC 4122, Appendix C).
/// </summary>
let urlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8")

/// <summary>
/// The namespace for ISO OIDs (from RFC 4122, Appendix C).
/// </summary>
let isoOidNamespace = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8")


let ruleFour = new Guid("d78c4a73-2f15-4136-a07c-31e23884f9f3")
let toGuid (name: string) : Guid =
    createGuid2 ruleFour name HashMethod.SHA1Method

let toGuid2 (guid: Guid) (name: string) : Guid =
    createGuid2 ruleFour (guid.ToString("N") + name) HashMethod.SHA1Method
