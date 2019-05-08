using System.Collections.Generic;
using Main.Domain.Core;
using Main.Domain.Enums;

namespace Main.Domain.Concrete
{
    public class Event : IEvent
    {
        public string Description { get; set; }
        public EventStage Stage { get; set; }
        public ICollection<IRollbackData> RollbackDatas { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}