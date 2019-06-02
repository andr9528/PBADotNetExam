using Service.Ordering.Domain.Core;
using Service.Ordering.Domain.Enums;

namespace Service.Ordering.Domain.Concrete
{
    public class Item : IItem
    {
        public string ItemNumber { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public ItemPosition Position { get; set; }
        public IOrder Order { get; set; }
        public int? FK_Order { get; set; }
    }
}