using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Backend.Service
{
    /// <summary>
    /// 功能細項權限管理
    /// </summary>
    public class AuthorityService : ServiceBase
    {
        private readonly AllCommonService _allCommonService;
        private readonly IConfiguration _conf;
        private string _Message = string.Empty;

        public AuthorityService(DBContext context, 
                                AllCommonService allCommonService,
                                IConfiguration configuration) : base(context)
        {
            _allCommonService = allCommonService;
            _conf = configuration;
        }

        /// <summary>
        /// 取得使用者功能權限
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="MenuID"></param>
        /// <param name="_Message"></param>
        /// <returns></returns>
        public async Task<AuthorityExtend> GetRight(string UserID, string MenuID)
        {
            AuthorityExtend auth = new AuthorityExtend();

            /* 子功能權限判定（如客戶權限有細到需區分檢示/新增/修改/刪除等功能則需開啟） */
            bool SubFuncAuthManage = _conf["Site:SubFuncAuthManage"] == "1";

            TbUserRight? data = await Lookup<TbUserRight>(ref _Message, x => x.MenuId == MenuID && x.UserId == UserID).SingleOrDefaultAsync();

            if (data != null)
            {
                if (SubFuncAuthManage)
                {
                    auth.Enabled = data.Enabled;
                    auth.Add_Enabled = data.AddEnabled;
                    auth.Modify_Enabled = data.ModifyEnabled;
                    auth.View_Enabled = data.ViewEnabled;
                    auth.Upload_Enabled = data.UploadEnabled;
                    auth.Download_Enabled = data.DownloadEnabled;
                    auth.Delete_Enabled = data.DeleteEnabled;
                }
                else
                {
                    auth.Enabled = data.Enabled;
                    auth.Add_Enabled = true;
                    auth.Modify_Enabled = true;
                    auth.View_Enabled = true;
                    auth.Upload_Enabled = true;
                    auth.Download_Enabled = true;
                    auth.Delete_Enabled = true;
                }
            }

            return auth;
        }

    }
}
