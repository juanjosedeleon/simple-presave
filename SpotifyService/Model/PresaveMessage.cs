namespace SimplePresave.Libraries.Model
{
    public class PresaveMessage
    {
        public string UserEmail { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public string RefreshToken { get; set; }
    }
}
