using System;
using System.Collections.Generic;
using System.Text;
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

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(Stage + "\t");
            builder.Append(Description + "\t");

            return builder.ToString();
        }

    }
}