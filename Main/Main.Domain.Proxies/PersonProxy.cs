using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Service.Banking.Domain.Core;

namespace Main.Domain.Proxies
{
    public class PersonProxy : IPerson
    {
        public string PersonNumber { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<IAccount> Accounts { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Id + "\t");
            builder.Append(PersonNumber + "\t");
            builder.Append(Name + "\t");
            builder.Append(Address + "\t");

            return builder.ToString();
        }

        [JsonConstructor]
        public PersonProxy(List<AccountProxy> accounts)
        {
            Accounts = new List<IAccount>(accounts);
        }

        public PersonProxy()
        {
            Accounts = new List<IAccount>();   
        }
    }
}