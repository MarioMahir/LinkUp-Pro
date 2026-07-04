using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.Helpers
{
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

            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return true;
            }

            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return true;
            }

            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            {
                return true;
            }

            return false;
        }
    }
}
