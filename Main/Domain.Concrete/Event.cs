using System;
using System.Collections.Generic;
using System.Text;
using Main.Domain.Core;
using Main.Domain.Enums;
using Newtonsoft.Json;

namespace Main.Domain.Concrete
{
    public class Event : IEvent
    {
        public string DatasAsString { get; set; }
        public EventStage Stage { get; set; }
        public ICollection<IRollbackData> RollbackDatas { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public string OrderNumber { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(Stage + "\t");
            builder.Append(OrderNumber + "\t");
            builder.Append(DatasAsString + "\t");

            return builder.ToString();
        }

        [JsonConstructor]
        public Event(List<RollbackData> data)
        {
            if (data == null)
            {
                RollbackDatas = new List<IRollbackData>();
            }
            else
            {
                RollbackDatas = new List<IRollbackData>(data);
            }
        }
        

        public Event()
        {
            RollbackDatas = new List<IRollbackData>();
        }
    }
}