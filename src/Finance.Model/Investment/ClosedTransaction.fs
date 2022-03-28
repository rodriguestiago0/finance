namespace Finance.Model.Investment

type CloseTransaction =
    { BuyTransaction : Transaction
      SellTransaction : Transaction
      Units: decimal }