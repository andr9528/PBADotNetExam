using System.Collections.Generic;
using Service.Banking.Domain.Core;

namespace Service.Banking.Domain.Concrete
{
    public class Person : IPerson
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<IAccount> Accounts { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}