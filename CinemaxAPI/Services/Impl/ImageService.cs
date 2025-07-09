namespace CinemaxAPI.Services.Impl
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public ImageService(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }
        public async Task<string> Upload(IFormFile file)
        {
            // construct the upload path
            var uploadPath = Path.Combine(_hostEnvironment.ContentRootPath, "Images");

            // check if the directory exists, if not create it
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // construct the full file path
            string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadPath, newFileName);

            // save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // newFileName.jpeg
            return newFileName;
        }
    }
}
