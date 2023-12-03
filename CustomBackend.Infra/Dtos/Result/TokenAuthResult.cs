namespace CustomBackend.Infra.Dtos.Result
{
    public record TokenAuthResult
    {
        public string Token { get; init; }
        public string TokenBearer { get => $"Bearer {Token}"; }
    }
}
