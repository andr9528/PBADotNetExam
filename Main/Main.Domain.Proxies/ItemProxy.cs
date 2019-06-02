using Service.Ordering.Domain.Core;
using Service.Ordering.Domain.Enums;
using System.Text;

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
        public ItemPosition Position { get; set; }
        public IOrder Order { get; set; }
        public int? FK_Order { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(ItemNumber + "\t");
            builder.Append(Name + "\t");
            builder.Append(Price + "\t");
            builder.Append(Amount + "\t");
            builder.Append(Description + "\t");

            return builder.ToString();
        }
    }
}