open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection;

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let services = builder.Services
    
    services.AddEndpointsApiExplorer() |> ignore
    services.AddSwaggerGen() |> ignore
    
    let app = builder.Build()

    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore
    
    if app.Environment.IsDevelopment() then
        app.UseSwagger() |> ignore;
        app.UseSwaggerUI() |> ignore;
    
    app.UseHttpsRedirection() |> ignore;
    
    app.Run()

    0 // Exit code
