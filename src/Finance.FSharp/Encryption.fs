namespace Finance.FSharp

open System.IO
open System.Security.Cryptography
open System.Text

module Encryption =
    let private convertToArray = System.Convert.FromBase64String
    let private convertToString = System.Convert.ToBase64String
    let private encodingToString : byte[] -> string = Encoding.UTF8.GetString
    let private encodingToArray : string -> byte[] = Encoding.UTF8.GetBytes

    let private encryptAlgorithm key =
        let aes = Aes.Create ()
        aes.KeySize <- 256
        aes.Key <- (encodingToArray key)
        aes

    let private toString (cypher, iv) =
        String.concat "@" [cypher; iv]

    let private fromString (str : string) =
        match str.Split '@' with
        | [| cypher; iv |] -> (cypher, iv) |> Ok
        | _ -> "Incorrectly encoded cypher" |> exn |> Error

    let encrypt key plainText =
        let aes = encryptAlgorithm key
        let iv = convertToString aes.IV
        let key =
            let bytes = encodingToArray plainText
            let encryptor = aes.CreateEncryptor ()
            use buffer = new MemoryStream ()
            use stream = new CryptoStream(buffer, encryptor, CryptoStreamMode.Write)
            stream.Write(bytes, 0, bytes.Length)
            stream.FlushFinalBlock ()
            convertToString (buffer.ToArray ())
        (key, iv)
        |> toString

    let decrypt key cypher =
        use aes = encryptAlgorithm key
        let parsedCypher = fromString cypher

        let mk (cypher, iv) =
            aes.DecryptCbc(convertToArray cypher, convertToArray iv)
            |> encodingToString
            
        parsedCypher
        |> Result.map mk



