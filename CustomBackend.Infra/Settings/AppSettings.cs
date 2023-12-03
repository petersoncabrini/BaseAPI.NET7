namespace CustomBackend.Infra.Settings
{
    public class AppSettings
    {
        public string MainDbConnection { get; set; }
        public bool UpdateDatabase { get; set; }
        public JwtSetting JwtSetting { get; set; }
    }
}
