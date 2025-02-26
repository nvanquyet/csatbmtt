namespace DesServer.Models
{
    public class Session(User user)
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public User User { get; set; } = user;
        public void EndSession() { }
    }
}