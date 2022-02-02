namespace Finance.FSharp

open System

[<AutoOpen>]
module Guid = 
    let (|Guid|_|) (str: string) = 
        match Guid.TryParse str with 
        | true, v -> Some v
        | _ -> None

[<AutoOpen>]
module Decimal =
    let (|IsDecimal|_|) (str : string) =
        match Decimal.TryParse(str) with
        | true, d -> Some d
        | _ -> None
    
    let (|IsDecimalOptional|_|) (str : string) =
        str
        |> Option.ofObj
        |> Option.map(fun str -> 
            match Decimal.TryParse(str) with
            | true, d -> Some d
            | _ -> None )

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