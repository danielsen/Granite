using System;

namespace Granite.Core
{
    public class DequeueResult<T>
    {
        public Guid AcknowledgeId { get; set; }
        public T Data { get; set; }
    }
}