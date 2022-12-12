using BASE.Areas.Backend.Models;
using BASE.Models;
using BASE.Models.DB;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;

namespace BASE.Areas.Backend.Controllers
{
    public class FileController : BaseController
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CK5_FileUpload()
        {
            var file = HttpContext.Request.Form.Files[0];
            IList<string> dangerous_ext = CommonParameters.dangerous_ext;
            CK5_UploadResponse Result = new CK5_UploadResponse();

            string ext = Path.GetExtension(file.FileName).Replace(".", "").ToLower();

            if (dangerous_ext.Contains(ext))
            {
                Result.uploaded = false;

                Result.error = new CK5_UploadError()
                {
                    message = "偵測到可能含攻擊資訊之檔案類型，請調整上傳檔案後重新上傳！"
                };
            }
            else
            {
                var result = await _fileService.FileUploadAsync(file, "CK5_Uploader", "CK5");

                if (result.IsSuccess)
                {
                    Result.uploaded = true;
                    // output 多加 domain，不然部份情況(最新消息作為信件發送)會造成無圖
                    Result.url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}" + result.FilePath;
                }
                else
                {
                    Result.uploaded = false;

                    Result.error = new CK5_UploadError()
                    {
                        message = "上傳失敗，失敗原因：" + result.Message
                    };
                }
            }

            return Json(Result);
        }

        public JsonResult GetFile(string FileID)
        {
            return Json(_fileService.GetFileInput(FileID));
        }

        public JsonResult FileDelete(string key)
        {
            return Json(new { key = key });
        }

        public async Task<IActionResult> FileDownload(string FileID)
        {
            // 取得該FileID的檔案路徑
            TbFileInfo tempResult = _fileService.Lookup<TbFileInfo>(ref _message).Where(x => x.FileId == FileID).FirstOrDefault();

            //真實檔案路徑
            string FileFullPath = _fileService.GetFileFullPath(tempResult.FilePath);

            ////讀取檔案
            MemoryStream memoryStream = new MemoryStream();
            using (var stream = new FileStream(FileFullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            //回傳出檔案
            return File(memoryStream, "application/zip", tempResult.FileName);
        }
    }
}
