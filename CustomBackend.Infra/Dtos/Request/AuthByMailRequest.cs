namespace CustomBackend.Infra.Dtos.Request
{
    public record AuthByMailRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
