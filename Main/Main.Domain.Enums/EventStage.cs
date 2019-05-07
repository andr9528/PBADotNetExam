namespace Main.Domain.Enums
{
    public enum EventStage
    {
        /// <summary>
        /// Property of this type has not been set yet.
        /// </summary>
        Null,

        /// <summary>
        /// Action has happened that created an event is newly created, further actions are to follow
        /// </summary>
        New,

        /// <summary>
        /// Action that created the event is done, no further actions follow
        /// </summary>
        Completed,

        /// <summary>
        /// Action failed to complete,  Rollback incoming
        /// </summary>
        Failed,

        /// <summary>
        /// Action has been Rolled back at the appropriate spots, no further actions follow
        /// </summary>
        Rollbacked, 
    }
}