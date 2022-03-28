namespace Finance.Model.Investment

open System

type OrderId = OrderId of Guid
type SellTransaction = Transaction*decimal

type Order =
    { OrderId : OrderId
      BuyTransaction : Transaction
      SellTransactions : SellTransaction[] }

