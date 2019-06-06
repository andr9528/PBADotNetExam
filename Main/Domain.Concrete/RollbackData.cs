using System.Text;
using Main.Domain.Core;
using Main.Domain.Enums;
using Newtonsoft.Json;

namespace Main.Domain.Concrete
{
    public class RollbackData : IRollbackData
    {
        public Services Service { get; set; }
        public Action Action { get; set; }
        public string Number { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public double Value { get; set; }
        public int FK_Event { get; set; }
        public IEvent Event { get; set; }

        [JsonConstructor]
        public RollbackData(Event @event)
        {
            Event = @event;
        }

        public RollbackData()
        {
            
        }
        
    }
}