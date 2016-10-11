namespace Carbon.Framework.Models
{
    public abstract class DomainObject : IDomainObject<int>
    {
        public virtual int Id { get; set; }        
    }
}