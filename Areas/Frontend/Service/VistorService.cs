using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.PTG;
using NPOI.XWPF.UserModel;

namespace BASE.Areas.Frontend.Service
{
    public class VistorService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;

        public VistorService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        /// <summary>取得瀏覽人數</summary>
        /// <param name="IP">目前連線IP  若為null只取瀏覽人數而不累加</param>
        /// <returns></returns>
        public async Task<int> Visitors(string IP)
        {
            String _Message = "";
            List<TbVisitors> temp = Lookup<TbVisitors>(ref _Message).ToList();

            if (string.IsNullOrEmpty(IP))
            {
                return temp.Count;
            }
            else
            {
                //找出同個IP在今天是否有登入，沒有就寫一筆，有就回傳數量
                TbVisitors check = temp.Where(x => x.Ip == IP && x.VisitDate.Date == DateTime.Now.Date).FirstOrDefault();

                if (check == null)
                {
                    await Insert(new TbVisitors()
                    {
                        Ip = IP,
                        VisitDate = DateTime.Now
                    });

                    return temp.Count + 1;
                }
                else
                {
                    return temp.Count;
                }
            }
        }
    }
}
