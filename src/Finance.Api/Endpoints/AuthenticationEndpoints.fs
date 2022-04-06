namespace Finance.Api.Endpoints

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Threading.Tasks
open FSharp.Core
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.Authentication
open Finance.FSharp
open Finance.Model
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.IdentityModel.JsonWebTokens
open Microsoft.IdentityModel.Tokens

[<RequireQualifiedAccess>]
module Authentication =
    let private authenticate (authenticationContext : AuthenticationContext) (login : LoginDto) =
        task{
            let mkToken (user : User) =
                let claims =
                    [| Claim(JwtRegisteredClaimNames.Sub, (deconstruct user.Username))
                       Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
                let expires = Nullable(DateTime.UtcNow.AddHours(8.0))
                let notBefore = Nullable(DateTime.UtcNow)
                let secret =
                    authenticationContext.Key
                    |> encodingGetBytes
                    |> SymmetricSecurityKey
                let signingCredentials = SigningCredentials(key = secret, algorithm = SecurityAlgorithms.HmacSha512)

                let token =
                    JwtSecurityToken(
                        issuer = authenticationContext.Issuer,
                        audience = authenticationContext.Audience,
                        claims = claims,
                        expires = expires,
                        notBefore = notBefore,
                        signingCredentials = signingCredentials)
                { AuthenticationDto.Token = JwtSecurityTokenHandler().WriteToken(token)
                  ExpirationDate = expires |> Option.ofNullable }

            return!
                authenticationContext.Authenticate (login.Username |> Username) login.Password
                |> AsyncResult.map mkToken
                |> IResults.ok
        }

    let registerEndpoint (authenticationContext : AuthenticationContext) (app : WebApplication) =
        app.MapPost("/api/login", Func<LoginDto, Task<IResult>>(fun login -> authenticate authenticationContext login))
            .AllowAnonymous()
            .WithTags("Authentication") |> ignore
        app