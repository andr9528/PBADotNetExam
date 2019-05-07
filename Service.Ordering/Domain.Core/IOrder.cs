using Shared.Repository.Core;
using System.Collections.Generic;

namespace Service.Ordering.Domain.Core
{
    public interface IOrder : IEntity
    {
        string OrderNumber { get; set; }
        ICollection<IItem> Items { get; set; }
    }
}