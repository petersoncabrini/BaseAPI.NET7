namespace CustomBackend.Infra.Dtos.Request
{
    public record AuthByRefreshTokenRequest
    {
        public string Email { get; init; }
        public string RefreshToken { get; init; }
    }
}
