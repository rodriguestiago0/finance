namespace Finance.FSharp

open System
open System.Text

[<AutoOpen>]
module Guid = 
    let (|Guid|_|) (str: string) = 
        match Guid.TryParse str with 
        | true, v -> Some v
        | _ -> None

[<AutoOpen>]
module Prelude =
    let konst x _ = x

    let nullable (x: 't)    
        = Nullable<'t>(x)

    let inline flip f a b = f b a

[<AutoOpen>]
module Seqs =
    let (|NotEmptySeq|_|) a =
        if Seq.isEmpty a then None
        else Some a

[<AutoOpen>]
module Arrays =
    let getLastN n array =
        match Array.length array with
        | l when l <= n -> array
        | l -> Array.skip (l - n) array

[<AutoOpen>]
module DiscriminatedUnion =
    open Microsoft.FSharp.Reflection

    let inline fromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        | [|case|] -> Ok (FSharpValue.MakeUnion(case,[||]) :?> 'a)
        | _ -> sprintf "Unexpected Enumerator value %s" s
               |> exn
               |> Error

    let getAsString (x:'a) =
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | _, value -> Seq.head value |> string

    let getCaseName (x:'a) =
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name

[<AutoOpen>]
module TypeIds =
    let inline deconstruct x = (^a: (member Deconstruct: unit -> ^b) x)
    
    let inline deconstructOption x = x |> Option.map deconstruct

    let inline idToString x = deconstruct x |> string

    let inline idToStringOption x = deconstructOption x |> Option.map string

    let inline fromBase64String str =
        Convert.FromBase64String(str)

    let inline toBase64String b =
        Convert.ToBase64String(b)

    let inline encodingGetString (b : byte[]) =
        Encoding.UTF8.GetString(b)

    let inline encodingGetBytes (str : string) =
        Encoding.UTF8.GetBytes(str)

[<AutoOpen>]
module Strings =
    let join (separator: string) (strings: seq<string>) =
        String.Join(separator, strings)

    let split (separator: string) (str: string) =
        str.Split(separator)
        
    let length (str: string) =
        str.Length
        
    let trim (s: string) = s.Trim()
    
    let trimEnd (trimChar: char) (s: string) =
        s.TrimEnd trimChar

    let (|NotWhiteSpace|_|) a =
        if String.IsNullOrWhiteSpace a then None
        else Some a

    let (|NotEmptyOrWhiteSpace|_|) a =
        if String.IsNullOrEmpty a || String.IsNullOrWhiteSpace a then None
        else Some a

    let (|NotNullOrEmpty|_|) (str : string) =
        if String.IsNullOrEmpty str then
            None
        else Some str

    let parseToInt (str : string) =
        str |> int

[<AutoOpen>]
module Decimal =
    let (|IsDecimal|_|) (str : string) =
        match Decimal.TryParse(str) with
        | true, d -> Some d
        | _ -> None

    let (|IsDecimalOptional|_|) (str : string) =
        match str with
        | NotNullOrEmpty str ->
            match Decimal.TryParse(str) with
            | true, d -> Some (Some d)
            | _ -> None
        | _ -> Some None

    let addOptional (dO1 : Option<decimal>) (dO2 : Option<decimal>) =
        match dO1, dO2 with
        | Some d1, Some d2 -> Some (d1+d2)
        | Some d1, None -> Some d1
        | None, Some d2 -> Some d2
        | _ -> None

[<AutoOpen>]
module DateTimes =
    let inline toUtcDateTime value =
        DateTime.SpecifyKind(value, DateTimeKind.Utc)
        
    let inline mkUtcWhenNoKind (value: DateTime) =
        if value.Kind = DateTimeKind.Utc then value
        else toUtcDateTime value
        
    let inline mkDateTimeOffset date =
        date
        |> (mkUtcWhenNoKind >> DateTimeOffset)

[<AutoOpen>]
module Exceptions =
    type NotFoundException(message) =
        inherit Exception(message)
        new() = NotFoundException(null)

    type NoContentException() =
        inherit Exception()

    type BadRequestException(message) =
        inherit Exception(message)
        new() = BadRequestException(null)

    type ForbiddenException(message) =
        inherit Exception(message)
        new() = ForbiddenException(null)

    type ValidationException(exns: exn []) =
        inherit AggregateException(exns)

        static member ofErrors errors =
            (Seq.map Exception errors)
            |> Array.ofSeq
            |> ValidationException :> exn

    type ConflictException(message) =
        inherit Exception(message)
        new() = ConflictException(null)

    let inline isNotFoundException (e: exn) =
        match e with
        | :? NotFoundException -> true
        | _ -> false