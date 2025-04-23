namespace SimplePresave.Libraries.Model
{
    public class SpotifySettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public string TrackId { get; set; }
    }
}
