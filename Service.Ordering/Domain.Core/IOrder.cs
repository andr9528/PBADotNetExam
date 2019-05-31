using Service.Ordering.Domain.Enums;
using Shared.Repository.Core;
using System.Collections.Generic;

namespace Service.Ordering.Domain.Core
{
    public interface IOrder : IEntity
    {
        string FromAccount { get; set; }
        string ToAccount { get; set; }
        string OrderNumber { get; set; }
        ICollection<IItem> Items { get; set; }
        OrderStage Stage { get; set; }
    }
}