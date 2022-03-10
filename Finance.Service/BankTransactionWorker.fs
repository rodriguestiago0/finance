namespace Finance.Service

open System
open System.Threading
open System.Threading.Tasks
open Finance.FSharp
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Finance.Application.BankTransaction

type BankTransactionWorker(logger : ILogger<CloseTransactionWorker>, configuration : IConfiguration ) =
    let _logger = logger
    let _configuration = configuration
    member val timer: Timer option = None with get, set
    member val cancellationToken: CancellationToken option = None with get, set

    interface IHostedService with
        member this.StartAsync(cancellationToken: CancellationToken) =
            "Worker Service running" |> _logger.LogInformation
            let context = BankTransactionContext.create _configuration["SqlConnectionString"] _configuration["SecretId"] _configuration["SecretKey"] _logger
            this.timer <- Some(new Timer(this.DoWork, context, TimeSpan.Zero, TimeSpan.FromHours(6.)))
            this.cancellationToken <- Some cancellationToken
            Task.CompletedTask

        member this.StopAsync(_ : CancellationToken) =
            "Worker Service is stopping" |> _logger.LogInformation
            match this.timer with
            | Some(t) -> t.Change(Timeout.Infinite, 0) |> ignore
            | None -> ()
            Task.CompletedTask

    interface IDisposable with
        member this.Dispose() =
            match this.timer with
            | Some(t) -> t.Dispose()
            | None -> ()

    member this.DoWork(state : obj) =
        async{
            "Start fetching bank transactions" |> _logger.LogInformation
            let context = state :?> BankTransactionContext
            return!
                BankTransactionService.importTransactions context
                |> AsyncResult.map (konst ())
                |> AsyncResult.tee (fun _ -> "Done fetching bank transactions" |> _logger.LogInformation)
        }
        |> Async.RunSynchronously
        |> ignore
        ()

