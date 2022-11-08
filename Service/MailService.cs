using BASE.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BASE.Service
{
    public class MailService: ServiceBase
    {
        private readonly IConfiguration _configuration;

        public MailService(DBContext context,
            IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 寄送信件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<MailResultModel> SendEmail(MailViewModel data)
        {
            MailResultModel result = new MailResultModel();
            try
            {
                // 建立 SmtpClient 物件
                SmtpClient smtp = new SmtpClient
                {
                    // Mail Server Domain
                    Host = _configuration.GetValue<string>("MailSettings:Host"),
                    // Mail Server Post
                    Port = _configuration.GetValue<int>("MailSettings:Port"),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    // 設定帳號密碼
                    Credentials = new NetworkCredential(_configuration.GetValue<string>("MailSettings:UserName"),
                                                        _configuration.GetValue<string>("MailSettings:Password")),
                    // 是否使用SSL加密傳輸 (注意：Gmail的smtp必需要使用SSL)
                    EnableSsl = _configuration.GetValue<bool>("MailSettings:EnableSsl")
                };

                // 建立 MaillMessage 物件
                MailMessage message = new MailMessage();
                // 寄件者
                message.From = new MailAddress(_configuration.GetValue<string>("MailSettings:FormAddress"), _configuration.GetValue<string>("MailSettings:FormName"));
                // 主旨
                message.Subject = data.Subject.ToString();
                // 內容
                message.Body = data.Body.ToString();
                message.IsBodyHtml = true;

                // 收件者
                // data.ToList?.Count > 0 意思等同於 data.ToList != null && data.ToList.Count > 0
                if (data.ToList?.Count > 0)
                {
                    data.ToList.ForEach(x => message.To.Add(new MailAddress(x.Email, x.Name)));
                }
                // 副本
                if (data.CCList?.Count > 0)
                {
                    data.CCList.ForEach(x => message.CC.Add(new MailAddress(x.Email, x.Name)));
                }
                // 密件副本
                if (data.BccList?.Count > 0)
                {
                    data.BccList.ForEach(x => message.Bcc.Add(new MailAddress(x.Email, x.Name)));
                }
                // 附件
                if (data.AttachmentList?.Count > 0)
                {
                    data.AttachmentList.ForEach(x => message.Attachments.Add(x));
                }

                // 發送Email
                await smtp.SendMailAsync(message);
                smtp.Dispose();
                
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            // Mail Log
            TbMailLog log = new TbMailLog()
            {
                // 寄件者
                Sender = _configuration.GetValue<string>("MailSettings:FormName"),
                // 接收者(寄件者+副本+密件副本)
                Receiver = JsonSerializer.Serialize(data.GetReceiver(), new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                // 主旨
                Title = data.Subject,
                // 內容
                Content = data.Body,
                // 附件(僅記錄名稱)
                Attachment = JsonSerializer.Serialize(data.AttachmentList?.Select(x => x.Name), new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                IsSuccess = result.IsSuccess,
                Message= result.Message,
                CreateDate = DateTime.Now,
            };
            await Insert(log);

            result.Sender = log.Sender;

            return result;
        }


        #region 呼叫範例
        //private async void CellSendEmail(IFormFile fileinput)
        //{
        //    MailViewModel model = new MailViewModel();
        //    model.Subject = "測試信件";
        //    model.Body = "內容123";

        //    // 新增寄件者or副本or密件副本，方式有以下兩種
        //    // 1.利用Function將資料寫入
        //    model.AddToList("willchen@iscom.com.tw", "WillChen");
        //    // 2.直接操作List方式寫入
        //    model.ToList.Add(new MailAddressInfo("willchen@iscom.com.tw", "WillChen"));

        //    // 新增附件方式有以下三種
        //    // 1.利用Function將檔案實體位置寫入
        //    var file = _context.TbFileInfo.Where(x => x.FileId == "FILE00000000092").FirstOrDefault();
        //    var path = _fileService.MapPath(file.FilePath);
        //    model.AddAttachment(path);
        //    // 2.利用Function將檔案Stream及名稱寫入
        //    using (var ms = new MemoryStream())
        //    {
        //        fileinput.CopyTo(ms);
        //        byte[] bytes = ms.ToArray();
        //        model.AddAttachment(new MemoryStream(bytes), fileinput.FileName);
        //    }
        //    // 3.直接操作List方式寫入
        //    model.AttachmentList.Add(new Attachment(""));

        //    await SendEmail(model);
        //}
        #endregion

    }
}
