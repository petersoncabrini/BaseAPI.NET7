namespace CustomBackend.Infra.Settings
{
    public class JwtSetting
    {
        public string Secret { get; set; }
        public int TokenTimeoutInMinutes { get; set; }
        public int RefreshTokenTimeoutInMinutes { get; set; }
    }
}
