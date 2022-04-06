namespace Finance.Api.Validation

open FSharp.Core
open Finance.Api.Helpers
open Finance.FSharp
open Finance.FSharp.Validation
open Microsoft.AspNetCore.Http

[<AutoOpen>]
module RequestValidation =

    type IValidator<'TModel> =
        abstract member Validate : unit -> ValidationResult<'TModel, seq<string>>

    type MethodHandler<'TDto> = 'TDto -> AsyncResult<IResult, exn>

    type RequestValidation<'TDto, 'TModel> =
        { Validation : Option<IValidator<'TModel>>
          Method : MethodHandler<'TDto> }

    type RequestValidationBuilder() =
        let defaultMethod _ =
            Results.NoContent() |> AsyncResult.retn

        member _.Yield _ =
            { RequestValidation.Method = defaultMethod
              Validation = None }

        [<CustomOperation("method")>]
        member __.Method (state, method) =
            { state with Method = method }

        [<CustomOperation("validation")>]
        member __.Validation (state, validation) =
            { state with Validation = validation }

        member _.Return(state) =
            let invokeMethod (dto : IValidator<'TDto>) =
                dto.Validate()
                |> ValidationResult.toResult
                |> Async.retn
                |> AsyncResult.bind state.Method
                |> AsyncResult.mapError IResults.handleException

            state.Validation
            |> Option.map invokeMethod

    let requestValidator = RequestValidationBuilder()