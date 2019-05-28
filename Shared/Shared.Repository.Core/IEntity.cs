using System;

namespace Shared.Repository.Core
{
    public interface IEntity
    {
        int Id { get; set; }
        byte[] Version { get; set; }
    }
}
