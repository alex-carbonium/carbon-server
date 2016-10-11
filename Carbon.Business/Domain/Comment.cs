using System;
using Carbon.Framework.Attributes;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public enum CommentType
    {
        Comment,
        Status
    }

    public enum CommentStatus
    {
        Opened,
        Resolved
    }

    public partial class Comment : DomainObject
    {
        public virtual Guid Uid { get; set; }
        public virtual Guid ParentUid { get; set; }
        
        [Required]
        public virtual string Text { get; set; }
        [Required]
        public virtual User User { get; set; }
        
        public virtual DateTime DateTime { get; set; }
        public virtual Project Project { get; set; }
        public virtual CommentType CommentType { get; set; }
        public virtual CommentStatus Status { get; set; }
        public virtual string PageId { get; set; }
        public virtual int? PageX { get; set; }
        public virtual int? PageY { get; set; }
        public virtual int? Number { get; set; }
    }
}
