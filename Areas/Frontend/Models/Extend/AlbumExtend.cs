using BASE.Models.DB;

namespace BASE.Areas.Frontend.Models.Extend
{
    public class AlbumExtend
    {
        /// <summary>
        /// 表頭
        /// </summary>
        public TbAlbum? Header { get; set; }

        /// <summary>
        /// 相片
        /// </summary>
        public TbFileInfo? FileInfo { get; set; }
    }
}
