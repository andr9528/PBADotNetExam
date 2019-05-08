using Main.Domain.Core;
using Main.Domain.Enums;

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
    }
}