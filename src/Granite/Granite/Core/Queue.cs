using System.Collections.Generic;

namespace Granite.Core
{
    public class Queue : BaseEntity
    {
        public Queue()
        {
            Items = new List<QueueItem>();
        }
        
        public virtual string Name { get; set; }
        public virtual IList<QueueItem> Items { get; set; }
    }
}