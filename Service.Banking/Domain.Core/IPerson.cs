using Shared.Repository.Core;
using System.Collections.Generic;

namespace Service.Banking.Domain.Core
{
    public interface IPerson : IEntity
    {
        string Name { get; set; }
        string Address { get; set; }
        ICollection<IAccount> Accounts { get; set; }
    }
}