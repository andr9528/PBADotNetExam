using Main.Domain.Enums;
using Shared.Repository.Core;

namespace Main.Domain.Core
{
    public interface IRollbackData : IEntity
    {
        Services Service { get; set; }
        Action Action { get; set; }

        /// <summary>
        /// The string number indicating the Account, Item or what ever is being changed
        /// </summary>
        string Number { get; set; }
    }
}