using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using BASE.Service;
using BASE.Service.Base;

namespace BASE.Areas.Backend.Service
{
    public class UserService : ServiceBase
    {
        private readonly AllCommonService _allCommonService;
        private readonly IConfiguration _conf;

        public UserService(DBContext context,
                           AllCommonService allCommonService,
                           IConfiguration configuration) : base(context)
        {
            _allCommonService = allCommonService;
        }

        /// <summary>取得使用者相關資料</summary>
        /// <returns></returns>
        public IQueryable<UserExtend> getUserExtend(System.Linq.Expressions.Expression<Func<UserExtend, bool>>? filter = null)
        {
            IQueryable<UserExtend> dataList = (from User in _context.TbUserInfo
                                               join UserPhoto in _context.TbFileInfo on User.Photo equals UserPhoto.FileId into UserPhoto0
                                               from UserPhoto in UserPhoto0.DefaultIfEmpty()

                                               select new UserExtend
                                               {
                                                   User = User,
                                                   UserPhoto = UserPhoto
                                               });

            if (filter != null)
            {
                dataList = dataList.Where(filter);
            }

            return dataList;
        }

    }
}
