using Service.Ordering.Domain.Enums;
using Shared.Repository.Core;

namespace Service.Ordering.Domain.Core
{
    public interface IItem : IEntity
    {
        string ItemNumber { get; set; }
        string Name { get; set; }
        double Price { get; set; }
        int Amount { get; set; }
        string Description { get; set; }
        ItemPosition Position { get; set; }
        IOrder Order { get; set; }
        int? FK_Order { get; set; }
    }
}