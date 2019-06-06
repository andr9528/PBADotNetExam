using Main.Domain.Core;
using Main.Domain.Enums;
using Newtonsoft.Json;
using System.Text;

namespace Main.Domain.Proxies
{
    public class RollbackDataProxy : IRollbackData
    {
        public Services Service { get; set; }
        public Action Action { get; set; }
        public string Number { get; set; }
        public double Value { get; set; }
        public int FK_Event { get; set; }
        public IEvent Event { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(string.Format("Id = {0}, ", Id));
            builder.Append(string.Format("Number = {0}, ", Number));
            builder.Append(string.Format("Value = {0}, ", Value));
            builder.Append(string.Format("Action = {0}, ", Action.ToString()));
            builder.Append(string.Format("Service = {0}", Service.ToString()));

            return builder.ToString();
        }

        [JsonConstructor]
        public RollbackDataProxy(EventProxy @event)
        {
            Event = @event;
        }

        public RollbackDataProxy()
        {

        }
    }
}