using BASE.Models.DB;

namespace BASE.Areas.Backend.Models.Extend
{
    public class AccountExtend
    {
        /// <summary> 帳號資訊 </summary>
        public TbUserInfo account { get; set; } = null!;
        /// <summary> 帳號群組 </summary>
        public TbUserInGroup accountGroup { get; set; }
        /// <summary> 群組 </summary>
        public TbGroupInfo groupInfo { get; set; }

        /// <summary> 經歷 </summary>
        public List<TbUserInfoExperience> listUserEXP { get; set; }
    }

    public class OperateExtend
    {
        /// <summary> 操作歷程記錄 </summary>
        public TbBackendOperateLog operateLog { get; set; }

        /// <summary> 操作帳號 </summary>
        public TbUserInfo User { get; set; }
    }
}
