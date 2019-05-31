using System.Collections.Generic;
using Service.Ordering.Domain.Core;
using Service.Ordering.Domain.Enums;

namespace Service.Ordering.Domain.Concrete
{
    public class Order : IOrder
    {
        public string OrderNumber { get; set; }
        public ICollection<IItem> Items { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public OrderStage Stage { get; set; }
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
    }
}