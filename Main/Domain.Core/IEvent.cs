using System.Collections.Generic;
using Main.Domain.Enums;
using Shared.Repository.Core;

namespace Main.Domain.Core
{
    public interface IEvent : IEntity
    {
        string Description { get; set; }
        EventStage Stage { get; set; }
        ICollection<IRollbackData> RollbackDatas { get; set; }
    }
}