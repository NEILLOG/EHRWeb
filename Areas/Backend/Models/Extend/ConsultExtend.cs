using BASE.Models.DB;

namespace BASE.Areas.Backend.Models.Extend
{
    public class ConsultExtend
    {
        /// <summary> 諮詢報名 </summary>
        public TbConsultRegister ConsultRegister { get; set; }

        /// <summary> 企業需求調查表 </summary>
        public TbFileInfo RequireSurveyFile { get; set; }

        /// <summary> 滿意度調查表 </summary>
        public TbFileInfo SatisfySurveyFile { get; set; }
        
        /// <summary> 顧問 </summary>
        public string ConsultantList { get; set; }

        /// <summary> 輔導助理 </summary>
        public string Assistant { get; set; }

        /// <summary> 審核狀態 </summary>
        public string sApprove { get; set; }

        /// <summary> 諮詢主題 </summary>
        public string textOfSubject { get; set; }


        /// <summary> 結案狀態 </summary>
        public string sClose { get; set; }
    }

    public class CounselingHistoryExtend
    {
        /// <summary> 諮詢輔導報名主表 </summary>
        public TbConsultRegister ConsultRegister { get; set; }
        /// <summary> 檔案表 </summary>
        public TbFileInfo FileInfo { get; set; }
    }

    /// <summary>
    /// 諮詢輔導報名
    /// </summary>
    public class CounselingSigninExtend
    {
        /// <summary> 頁籤日期</summary>
        public string sheetName { get; set;}
        // 輔導時間
        public string CounselingTime { get; set; }
        // 輔導地點
        public string CounselingPlace { get; set; }
        // 輔導對象列表
        public List<CounselingObjectExtend> listObject { get; set; } 
    }

    public class CounselingObjectExtend
    {
        /// <summary> 單位 </summary>
        public string Unit { get; set; }

        /// <summary> 姓名 </summary>
        public string Name { get; set; }

        /// <summary> 職稱 </summary>
        public string JobTitle { get; set; }
    }
}
