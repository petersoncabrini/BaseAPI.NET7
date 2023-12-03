using CustomBackend.Domain.Common.Models;

namespace CustomBackend.Domain.Configuracoes.Models
{
    public class Configuracao : EntityBase
    {
        public Configuracao() : base()
        {

        }

        public virtual string SmtpSender { get; set; }
        public virtual string SmtpHost { get; set; }
        public virtual string SmtpUser { get; set; }
        public virtual int SmtpPort { get; set; }
        public virtual bool SmtpEnableSsl { get; set; } = true;
        public virtual string SmtpPassword { get; set; }

        public virtual string UrlDoSistema { get; set; }

        public virtual string NomeDoRepositorioDeArquivos { get; set; } = "CustomBackendFiles";
        public virtual string IdDoGoogleAnalytics { get; set; }

        public virtual string TokenSecret { get; set; } = "6DF4168E-9F4F-4193-8016-C8BF1C26154E";
        public virtual int TokenTimeoutInMinutes { get; set; } = 300;
        public virtual int RefreshTokenTimeoutInMinutes { get; set; } = 600;

        public virtual string LogoUrl { get; set; }
        public virtual string LogoNegativaUrl { get; set; }
        public virtual string ImagemPadraoUrl { get; set; }
        public virtual string CapaPadraoUrl { get; set; }
        public virtual string BackgroundUrl { get; set; }
        public virtual string IconeUrl { get; set; }

        public virtual string DebugFrontColor { get; set; } = "white";
        public virtual string DebugBackColor { get; set; } = "red";
        public virtual string DebugTitle { get; set; } = "Homologação";
        public virtual string EmailsDeDestinosDoDebug { get; set; }
        public virtual bool IsDebug { get; set; } = true;
    }
}
