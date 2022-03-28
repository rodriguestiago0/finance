open System
open Finance.Api.Endpoints
open Finance.Api.Settings
open Finance.Application.Broker
open Finance.Application.Degiro
open Finance.Application.Dividend
open Finance.Application.Ticker
open Finance.Application.Transaction
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.HttpLogging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

let configureSettings (configurationBuilder: IConfigurationBuilder) =
    configurationBuilder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", false)

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder
        .Logging
        .ClearProviders()
        .AddConsole() |> ignore

    let configurationBuilder = ConfigurationBuilder() |> configureSettings
    let settings = configurationBuilder.Build().Get<Settings>()

    let degiroContext = DegiroContext.create settings.SqlConnectionString
    let dividendContext = DividendContext.Create settings.SqlConnectionString
    let tickerContext = TickerContext.create settings.SqlConnectionString
    let brokerContext = BrokerContext.create settings.SqlConnectionString
    let transactionContext = ApiTransactionContext.create settings.SqlConnectionString
    let services = builder.Services

    services.AddHttpLogging(fun logging -> logging.LoggingFields <- HttpLoggingFields.Request )
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddCors(fun options ->
                            options.AddPolicy("AllowAll", fun builder ->
                                 builder.AllowAnyHeader()
                                        .AllowAnyOrigin()
                                        .AllowAnyMethod() |> ignore)) |> ignore

    let app = builder.Build()

    app.UseHttpsRedirection()
       .UseHttpLogging()
       .UseCors("AllowAll") |> ignore

    Ticker.registerEndpoint app tickerContext
    Broker.registerEndpoint app brokerContext
    Transaction.registerEndpoint app transactionContext degiroContext
    Dividend.registerEndpoint app dividendContext

    if app.Environment.IsDevelopment() then
        app.UseSwagger()
           .UseSwaggerUI() |> ignore;

    let log = app.Logger

    log.LogInformation($"Application Name: {builder.Environment.ApplicationName}");
    log.LogInformation($"Environment Name: {builder.Environment.EnvironmentName}");
    log.LogInformation($"ContentRoot Path: {builder.Environment.ContentRootPath}");
    
    app.Run()

    0 // Exit code

