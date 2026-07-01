using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace LinkUpPro.Application.Attributes
{
    public class ValidateFileAttribute : ValidationAttribute
    {
        private readonly int _maxFileSizeMb;
        private readonly string[] _allowedExtensions;

        public ValidateFileAttribute(int maxFileSizeMb, string[] allowedExtensions)
        {
            _maxFileSizeMb = maxFileSizeMb;
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"El formato de la imagen no es válido. Formatos permitidos: {string.Join(", ", _allowedExtensions)}.");
                }

                var fileSizeMb = file.Length / 1024f / 1024f;
                if (fileSizeMb > _maxFileSizeMb)
                {
                    return new ValidationResult($"El tamaño de la imagen excede el límite máximo de {_maxFileSizeMb} MB.");
                }
            }

            return ValidationResult.Success;
        }
    }
}