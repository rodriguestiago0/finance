namespace Finance.Api.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type DividendDto =
    { DividendId : Guid
      TickerId : Guid
      Value : decimal
      Taxes : Option<decimal>
      ReceivedAt : DateTimeOffset }
    with
        static member ofDomain (model : Dividend) : DividendDto =
            { DividendDto.DividendId = deconstruct model.DividendId
              TickerId = deconstruct model.TickerId
              Value = model.Value
              Taxes = model.Taxes
              ReceivedAt = model.ReceivedAt }

        static member toDomain (dto : DividendDto) : Dividend =
            { Dividend.DividendId = dto.DividendId |> DividendId
              TickerId = dto.TickerId |> TickerId
              Value = dto.Value
              Taxes = dto.Taxes
              ReceivedAt = dto.ReceivedAt }