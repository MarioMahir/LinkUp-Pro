namespace LinkUpPro.Shared.Services
{
    public class EmailSettings
    {
        /// <summary>Cuenta remitente (usuario SMTP). Vacío = modo desarrollo: los correos se guardan como archivos HTML.</summary>
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public string FromName { get; set; } = "LinkUp Pro";

        /// <summary>Carpeta (relativa a la raíz de la app) donde se escriben los correos cuando no hay SMTP configurado.</summary>
        public string OutputFolder { get; set; } = "App_Data/correos";

        public bool IsSmtpConfigured =>
            !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Email);
    }
}
