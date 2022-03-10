namespace Finance.HttpClient.Nordigen

open System.Net.Http
open Finance.HttpClient

module HttpClient =
    [<Literal>]
    let BaseUrl = "https://ob.nordigen.com/api/v2/"
    
    let login (client : HttpClient) body =

        failwith ""
    