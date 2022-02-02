open System
open Finance.Api.Endpoints
open Finance.Api.Settings
open Finance.Application.Degiro
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

let configureSettings (configurationBuilder: IConfigurationBuilder) =
    configurationBuilder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", false)

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let configurationBuilder = ConfigurationBuilder() |> configureSettings
    let settings = configurationBuilder.Build().Get<Settings>()
    
    let degiroContext = DegiroContext.Create settings.SqlConnectionString
    let services = builder.Services
    
    services.AddEndpointsApiExplorer() |> ignore
    services.AddSwaggerGen() |> ignore
    
    let app = builder.Build()

    Degiro.registerEndpoint app degiroContext |> ignore
    
    if app.Environment.IsDevelopment() then
        app.UseSwagger() |> ignore;
        app.UseSwaggerUI() |> ignore;
    
    app.UseHttpsRedirection() |> ignore;
    
    app.Run()

    0 // Exit code

