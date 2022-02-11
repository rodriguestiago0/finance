open Finance.Service
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

[<EntryPoint>]
let main _ =
    async {
        let hostBuilder = Host.CreateDefaultBuilder()
        hostBuilder.ConfigureServices(fun hostContext services ->
            services.AddHostedService<WorkerService>() |> ignore)
        |> ignore

        let host = hostBuilder.Build()
        let! _ = host.StartAsync() |> Async.AwaitTask

        return! host.WaitForShutdownAsync() |> Async.AwaitTask
    } |> Async.RunSynchronously
    
    0