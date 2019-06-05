using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Service.Ordering.Domain.Core;
using Service.Ordering.Domain.Enums;

namespace Main.Domain.Proxies
{
    public class OrderProxy : IOrder
    {
        public string OrderNumber { get; set; }
        public ICollection<IItem> Items { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public OrderStage Stage { get; set; }
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(OrderNumber + "\t");
            builder.Append(Stage + "\t");
            builder.Append(FromAccount + "\t");
            builder.Append(ToAccount + "\t");
            builder.Append(Items.Count + "\t");

            return builder.ToString();
        }

        [JsonConstructor]
        public OrderProxy(List<ItemProxy> items)
        {
            Items = new List<IItem>(items);
        }

        public OrderProxy()
        {
            Items = new List<IItem>();
        }
    }
}