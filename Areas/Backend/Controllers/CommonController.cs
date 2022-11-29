using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BASE.Areas.Backend.Controllers
{
    public class CommonController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private IHostingEnvironment Environment;
        public CommonController(AllCommonService allCommonService,
                                IHostingEnvironment _environment)
        {
            _allCommonService = allCommonService;
            Environment = _environment;
        }

        /// <summary> 前端連結取得檔案function
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string filePath, string fileName)
        {
            string decrypt_fileName = EncryptService.AES.Decrypt(fileName);

            //Determine the Content Type of the File.
            string contentType = "";
            FileExtensionContentTypeProvider providers = new FileExtensionContentTypeProvider();

            //-- openoffice content type cant not found,must manual Mappings
            providers.Mappings.Add(".odt", "application/vnd.oasis.opendocument.text");

            providers.TryGetContentType(decrypt_fileName, out contentType);

            //Build the File Path.
            string path = Path.Combine(this.Environment.WebRootPath, filePath) + decrypt_fileName;
            var file = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(file, contentType, decrypt_fileName);
        }
    }
}
