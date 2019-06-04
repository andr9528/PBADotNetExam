namespace Main.Domain.Enums
{
    public enum EventStage
    {
        Null, New, Completed, Rollbacked, TransferMoney, StockUpdate, InsuficientFunds,
        OutOfStock, MissingAccount, UpdateError, MissingItem, TotalFailure, UnknownError
    }


    /*
     * New -->
     *  TransferMoney -->
     *      InsuficientFunds -->
     *          Rollbacked --X --> TotalFailure
     *      MissingAccount -->
     *          Rollbacked --X --> TotalFailure
     *      UpdateError -->
     *          Rollbacked --X --> TotalFailure
     *      StockUpdate -->
     *          OutOfStock -->
     *              Rollbacked --X --> TotalFailure
     *          MissingItem -->
     *              Rollbacked --X --> TotalFailure
     *          Completed --X
     */
}