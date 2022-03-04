namespace Finance.Service

open System
open System.Threading
open System.Threading.Tasks
open Finance.FSharp
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Finance.Application.Transaction

type CloseTransactionWorker(logger : ILogger<CloseTransactionWorker>, configuration : IConfiguration ) =
    let _logger = logger
    let _configuration = configuration
    member val timer: Timer option = None with get, set
    member val cancellationToken: CancellationToken option = None with get, set

    interface IHostedService with
        member this.StartAsync(cancellationToken: CancellationToken) =
            "Worker Service running" |> _logger.LogInformation
            let context = TransactionContext.Create _configuration["SqlConnectionString"] _logger
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
            "Start Calculating Close Transactions" |> _logger.LogInformation
            let context = state :?> TransactionContext
            return!
                Transaction.processCloseTransaction context
                |> AsyncResult.map (fun _ -> ())
                |> AsyncResult.tee (fun _ -> "Done Calculating Close Transactions" |> _logger.LogInformation)
        }
        |> Async.RunSynchronously
        |> ignore
        ()

