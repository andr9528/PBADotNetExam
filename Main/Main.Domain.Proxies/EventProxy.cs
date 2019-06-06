using System.Collections.Generic;
using System.Text;
using Main.Domain.Core;
using Main.Domain.Enums;
using Newtonsoft.Json;

namespace Main.Domain.Proxies
{
    public class EventProxy : IEvent
    {
        public string OrderNumber { get; set; }
        public string DatasAsString { get; set; }
        public EventStage Stage { get; set; }
        public ICollection<IRollbackData> RollbackDatas { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(Stage + "\t");
            builder.Append(OrderNumber + "\t");

            return builder.ToString();
        }

        [JsonConstructor]
        public EventProxy(List<RollbackDataProxy> rollbackDatas)
        {
            RollbackDatas = new List<IRollbackData>(rollbackDatas);
        }


        public EventProxy()
        {
            RollbackDatas = new List<IRollbackData>();
        }
    }
}