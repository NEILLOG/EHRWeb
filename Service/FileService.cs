using BASE.Areas.Backend.Models;
using BASE.Extensions;
using BASE.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BASE.Service
{
    public class FileService : ServiceBase
    {
        private string _message;
        //Session
        private readonly IHttpContextAccessor _contextAccessor;
        //appsetting
        private readonly IConfiguration _config;
        //web Environment(
        private readonly IWebHostEnvironment _webHostEnvironment;
        //lock 可設定多執行序
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private AllCommonService _allCommonService;

        public FileService(DBContext context,
                        IHttpContextAccessor httpContextAccessor,
                        IConfiguration configuration,
                        IWebHostEnvironment webHostEnvironment,
                        AllCommonService allCommonService) : base(context)
        {
            _contextAccessor = httpContextAccessor;
            _config = configuration;
            _webHostEnvironment = webHostEnvironment;
            _allCommonService = allCommonService;
        }

        /// <summary>
        /// 單檔案上傳
        /// </summary>
        /// <param name="File"></param>
        /// <param name="Folder"></param>
        /// <param name="FileDescription"></param>
        /// <param name="OldFileID"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<FileResultModel> FileUploadAsync(IFormFile File, string Folder, string? FileDescription = null, string? OldFileID = null, string? filename = null, IDbContextTransaction? transaction = null)
        {
            UserSessionModel? userinfo = _contextAccessor.HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            DateTime now = DateTime.Now;
            FileResultModel result = new FileResultModel();

            try
            {
                var fileRealName = this.GenerateFileID();

                // 預設的存檔路徑
                string fileUploadRoot = _config["Site:FileUploadRoot"];

                // 檔案存放路徑 (注意：檔案只能存在wwwroot底下)
                string path = MapPath(fileUploadRoot, Folder);

                // 建立目錄
                CreateDirectory(path);

                //存檔
                string extension = Path.GetExtension(File.FileName);//檔案類型
                string save = MapPath(fileUploadRoot, Folder, fileRealName + extension);

                using (FileStream stream = new FileStream(save, FileMode.Create))
                {
                    await File.CopyToAsync(stream);
                }

                //↓↓↓↓↓ Lock With Async ↓↓↓↓↓
                await _lock.WaitAsync();

                //資料庫紀錄
                string ralativepath = String.Join('/', fileUploadRoot, Folder, fileRealName + extension);
                //string MaxFileID = await Lookup<TbFileInfo>(ref _message).Select(x => x.FileId).MaxAsync();

                TbFileInfo Entity = new TbFileInfo();
                //Entity.FileId = _allCommonService.IDGenerator("FILE", 15, MaxFileID);
                Entity.FileId = await _allCommonService.IDGenerator<TbFileInfo>();
                Entity.FileName = String.IsNullOrEmpty(filename) ? File.FileName : filename;
                Entity.FileRealName = fileRealName;
                Entity.FileDescription = string.IsNullOrEmpty(FileDescription) ? string.Empty : FileDescription;
                Entity.FilePath = ralativepath;
                Entity.Order = 1;
                Entity.IsDelete = false;
                Entity.CreateUser = userinfo == null ? "" : userinfo.UserID;
                Entity.CreateDate = now;
                var InsertResult = await Insert<TbFileInfo>(Entity, transaction);

                _lock.Release();
                //↑↑↑↑↑ Lock With Async ↑↑↑↑↑

                //回傳結果
                result.FilePath = Entity.FilePath;
                result.IsSuccess = InsertResult.IsSuccess;
                result.Message = InsertResult.Message;
                if (InsertResult.IsSuccess)
                {
                    //成功回傳FileID
                    result.FileID = Entity.FileId;

                    //處理單檔(舊檔)刪除
                    await FileDelete(OldFileID);
                }

            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                    result.FileID = String.Empty;
                }
            }

            return result;
        }

        /// <summary>
        /// 多檔案上傳
        /// </summary>
        /// <param name="Files"></param>
        /// <param name="Folder"></param>
        /// <param name="FileDescription"></param>
        /// <param name="OldFileID"></param>
        /// <param name="SplitChar"></param>
        /// <returns></returns>
        public async Task<FileResultModel> FileUploadMultipleAsync(IEnumerable<IFormFile> Files, string Folder, string? FileDescription = null, string? OldFileID = null, char SplitChar = ',')
        {
            FileResultModel result = new FileResultModel();
            List<string> NewFileIDList = new List<string>();
            try
            {
                if (Files != null && Files.Count() > 0)
                {
                    foreach (IFormFile File in Files)
                    {
                        //呼叫單檔上傳
                        var fileResult = await FileUploadAsync(File, Folder, FileDescription);
                        result.IsSuccess = fileResult.IsSuccess;
                        result.Message += fileResult.Message;

                        if (fileResult.IsSuccess)
                        {
                            NewFileIDList.Add(fileResult.FileID);
                        }
                        else
                        {
                            //新增失敗，刪除已上傳檔案，並直接return
                            await FileDelete(NewFileIDList);
                            return result;
                        }
                    }
                    result.FileID = String.Join(SplitChar, NewFileIDList);
                }

                /* 刪除舊檔 */
                var delete = await FileDelete(OldFileID);

                if (!delete.IsSuccess)
                {
                    await _allCommonService.Error_Record("FileService", "delete files", "id:" + OldFileID + result.Message);
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message += ex.ToString();
                result.FileID = String.Empty;

                //刪除已上傳檔案
                await FileDelete(NewFileIDList);
            }

            return result;
        }

        /// <summary>
        /// 20220427 Chester
        /// 除圖片外預設回傳 other 顯示設定的 icon (fileinput-custom.js 中 previewFileIconSettings 可設定對應圖)
        /// 若改回原設定 type = "pdf"、"office"...則會顯示檔案內部預覽 (但不一定所有檔案均有辦法預覽成功)，並會額外增加站台 loading (iframe連出到其它站台) 
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public string BS_Fileinput_GetFileType(string FileName)
        {
            string type = string.Empty;

            string ext = Path.GetExtension(FileName);

            switch (ext)
            {
                case ".jpg":
                case ".png":
                case ".jpeg":
                case ".gif":
                    type = "image";
                    break;
                case ".pdf":
                    //type = "pdf";
                    type = "other";
                    break;
                case ".html":
                case ".htm":
                    //type = "html";
                    type = "other";
                    break;
                case ".doc":
                case ".docx":
                case ".xls":
                case ".xlsx":
                case ".ppt":
                case ".pptx":
                    //type = "office";
                    type = "other";
                    break;
                case ".odt":
                case ".ods":
                case ".odp":
                    //type = "gdocs";
                    type = "other";
                    break;
                case ".webm":
                case ".mp4":
                case ".og":
                    //type = "video";
                    type = "other";
                    break;
                case ".ogg":
                case ".mp3":
                case ".wav":
                    //type = "audio";
                    type = "other";
                    break;
                case ".txt":
                case ".md":
                case ".ini":
                case ".nfo":
                    //type = "text";
                    type = "other";
                    break;
                default:
                    type = "other";
                    break;
            }

            return type;
        }


        /// <summary>
        /// 取得檔案(FileInput使用)
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public object GetFileInput(string FileID)
        {
            _message = string.Empty;
            List<string> fileIDList = FileID == null ? new List<string>() : FileID.Split(',').ToList();

            if (fileIDList.Any())
            {
                List<TbFileInfo> temp = Lookup<TbFileInfo>(ref _message, x => fileIDList.Contains(x.FileId) && !x.IsDelete).ToList();

                //製造bootstrap file input 所需要的json
                var initialPreview = temp.Select(x => x.FilePath).ToList();

                var initialPreviewConfig = temp.Select(x => new { caption = x.FileName, url = "/Backend/File/FileDelete", key = x.FileId, downloadUrl = x.FilePath, filename = x.FileName, previewAsData = true, type = BS_Fileinput_GetFileType(x.FilePath) }).ToList();

                return new { isSuccess = true, data = new { initialPreview = initialPreview, initialPreviewConfig = initialPreviewConfig } };
            }
            else
            {
                return new { isSuccess = true, data = new { initialPreview = new List<string>(), initialPreviewConfig = new List<string>() } };
            }
        }

        /// <summary>
        /// 取檔案真實路徑(wwwroot底下)
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public string GetFileFullPath(string FilePath)
        {
            string fullPath = string.Empty;
            if (!string.IsNullOrEmpty(FilePath))
            {
                fullPath = MapPath(FilePath);
            }
            return fullPath;
        }

        /// <summary>
        /// 刪除檔案_單檔 或 陣列相串之string
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public async Task<FileResultModel> FileDelete(string? FileID, char SplitChar = ',')
        {
            FileResultModel result = new FileResultModel();
            try
            {
                if (!string.IsNullOrEmpty(FileID))
                {
                    List<string> FileIDList = FileID.Split(SplitChar).ToList();
                    result = await FileDelete(FileIDList);
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message += ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// 刪除檔案_多檔List
        /// </summary>
        /// <param name="FileIDList"></param>
        /// <returns></returns>
        public async Task<FileResultModel> FileDelete(List<string> FileIDList)
        {
            UserSessionModel? userinfo = _contextAccessor.HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            _message = string.Empty;
            DateTime now = DateTime.Now;
            FileResultModel result = new FileResultModel();

            try
            {
                if (FileIDList != null && FileIDList.Count > 0)
                {
                    List<TbFileInfo> FileInfoList = Lookup<TbFileInfo>(ref _message, x => FileIDList.Contains(x.FileId)).ToList();
                    if (FileInfoList != null && FileInfoList.Count > 0)
                    {
                        foreach (var file in FileInfoList)
                        {
                            file.IsDelete = true;
                            file.ModifyUser = userinfo.UserID;
                            file.ModifyDate = now;

                            if (_config.GetValue<bool>("Site:FileRealDelete"))
                            {
                                // 檔案存放路徑 
                                string path = MapPath(file.FilePath);

                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                            }
                        }

                        var update = await UpdateRange<TbFileInfo>(FileInfoList);

                        if (!update.IsSuccess)
                        {
                            result.Message = update.Message;
                            result.IsSuccess = update.IsSuccess;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.ToString();

                return result;
            }

            return result;
        }

        /// <summary>
        /// 給相對路徑，產絕對路徑
        /// </summary>
        /// <param name="paths">相對路徑，可傳多個</param>
        /// <returns></returns>
        public string MapPath(params string[] paths)
        {
            char separator = '/';

            List<string> path_list = new List<string>();
            path_list.Add(_webHostEnvironment.WebRootPath);

            foreach (var path in paths)
            {
                List<string> path_array = path.Split(separator).Where(x => x != string.Empty && x != "~").ToList();
                path_list = path_list.Concat(path_array).ToList();
            }

            string real_path = Path.Combine(path_list.ToArray());

            return real_path;
        }


        /// <summary>
        /// 私有函數: 產生檔案編號
        /// </summary>
        /// <returns></returns>
        public String GenerateFileID()
        {
            //Random Rnd = new Random(Guid.NewGuid().GetHashCode());
            string sCodeList = "123456789ABCDEFGHJKLMNPQRSTUVWXYZ";      // 去除相似物件
            string sCode = "";
            Random rand = new Random();
            String Prefix = DateTime.Now.ToString("yyMMddhhmmssffffff");
            String NextID = String.Empty; //新序號
            Boolean isExist = true;

            while (isExist)
            {
                //Sample: 170112182358123456 + 12345
                //NextID = Prefix + Rnd.Next(10000, 99999).ToString();
                for (Int16 i = 0; i < 5; i++)
                    sCode += sCodeList[rand.Next(sCodeList.Length)];
                NextID = Prefix + sCode;
                isExist = this.isIDExist(NextID);
            }

            return NextID;
        }

        /// <summary>
        /// 私有函數: 檔案編號是否存在
        /// </summary>
        /// <param name="FileRealName"></param>
        /// <returns></returns>
        private Boolean isIDExist(String FileRealName)
        {
            string errormsg = "";
            return Lookup<TbFileInfo>(ref errormsg, x => x.FileRealName == FileRealName).Any();
        }

        /// <summary>
        /// 私有函數: 建立目錄
        /// </summary>
        /// <param name="path"></param>
        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public TbFileInfo? GetFileInfo(string FileId)
        {
            TbFileInfo? info = Lookup<TbFileInfo>(ref _message, x => x.FileId == FileId && !x.IsDelete).SingleOrDefault();

            return info;
        }
    }


}
