namespace Carbon.Framework.Models
{
    public interface IDomainObject<TKey>
    {
        TKey Id { get;  set; }
    }
}
