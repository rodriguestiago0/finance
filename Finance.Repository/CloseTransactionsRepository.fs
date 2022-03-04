namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module CloseTransactionsRepository =

    let createCloseTransactions connectionString (closeTransactions : seq<CloseTransaction>) =
        async {
            try
                let data =
                    closeTransactions
                    |> Seq.map CloseTransactionDto.ofDomain
                    |> Seq.map (fun closeTransaction ->
                            [ "@BuyTransactionId", Sql.int closeTransaction.BuyTransactionId
                              "@SellTransactionId", Sql.int closeTransaction.SellTransactionId
                              "@Units", Sql.decimal closeTransaction.Units ]
                        )
                    |> List.ofSeq
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync [ "INSERT INTO
                            Closed_Transaction (buy_transaction_id, sell_transaction_id, units)
                            VALUES (@BuyTransactionId, @SellTransactionId, @Units)", data]
                    |> Async.AwaitTask
                return Ok (List.sum result)
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions
