namespace Rumbi.Data.Models
{
    public class User
    {
        public ulong Id { get; set; }

        public string Username { get; set; }
        public ulong ColorRoleId { get; set; }
        public uint Color { get; set; }
    }
}
