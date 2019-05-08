using System.Collections.Generic;
using Service.Ordering.Domain.Core;

namespace Service.Ordering.Domain.Concrete
{
    public class Order : IOrder
    {
        public string OrderNumber { get; set; }
        public ICollection<IItem> Items { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}