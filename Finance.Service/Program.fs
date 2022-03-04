
open System
open System.IO
open Finance.Service
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging


[<EntryPoint>]
let main argv =
    let hostConfig (hCfg:IConfigurationBuilder) =
        hCfg.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings", optional = true)
            //.AddEnvironmentVariables(prefix = "PREFIX_")
            .AddCommandLine(argv)
            |> ignore

    let appConfig (host: HostBuilderContext) (aCfg: IConfigurationBuilder) =
        aCfg.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional = false)
            .AddJsonFile( $"appsettings.%s{host.HostingEnvironment.EnvironmentName}.json", optional = true)
            //.AddEnvironmentVariables(prefix =  "PREFIX_")
            .AddCommandLine(argv)
            |> ignore

    let serviceConfig (ctx: HostBuilderContext) (sCfg: IServiceCollection) =

        sCfg.AddLogging()
            .AddHostedService<CloseTransactionWorker>()
            |> ignore

    let loggingConfig (_: HostBuilderContext) (lCfg: ILoggingBuilder) =
        lCfg.AddConsole()
            .AddDebug()
            |> ignore

    let host =
        HostBuilder()
            .ConfigureHostConfiguration(Action<IConfigurationBuilder> hostConfig)
            .ConfigureAppConfiguration(Action<HostBuilderContext,IConfigurationBuilder> appConfig)
            .ConfigureServices(Action<HostBuilderContext,IServiceCollection> serviceConfig)
            .ConfigureLogging(Action<HostBuilderContext,ILoggingBuilder> loggingConfig)
            .Build()

    host.RunAsync() |> Async.AwaitTask |> Async.RunSynchronously

    0 // return an integer exit code