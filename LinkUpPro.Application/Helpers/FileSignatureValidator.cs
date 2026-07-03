using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.Helpers
{
    // Verifica el contenido real (magic bytes) de un archivo subido, en vez de
    // confiar únicamente en su extensión. Evita que un archivo renombrado con
    // una extensión de imagen válida pero con otro contenido sea aceptado.
    public static class FileSignatureValidator
    {
        public static bool HasValidImageSignature(IFormFile? file)
        {
            if (file == null || file.Length < 12)
            {
                return false;
            }

            var header = new byte[12];

            using (var stream = file.OpenReadStream())
            {
                var bytesRead = 0;

                while (bytesRead < header.Length)
                {
                    var read = stream.Read(header, bytesRead, header.Length - bytesRead);

                    if (read == 0)
                    {
                        break;
                    }

                    bytesRead += read;
                }

                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                if (bytesRead < header.Length)
                {
                    return false;
                }
            }

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return true;
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return true;
            }

            // WEBP: "RIFF"<4 bytes de tamaño>"WEBP"
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            {
                return true;
            }

            return false;
        }
    }
}
