using Main.Domain.Enums;
using Shared.Repository.Core;

namespace Main.Domain.Core
{
    public interface IRollbackData : IEntity
    {
        Services Service { get; set; }
        Action Action { get; set; }
        string Number { get; set; }
        double Value { get; set; }
        int FK_Event { get; set; }
        IEvent Event { get; set; }

    }
}