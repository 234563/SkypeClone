using Application.Interfaces.FileHandling;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IFileHelper _fileHelper;

        public ImagesController(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadDto request)
        {
            try
            {
                var relativePath =  _fileHelper.SaveBase64ToFile(
                    request.Base64String,
                    "File",
                    request.SubFolder);

                var fileUrl = _fileHelper.GetFileUrl(relativePath);

                return Ok(new
                {
                    Path = relativePath,
                    Url = fileUrl,
                    Message = "Image saved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }
    }

    public class ImageUploadDto
    {
        public string Base64String { get; set; }
        public string FileName { get; set; }
        public string SubFolder { get; set; } = "general";
    }
}
