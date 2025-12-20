using Auth.Contarcts;

namespace Auth.Services
{
    public class ImageHelper : IImageHelper
    {
        #region DI
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        public ImageHelper(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        public bool DeleteImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            relativePath = relativePath.Replace("\\", "/").TrimStart('/');
            var fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }

        public string GetImageUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

          
            if (Uri.IsWellFormedUriString(relativePath, UriKind.Absolute))
                return relativePath;

            relativePath = relativePath.Replace("\\", "/").TrimStart('/');

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return "/" + relativePath;             

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}/{relativePath}";
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string subFolder)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Image file is required.");

            var fileExtension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            
            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRootPath, "Uploads", "Images", subFolder);

            Directory.CreateDirectory(folderPath); 

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

           
            return Path.Combine("Uploads", "Images", subFolder, fileName).Replace("\\", "/");
        }
    }
}
