using Shared.Repository.Core;

namespace Service.Banking.Domain.Core
{
    public interface IAccount : IEntity
    {
        double Balance { get; set; }
        IPerson Owner { get; set; }
        string AccountNumber { get; set; }
    }
}