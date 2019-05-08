using Service.Banking.Domain.Core;

namespace Service.Banking.Domain.Concrete
{
    public class Account : IAccount
    {
        public double Balance { get; set; }
        public IPerson Owner { get; set; }
        public string AccountNumber { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}