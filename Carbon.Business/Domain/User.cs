using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public class User : IDomainObject<string>
    {
        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public string Email { get; set; }                

        public static readonly User System = new User { FriendlyName = "system" };        
    }
}
