open System
open Finance.Api.Endpoints
open Finance.Api.Settings
open Finance.Application.Authentication
open Finance.Application.Broker
open Finance.Application.FileImporter
open Finance.Application.Dividend
open Finance.Application.Ticker
open Finance.Application.Transaction
open Finance.Application.BankTransaction
open Finance.FSharp
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.HttpLogging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open Microsoft.OpenApi.Models

let configureSettings (configurationBuilder: IConfigurationBuilder) =
    configurationBuilder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", false)
                        .AddUserSecrets<Settings>()
[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder
        .Logging
        .ClearProviders()
        .AddConsole() |> ignore

    let configurationBuilder = ConfigurationBuilder() |> configureSettings
    let settings = configurationBuilder.Build().Get<Settings>()

    let degiroContext = FileImporterContext.create settings.SqlConnectionString
    let dividendContext = DividendContext.Create settings.SqlConnectionString
    let tickerContext = TickerContext.create settings.SqlConnectionString
    let brokerContext = BrokerContext.create settings.SqlConnectionString
    let transactionContext = ApiTransactionContext.create settings.SqlConnectionString
    let bankContext = ApiBankTransactionContext.create settings.SqlConnectionString settings.SecretId settings.SecretKey
    let authenticationContext = AuthenticationContext.create settings.SqlConnectionString settings.Jwt.Issuer settings.Jwt.Audience settings.Jwt.Key

    let services = builder.Services

    let secret =
        settings.Jwt.Key
        |> encodingGetBytes
        |> SymmetricSecurityKey

    let tokenValidationParameters =
        TokenValidationParameters(
            ValidateActor = true,
            ValidateAudience = true,
            ValidIssuer = settings.Jwt.Issuer,
            ValidAudience = settings.Jwt.Audience,
            IssuerSigningKey = secret )

    let securityScheme =
        OpenApiSecurityScheme(
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security")

    let securityReq = OpenApiSecurityRequirement()
    securityReq.Add(
        OpenApiSecurityScheme(
            Reference = OpenApiReference(
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer" ))
        ,Array.empty)

    let contact =
        OpenApiContact(
            Name = "Tiago Rodrigues",
            Email = "hi@tiagorodrigues.me",
            Url = Uri("https://tiagorodrigues.me"))

    let license =
        OpenApiLicense(
            Name = "Free License",
            Url = Uri("https://tiagorodrigues.me"))

    let openApiInfo =
        OpenApiInfo(
            Version = "v1",
            Title = "Finance - API",
            Description = "Finance API",
            TermsOfService = Uri("http://www.example.com"),
            Contact = contact,
            License = license )

    services
        .AddHttpLogging(fun logging ->
            logging.LoggingFields <-
                HttpLoggingFields.RequestHeaders
                ||| HttpLoggingFields.RequestMethod
                ||| HttpLoggingFields.RequestPath
                ||| HttpLoggingFields.RequestProtocol
                ||| HttpLoggingFields.RequestQuery
                ||| HttpLoggingFields.ResponseStatusCode
                ||| HttpLoggingFields.ResponseHeaders)
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(fun opt ->
            opt.SwaggerDoc("v1", openApiInfo)
            opt.AddSecurityDefinition("Bearer", securityScheme)
            opt.AddSecurityRequirement(securityReq))
        .AddCors(fun options ->
                        options.AddPolicy("AllowAll", fun builder ->
                             builder.AllowAnyHeader()
                                    .AllowAnyOrigin()
                                    .AllowAnyMethod() |> ignore))
        .AddAuthorization(fun opt ->
            opt.FallbackPolicy <-
                AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build()
            )
        .AddAuthentication(fun opt ->
            opt.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            opt.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
            opt.DefaultScheme <- JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun opt ->
            opt.TokenValidationParameters <- tokenValidationParameters )|> ignore

    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseSwagger()
           .UseSwaggerUI() |> ignore

    app.UseHttpsRedirection()
       .UseHttpLogging()
       .UseCors("AllowAll")
       .UseAuthentication()
       .UseAuthorization() |> ignore

    app
    |> Authentication.registerEndpoint authenticationContext
    |> Ticker.registerEndpoint tickerContext
    |> Broker.registerEndpoint brokerContext
    |> Transaction.registerEndpoint transactionContext degiroContext
    |> Dividend.registerEndpoint dividendContext
    //|> Bank.registerEndpoint bankContext
    |> ignore

    let log = app.Logger

    log.LogInformation($"Application Name: {builder.Environment.ApplicationName}");
    log.LogInformation($"Environment Name: {builder.Environment.EnvironmentName}");
    log.LogInformation($"ContentRoot Path: {builder.Environment.ContentRootPath}");
    
    app.Run()

    0 // Exit code

