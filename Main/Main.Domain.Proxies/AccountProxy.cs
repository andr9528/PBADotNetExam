using Newtonsoft.Json;
using Service.Banking.Domain.Core;
using System.Text;

namespace Main.Domain.Proxies
{
    public class AccountProxy : IAccount
    {
        public double Balance { get; set; }
        public IPerson Owner { get; set; }
        public string AccountNumber { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public int FK_Owner { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(AccountNumber + "\t");
            builder.Append(Balance + "\t");
            builder.Append(Owner.Name + "\t");

            return builder.ToString();
        }

        [JsonConstructor]
        public AccountProxy(PersonProxy owner)
        {
            Owner = owner;
        }

        public AccountProxy()
        {
            
        }
    }
}