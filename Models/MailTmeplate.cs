namespace BASE.Models
{
    public class MailTmeplate
    {
        /// <summary>活動</summary>
        public class Activity 
        {
            //報名成功
            public static String REGISTER_SUCCESS_SUBJECT = "勞動部勞動力發展署桃竹苗分署-{0}–{1}報名完成通知信 (本郵件由系統自動寄發，請勿直接回覆此郵件)";
            public static String REGISTER_SUCCESS_CONTNET = @"
                <p>{0} 您好</p>
                <p>您已完成報名程序，以下是您報名的活動資訊<br />
                活動主題：「{1}-{2}」<br />
                活動日期：{3}<br />
                報名結果：待審核中<br />
                </p>
    
                <p>※請注意收到此封信不代表您的報名已錄取，主辦單位保留參加資格審核之權利，我們將會審核您的報名資料，請隨時留意審核結果通知信。</p

                <p>若對活動有任何問題或欲取消報名，您可回覆信件至{4}或來電與我們聯繫。</p>

                <p>
                敬祝 順心平安<br />
                勞動部勞動力發展署桃竹苗分署<br />
                桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />
                諮詢電話02-23660812 #164、#127  03-4855368#1905
                </p>
            ";
            
            //滿意度
            public static String SATISFACTION_SUBJECT = "【活動滿意度問卷】桃竹苗分署{0}{1}–{2}";
            public static String SATISFACTION_CONTNET = @"
                <p>敬愛的學員，您好：<br />
                感謝您於 {0} 參加勞動部勞動力發展署桃竹苗分署主辦之{1}–{2}。</p>

                <p>為優化活動品質，桃竹苗分署邀請您填寫本次活動滿意度調查問卷，作為未來活動規劃與改善之參考，敬請協助填寫，謝謝您！</p>

                <p>{0} 滿意度問卷調查連結：</p>
                <p><a href=""{3}"">網址連結</a></p>

                <p>附件：課程講義、其他服務資源資訊</p>

                <p>若有任何問題，歡迎來電或來信詢問<br />
                敬祝 順心平安<br />
                勞動部勞動力發展署桃竹苗分署<br />
                桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />
                諮詢電話02-23660812 #164、#127  03-4855368#1905
                </p>";
        }
        /// <summary>諮詢服務</summary>
        public class Consult {
            public static String REQUIRED_SURVEY_SUBJECT = "【勞動部桃竹苗分署人資整合案_諮詢服務】敬請協助填寫企業需求調查表，謝謝您！";
            public static String REQUIRED_SURVEY_CONTNET = @"
                <p>{0} {1}您好</p>
 
                <p>感謝您預約報名企業諮詢輔導服務，本分署計畫人員將盡速致電與您確認實際諮詢需求。</p>

                <p>為使後續諮詢流程順暢，需請您預先填寫企業需求調查表(<a href=""{2}"" target=""_blank"">連結</a>)，並於一周內回傳，<br />
                以利諮詢顧問預先了解，於輔導時能更有效迅速地提供方案或對策供參！ </p>
 
                <p>另本分署將同步進行媒合顧問時間，待確認後將再次致電與您確認輔導時間。</p>

                <p>若有任何問題也歡迎隨時來信或來電，謝謝您！<br />
                敬祝 事事順心！</p>

                <p>勞動部勞動力發展署桃竹苗分署<br />
                桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />
                計畫諮詢電話：03-4855368#1905 <br />
                Email：yrung@wda.gov.tw</p>";
        }

        /// <summary>臨時課程變更</summary>
        public class Resource
        {
            public static String MODIFY_APPLY_SUBJECT = "勞動部勞動力發展署桃竹苗分署-企業人力資源提升計畫—事業單位上傳課程臨時變更提醒通知信";
            public static String MODIFY_APPLY_CONTNET = @"
                <p>您好</p>
                <p>{0} 有事業單位上傳課程臨時變更申請書，提醒您儘速上系統下載文件，辦理相關作業並於完成後上系統點選「同意」或「不同意」。</p>
            ";
        }
    }
}
