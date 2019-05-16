using System.Collections.Generic;
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
    }
}