using Service.Ordering.Domain.Core;

namespace Main.Domain.Proxies
{
    public class ItemProxy : IItem
    {
        public string ItemNumber { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}