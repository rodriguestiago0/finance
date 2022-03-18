
open System
open System.IO
open Finance.Service
open Finance.Service.Settings
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration.UserSecrets


[<EntryPoint>]
let main argv =
    let hostConfig (hCfg:IConfigurationBuilder) =
        hCfg.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings", optional = true)
            .AddCommandLine(argv)
            |> ignore

    let appConfig (host: HostBuilderContext) (aCfg: IConfigurationBuilder) =
        aCfg.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional = false)
            .AddJsonFile( $"appsettings.%s{host.HostingEnvironment.EnvironmentName}.json", optional = true)
            .AddUserSecrets<Settings>()
            .AddCommandLine(argv)
            |> ignore

    let serviceConfig (ctx: HostBuilderContext) (sCfg: IServiceCollection) =

        sCfg.AddLogging()
            //.AddHostedService<CloseTransactionWorker>()
            .AddHostedService<BankTransactionWorker>()
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