namespace Finance.FSharp

open System

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

[<AutoOpen>]
module TypeIds =
    let inline deconstruct x = (^a: (member Deconstruct: unit -> ^b) x)
    
    let inline deconstructOption x = x |> Option.map deconstruct

    let inline idToString x = deconstruct x |> string

    let inline idToStringOption x = deconstructOption x |> Option.map string

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
        
    let isNullOrEmpty (s: string) =
        String.IsNullOrEmpty(s)

    let (|IsNotNullOrEmpty|_|) (str : string) =
        if isNullOrEmpty str then
            None
        else Some str

[<AutoOpen>]
module Decimal =
    let (|IsDecimal|_|) (str : string) =
        match Decimal.TryParse(str) with
        | true, d -> Some d
        | _ -> None

    let (|IsDecimalOptional|_|) (str : string) =
        match str with
        | IsNotNullOrEmpty str ->
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
        new() = NotFoundException(String.Empty)

    type NoContentException() =
        inherit Exception()

    type BadRequestException(message) =
        inherit Exception(message)

    type ForbiddenException(message) =
        inherit Exception(message)
        new() = ForbiddenException(String.Empty)