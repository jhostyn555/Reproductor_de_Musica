using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EcommerceApp.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<(string Url, string PublicId)> SubirAudioAsync(IFormFile archivo, string carpeta = "musicas/audio")
        {
            await using var stream = archivo.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(archivo.FileName, stream),
                Folder = carpeta,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new InvalidOperationException(result.Error.Message);
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task<(string Url, string PublicId)> SubirImagenAsync(IFormFile archivo, string carpeta = "musicas/imagenes")
        {
            await using var stream = archivo.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(archivo.FileName, stream),
                Folder = carpeta,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Quality("auto")
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new InvalidOperationException(result.Error.Message);
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        /// <summary>
        /// Sube un audio desde una URL pública. Cloudinary descarga el archivo remoto
        /// y devuelve la misma forma (URL segura + PublicId) que la subida normal.
        /// </summary>
        public async Task<(string Url, string PublicId)> SubirAudioDesdeUrlAsync(string url, string carpeta = "musicas/audio")
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(url),
                Folder = carpeta,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new InvalidOperationException(result.Error.Message);
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        /// <summary>
        /// Sube una imagen desde una URL pública, aplicando la misma transformación que la subida normal.
        /// </summary>
        public async Task<(string Url, string PublicId)> SubirImagenDesdeUrlAsync(string url, string carpeta = "musicas/imagenes")
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(url),
                Folder = carpeta,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Quality("auto")
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
                throw new InvalidOperationException(result.Error.Message);
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task EliminarRecursoAsync(string publicId, ResourceType resourceType = ResourceType.Image)
        {
            var deleteParams = new DeletionParams(publicId) { ResourceType = resourceType };
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
