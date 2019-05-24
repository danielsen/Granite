using System;

namespace Granite.Core
{
    public class Options<T>
    {
        /// <summary>
        /// By default if a messages is not acknowledged within 30 seconds of
        /// receipt, it is placed back in the queue so it can be fetched again.
        /// </summary>
        public TimeSpan Visibility { get; set; }
        
        /// <summary>
        /// When a message is queued it is typically available for immediate
        /// retrieval. However if a delay is desired it can be configured
        /// here.
        /// </summary>
        public TimeSpan Delay { get; set; }
        
        public IPersistedQueue<T> DeadQueue { get; set; }
        
        /// <summary>
        /// This option is only available when a dead queue is specified. This
        /// option sets a limit on the numer of times an item will be consumed
        /// from a queue. Once this limit is reached without acknowledgement
        /// of the message, the queue item will be placed on the dead queue.
        /// </summary>
        public int MaxRetries { get; set; }
    }
}