using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Granite.Infrastructure.Common.Data.Orm;

namespace Granite.Core
{
    public class BaseEntity : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime CreatedUtc { get; set; }

        [NotMapped]
        [Display(Name = "Created")]
        public DateTime Created => CreatedUtc.ToLocalTime();
        
        public DateTime? ModifiedUtc { get; set; }

        [NotMapped]
        [Display(Name = "Modified")]
        public DateTime? Modified => ModifiedUtc?.ToLocalTime() ?? null;
    }
}