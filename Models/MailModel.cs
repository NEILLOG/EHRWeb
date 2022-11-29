using System.Net.Mail;
using System.Text;

namespace BASE.Models
{
    #region 呼叫MailService傳入的Model
    public class MailViewModel
    {
        public MailViewModel()
        {
            ToList = new List<MailAddressInfo>();
            CCList = new List<MailAddressInfo>();
            BccList = new List<MailAddressInfo>();
            AttachmentList = new List<Attachment>();
        }


        /// <summary>收件者列表</summary>
        public List<MailAddressInfo> ToList { get; set; }
        /// <summary>主旨</summary>
        public string Subject { get; set; } = null!;
        /// <summary>內容</summary>
        public string Body { get; set; } = null!;

        /// <summary>副本列表</summary>
        public List<MailAddressInfo> CCList { get; set; }
        /// <summary>密件副本列表</summary>
        public List<MailAddressInfo> BccList { get; set; }
        /// <summary>附件列表</summary>
        public List<Attachment> AttachmentList { get; set; }




        /// <summary>
        /// 新增附件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName">檔案名稱</param>
        public void AddAttachment(Stream stream, string fileName)
        {
            AttachmentList.Add(new Attachment(stream, fileName));
        }

        /// <summary>
        /// 新增附件
        /// </summary>
        /// <param name="fileFullPath">附件實體位置</param>
        public void AddAttachment(string fileFullPath)
        {
            AttachmentList.Add(new Attachment(fileFullPath));
        }

        /// <summary>
        /// 新增寄件者
        /// </summary>
        /// <param name="email">電子信箱</param>
        /// <param name="name">名稱(預設空白)</param>
        public void AddToList(string email, string name = "")
        {
            ToList.Add(new MailAddressInfo(email, name));
        }

        /// <summary>
        /// 新增副本
        /// </summary>
        /// <param name="email">電子信箱</param>
        /// <param name="name">名稱(預設空白)</param>
        public void AddCCList(string email, string name = "")
        {
            CCList.Add(new MailAddressInfo(email, name));
        }

        /// <summary>
        /// 新增密件副本
        /// </summary>
        /// <param name="email">電子信箱</param>
        /// <param name="name">名稱(預設空白)</param>
        public void AddBccList(string email, string name = "")
        {
            BccList.Add(new MailAddressInfo(email, name));
        }

        public object GetReceiver()
        {
            return new { ToList, CCList, BccList };
        }

        public String GetToList()
        {
            return String.Join(";", this.ToList.Select(x => x.Email));
        }

        public String GetCCList()
        {
            return String.Join(";", this.CCList.Select(x => x.Email));
        }
    }

    /// <summary>
    /// 收件者or副本or密件副本 使用之類別
    /// </summary>
    public class MailAddressInfo
    {
        public MailAddressInfo(string email)
        {
            Email = email;
        }

        public MailAddressInfo(string email, string name)
        {
            Email = email;
            Name = name;
        }


        /// <summary>收件者Mail</summary>
        public string Email { get; set; } = null!;

        /// <summary>收件者名稱</summary>
        public string? Name { get; set; }
    }    
    
    
    #endregion


    #region MailService回傳的Model

    public class MailResultModel
    {
        public MailResultModel()
        {
            IsSuccess = false;
        }

        /// <summary>是否成功</summary>
        public bool IsSuccess { get; set; }

        /// <summary>訊息</summary>
        public string? Message { get; set; }

        public string Sender { get; set; }
    }
    #endregion

}
