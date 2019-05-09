using Service.Banking.Domain.Core;

namespace Main.Domain.Proxies
{
    public class AccountProxy : IAccount
    {
        public double Balance { get; set; }
        public IPerson Owner { get; set; }
        public string AccountNumber { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
    }
}