namespace SimplePresave.Libraries.Model
{
    public class TokenRequestDto
    {
        public string? AuthorizationCode { get; set; }
        public string? RedirectUri { get; set; }
        public decimal TimeOffset { get; set; } = 0;
    }
}
