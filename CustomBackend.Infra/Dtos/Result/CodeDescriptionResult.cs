namespace CustomBackend.Infra.Dtos.Result
{
    public record CodeDescriptionResult
    {
        public string Code { get; init; }
        public string Description { get; init; }
    }
}
