using System;

namespace Granite.Infrastructure.Common.Data.Orm
{
    public interface IEntity
    {
        Guid Id { get; set; }
        DateTime CreatedUtc { get; set; }
        DateTime? ModifiedUtc { get; set; }
    }
}