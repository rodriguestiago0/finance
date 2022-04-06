namespace Finance.Application.FileImporter

module FileImporterService =
    type FileImporter =
        | Degiro
        | Revolut
        with
        static member toInt value =
            match value with
            | Degiro -> 1
            | Revolut -> 2

        static member fromInt value =
            match value with
            | 1 -> Ok Degiro
            | 2 -> Ok Revolut
            | _ -> "Invalid File importer value" |> exn |> Error
