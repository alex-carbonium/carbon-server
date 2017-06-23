namespace Carbon.Business.Domain
{
    public class CompanyInfo
    {
        public string Name { get; set; }
        public string Logo { get; set; }

        public UserInfo Owner { get; set; }
    }

    public class UserInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
    }
}
