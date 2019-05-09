using System.Collections.Generic;
using Service.Ordering.Domain.Core;

namespace Main.Domain.Proxies
{
    public class OrderProxy : IOrder
    {
        public string OrderNumber { get; set; }
        public ICollection<IItem> Items { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}