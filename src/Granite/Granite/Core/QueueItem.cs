using System;

namespace Granite.Core
{
    public class QueueItem : BaseEntity
    {
        public QueueItem()
        {
            AcknowledgeId = Guid.NewGuid();
        }
        
        public virtual Guid AcknowledgeId { get; set; }
        public virtual int Attempts { get; set; }
        public virtual DateTime Available { get; set; }
        public virtual byte[] Payload { get; set; }
        public virtual DateTime? Acknowledge { get; set; }
        public virtual Queue Queue { get; set; }
        public virtual SerializationType SerializationType { get; set; }
    }
}