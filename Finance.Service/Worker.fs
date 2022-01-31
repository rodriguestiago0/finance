namespace Finance.Service

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

type WorkerService(_logger: ILogger<WorkerService>) =
    member val timer: Timer option = None with get, set
    member val cancellationToken: CancellationToken option = None with get, set

    interface IHostedService with
        member this.StartAsync(cancellationToken: CancellationToken) =
            "Worker Service running" |> _logger.LogInformation
            this.timer <- Some(new Timer(this.DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5.)))
            this.cancellationToken <- Some cancellationToken
            Task.CompletedTask

        member this.StopAsync(cancellationToken: CancellationToken) =
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

    member this.DoWork(state: Object) =
        "Worker Service is working." |> _logger.LogInformation