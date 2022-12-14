using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Areas.Frontend.Service;
using BASE.Extensions;
using BASE.Models;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;

namespace BASE.Areas.Frontend.Controllers
{
    public class ActivityController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly ActivityService _activityService;
        private readonly QuizService _quizService;
        private readonly MailService _mailService;

        public ActivityController(IConfiguration configuration,
            AllCommonService allCommonService,
            ActivityService activtyService,
            FileService fileService,
            QuizService quizService,
            MailService mailService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _activityService = activtyService;
            _fileService = fileService;
            _quizService = quizService;
            _mailService = mailService;
        }

        public async Task<IActionResult> List(String id = "")
        {
            VM_Activity data = new VM_Activity();
            switch (id)
            {
                case "截止報名":
                case "講座":
                case "課程":
                case "活動": data.Search.Category = id; break;
                default:
                    data.Search.Category = "課程"; break;
            }

            try
            {
                //取資料
                IQueryable<ActivityExtend>? dataList = _activityService.GetActivityList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.ActivityExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);

                foreach(var item in data.ActivityExtendList)
                    item.Sections = _allCommonService.Lookup<TbActivitySection>(ref _message, x => x.ActivityId == item.Header.Id).ToList();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> List(VM_Activity datapost)
        {
            try
            {
                //取資料
                IQueryable<ActivityExtend>? dataList = _activityService.GetActivityList(ref _message, datapost.Search);

                //分頁
                if (dataList != null)
                    datapost.ActivityExtendList = await PagerInfoService.GetRange(dataList, datapost.Search.PagerInfo);

                foreach (var item in datapost.ActivityExtendList)
                    item.Sections = _allCommonService.Lookup<TbActivitySection>(ref _message, x => x.ActivityId == item.Header.Id).ToList();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }


            return View(datapost);
        }

        public async Task<IActionResult> Detail(string id)
        {

            VM_Activity data = new VM_Activity();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                data.ActivityExtendItem = _activityService.GetActivityExtendItem(ref _message, decrypt_id);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        public async Task<IActionResult> Register(string id)
        {
            HttpContext.Session.Set(Backend.Models.SessionStruct.VerifyCode.Activity, new ValidImageHelper().RandomCode(5));

            VM_ActivityReigster data = new VM_ActivityReigster();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                data.Header = _allCommonService.Lookup<TbActivity>(ref _message, x => x.Id == decrypt_id).FirstOrDefault();

                if (data.Header.RegEndDate <= DateTime.Now)
                    throw new Exception("報名日期已過，請選擇別項活動");

                data.Sections = _activityService.GetSections(ref _message, decrypt_id).ToList();
                data.id = id;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VM_ActivityReigster data)
        {
            string decrypt_id = EncryptService.AES.RandomizedDecrypt(data.id);//postback後仍然要解密
            string Feature = "活動報名", Action = "新增";
            bool isSuccess = false; // 最終動作成功與否
            bool unCaughtError = false;  // 例外錯誤發生，特別記錄至 TbLog

            DateTime dtnow = DateTime.Now;
            TbActivityRegister? main = new TbActivityRegister();
            List<TbActivityRegisterSection>? RegSections = new List<TbActivityRegisterSection>();

            String code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Activity);
           
            data.Sections = _activityService.GetSections(ref _message, decrypt_id).ToList();

            if (code == data.VerifyCode)
            {
                try
                {
                    if (data.Main == null || data.RegisterSection == null || data.RegisterSection.Count() <= 0)
                    {
                        TempData["TempMsg"] = "資料回傳有誤，請重新操作！";
                    }

                    main.CompanyName = data.Main.CompanyName;
                    main.CompanyLocation = data.Main.CompanyLocation;
                    main.CompanyType = data.Main.CompanyType;
                    main.Name = data.Main.Name;
                    main.JobTitle = data.Main.JobTitle;
                    main.Phone = data.Main.Phone;
                    main.CellPhone = data.Main.CellPhone;
                    main.Email = data.Main.Email;
                    main.CompanyEmpAmount = data.Main.CompanyEmpAmount;
                    main.InfoFrom = String.Join(",", data.ckbsInfoFrom);
                    main.ActivityId = decrypt_id; //給解密後的ID
                    main.CreateDate = DateTime.Now;

                    foreach (var item in data.RegisterSection)
                    {
                        TbActivityRegisterSection m = new TbActivityRegisterSection();
                        m.ActivityId = decrypt_id;  //給解密後的ID
                        m.RegisterSectionId = item.RegisterSectionId;
                        m.RegisterSectionType = item.RegisterSectionType;
                        m.IsVegin = item.IsVegin;

                        RegSections.Add(m);
                    }

                    using (var transaction = _activityService.GetTransaction())
                    {
                        try
                        {
                            //新增
                            await _activityService.Insert(main, transaction);
                            //target_id = main.Id; //新增後的主鍵存起來

                            //新增場次
                            foreach (var reg in RegSections)
                            {
                                reg.RegisterId = main.Id;
                                await _activityService.Insert(reg, transaction);
                            }

                            transaction.Commit();
                            isSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            _message += ex.ToString();
                            TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                            unCaughtError = true;
                        }
                    }


                }
                catch (Exception ex)
                {
                    TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                    _message += ex.ToString();
                    unCaughtError = true;
                }
            } 
            else
            {
                data.Header = _allCommonService.Lookup<TbActivity>(ref _message, x => x.Id == decrypt_id).FirstOrDefault();
                data.Sections = _activityService.GetSections(ref _message, decrypt_id).ToList();
                TempData["TempMsg"] = "驗證碼錯誤";
            }

            if (isSuccess)
            {
                TempData["CostomTempEmail"] = main.Email;

                var activity = _activityService.GetActivityExtendItem(ref _message, decrypt_id);
                var activity_user = _allCommonService.Lookup<TbUserInfo>(ref _message, x => x.UserId == activity.Header.CreateUser).FirstOrDefault();

                //日期格式:○月○日  ○:○-○:○ (實體/線上 )、○月○日  ○:○-○:○ (實體/線上 )···
                var dateFormat = @"{0} {1}-{2}({3})";
                var dateString = new List<string>();
                foreach (var item in data.RegisterSection)
                {
                    var section = activity.Sections.Where(x => x.Id == item.RegisterSectionId).First();

                    dateString.Add(String.Format(dateFormat, 
                        section.Day.ToString("MM月dd日"),
                        section.StartTime.ToString(@"hh\:mm"),
                        section.EndTime.ToString(@"hh\:mm"),
                        section.SectionType
                    ));
                }

                await _mailService.SendEmail(new MailViewModel()
                {
                    Subject = String.Format(MailTmeplate.Activity.REGISTER_SUCCESS_SUBJECT, activity.Header.Title, activity.Header.Subject),
                    Body = String.Format(MailTmeplate.Activity.REGISTER_SUCCESS_CONTNET,
                                            data.Main.Name,
                                            activity.Header.Title,
                                            activity.Header.Subject,
                                            String.Join("<br />", dateString),
                                            activity_user.Email
                                            ),
                    ToList = new List<MailAddressInfo>() { new MailAddressInfo(main.Email) }
                });
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("FrontEnd", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
                return RedirectToAction("Detail", new { id = data.id }); //給未解密的ID
            else
            {
                if (main == null)
                    return RedirectToAction("Detail", new { id = data.id }); //給未解密的ID
                else
                    return View(data); /* 失敗回原頁 */
            }

        }

        /// <summary>使用者上傳健康聲明調查表</summary>
        /// <param name="id">報名編號(已加密)</param>
        public async Task<IActionResult> HealthUpload(string id)
        {
            string test = EncryptService.AES.RandomizedEncrypt("1");

            VM_OtherUpload data = new VM_OtherUpload();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            Int64 _decrypt_id = 0; Int64.TryParse(decrypt_id, out _decrypt_id);

            TbActivityRegister? main = _fileService.Lookup<TbActivityRegister>(ref _message, x => x.Id == _decrypt_id).FirstOrDefault();

            try
            {
                //檢查ID是否存在
                if (main == null)
                    throw new Exception("查無此筆報名資料");
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HealthUpload(string id, VM_OtherUpload datapost)
        {
            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            Int64 _decrypt_id = 0; Int64.TryParse(decrypt_id, out _decrypt_id);

            string Feature = "健康聲明調查表上傳", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            TbActivityRegister? main = _fileService.Lookup<TbActivityRegister>(ref _message, x => x.Id == _decrypt_id).FirstOrDefault();
            main.ModifyDate = DateTime.Now;
            main.ModifyUser = "使用者上傳";

            try
            {
                using (var transaction = _fileService.GetTransaction())
                {
                    try
                    {
                        //輪播圖片
                        if (datapost.ModifyFile != null)
                        {
                            var photo_upload = await _fileService.FileUploadAsync(datapost.ModifyFile,"Health/" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                                                                                  "Health",
                                                                                  null,
                                                                                  null,
                                                                                  transaction);

                            if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                            {
                                main.FileIdHealth = photo_upload.FileID;
                            }
                            else
                            {
                                _message += photo_upload.Message;
                            }
                        }
                        //新增
                        await _fileService.Update(main, transaction);

                        transaction.Commit();
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        _message += ex.ToString();
                        TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                        unCaughtError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "健康聲明書上傳成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
            {
                return RedirectToAction("Detail", new { id = EncryptService.AES.RandomizedEncrypt(main.ActivityId) }); //導向回活動頁面
            }
            else
            {
                //if (datapost.ModifyItem == null)
                //{
                //    return RedirectToAction("List", "Resource");
                //}
                //else
                //{
                    return View(datapost);
                //}
            }
        }

        /// <param name="id">註冊編號(已加密)</param>
        public async Task<IActionResult> Quiz(string id)
        {
            string RegisterId = EncryptService.AES.RandomizedDecrypt(id);
            Int64.TryParse(RegisterId, out Int64 _RegisterId);

            VM_Quiz data = new VM_Quiz();
            try
            {
                var regModel = _allCommonService.Lookup<TbActivityRegister>(ref _message, x => x.Id == _RegisterId).FirstOrDefault();
                if (regModel == null)
                    throw new Exception("查無此報名資料");

                var activity_model = _allCommonService.Lookup<TbActivity>(ref _message, x => x.Id == regModel.ActivityId).FirstOrDefault();
                if (activity_model == null)
                    throw new Exception("查無活動資料");

                var quiz_model = _allCommonService.Lookup<TbQuiz>(ref _message, x => x.Id == activity_model.Qid).FirstOrDefault();
                if (activity_model == null)
                    throw new Exception("此活動查無此問卷資料");

                //TODO: 如果已經填寫過問卷，則重新導向至已填答頁面
                var IsLastFillResponse = _allCommonService.Lookup<TbActivityQuizResponse>(ref _message, x => x.QuizId == quiz_model.Id).Any();
                if (IsLastFillResponse)
                    return RedirectToAction("QuizView", new { id = id });

                //組合回傳資訊
                data.QuizID = EncryptService.AES.RandomizedEncrypt(quiz_model.Id);
                data.RegisterID = EncryptService.AES.RandomizedEncrypt(regModel.Id.ToString());

                QuizExtend? dataList = _quizService.GetExtendItem(ref _message, quiz_model.Id);

                data.ExtendItem = dataList;

            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = ex.Message ?? "儲存失敗";

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);

        }

        public async Task<IActionResult> QuizView(string id)
        {
            string RegisterId = EncryptService.AES.RandomizedDecrypt(id);
            Int64.TryParse(RegisterId, out Int64 _RegisterId);

            VM_QuizView? dataList = new VM_QuizView();

            try
            {
                var regModel = _allCommonService.Lookup<TbActivityRegister>(ref _message, x => x.Id == _RegisterId).FirstOrDefault();
                if (regModel == null)
                    throw new Exception("查無此報名資料");

                var activity_model = _allCommonService.Lookup<TbActivity>(ref _message, x => x.Id == regModel.ActivityId).FirstOrDefault();
                if (activity_model == null)
                    throw new Exception("查無活動資料");

                var quiz_model = _allCommonService.Lookup<TbQuiz>(ref _message, x => x.Id == activity_model.Qid).FirstOrDefault();
                if (quiz_model == null)
                    throw new Exception("無問卷資料");

                var resp_model = _allCommonService.Lookup<TbActivityQuizResponse>(ref _message, x => x.RegisterId == regModel.Id).ToList();
                if(resp_model.Count <= 0)
                    throw new Exception("無問卷回覆資料");
                
                dataList.Header = quiz_model;
                dataList.Responses = resp_model;
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(dataList);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ajaxQuiz(List<TbActivityQuizResponse> data, String QuizId, String RegisterId)
        {
            string Feature = "問卷回覆", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            String Message = "";

            QuizId = EncryptService.AES.RandomizedDecrypt(QuizId);
            RegisterId = EncryptService.AES.RandomizedDecrypt(RegisterId);
            Int64 _RegisterId = 0; Int64.TryParse(RegisterId, out _RegisterId);

            try
            {
                try
                {
                    if (data.Count <= 0)
                        throw new Exception("無任何資料傳入");

                    foreach(var item in data)
                    {
                        item.CreateDate = DateTime.Now;
                        item.QuizId = QuizId;
                        item.RegisterId = _RegisterId;
                        await _allCommonService.Insert(item);
                    }

                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    _message += ex.ToString();
                    Message = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                    unCaughtError = true;
                }
            }
            catch (Exception ex)
            {
                Message = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                Message = $"您的問卷已填答完成，感謝您的回覆";
            }
            else
            {
                Message = Message ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            return Json(new { IsSuccess = isSuccess, Message = Message });
        }


        //簽到
        public async Task<IActionResult> Checkin(string aid, string sid)
        {
            string ActivityId = EncryptService.AES.RandomizedDecrypt(aid);
            string SectionId = EncryptService.AES.RandomizedDecrypt(sid);
            Int64.TryParse(SectionId, out long _SectionId);

            VM_Checkin data = new VM_Checkin();

            try
            {
                var activity = _allCommonService.Lookup<TbActivity>(ref _message, x => x.Id == ActivityId).FirstOrDefault();
                if (activity == null)
                    throw new Exception("找不到此活動編號");

                var section = _allCommonService.Lookup<TbActivitySection>(ref _message, x => x.Id == _SectionId && x.ActivityId == ActivityId).FirstOrDefault();
                if (section == null)
                    throw new Exception("找不到此場次編號，或此活動不包含此場次編號");

                data.Section = section;
                data.Activity = activity;

            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = ex.Message ?? "儲存失敗";

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);

        }


        //簽到
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkin(string aid, string sid, VM_Checkin datapost)
        {
            string ActivityId = EncryptService.AES.RandomizedDecrypt(aid);
            string SectionId = EncryptService.AES.RandomizedDecrypt(sid);
            Int64.TryParse(SectionId, out long _SectionId);

            string Feature = "簽到", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            try
            {
                try
                {
                    var activity = _allCommonService.Lookup<TbActivity>(ref _message, x => x.Id == ActivityId).FirstOrDefault();
                    if (activity == null)
                        throw new ValidException("找不到此活動編號");

                    var section = _allCommonService.Lookup<TbActivitySection>(ref _message, x => x.Id == _SectionId && x.ActivityId == ActivityId).FirstOrDefault();
                    if (section == null)
                        throw new ValidException("找不到此場次編號，或此活動不包含此場次編號");

                    datapost.Section = section;
                    datapost.Activity = activity;

                    var register = _allCommonService.Lookup<TbActivityRegister>(ref _message, x => x.Name == datapost.Name &&
                                                                                                   x.Email == datapost.Email &&
                                                                                                   x.ActivityId == ActivityId).FirstOrDefault();
                    if (register == null)
                        throw new ValidException("找不到您的報名資料，請再確認");

                    var register_section = _allCommonService.Lookup<TbActivityRegisterSection>(ref _message, x => x.RegisterSectionId == _SectionId && x.ActivityId == ActivityId).FirstOrDefault();
                    if (register == null)
                        throw new ValidException("找不到您的報名場次資料，請再確認");

                    if (DateTime.Now.Hour < 12)
                    {
                        register_section.IsSigninAm = true;
                        register_section.SigninDateAm = DateTime.Now;
                    }
                    else
                    {
                        register_section.IsSigninPm = true;
                        register_section.SigninDatePm = DateTime.Now;
                    }

                    register_section.ModifyDate = DateTime.Now;
                    register_section.ModifyUser = "QRCode簽到";

                    //更新簽到日
                    await _fileService.Update(register_section);

                    //寄送問卷調查通知
                    await _mailService.SendEmail(new MailViewModel()
                    {
                        Subject = String.Format(MailTmeplate.Activity.SATISFACTION_SUBJECT, section.Day.ToString("yyyy / MM / dd"), activity.Title, activity.Subject),
                        Body = String.Format(MailTmeplate.Consult.REQUIRED_SURVEY_CONTNET,
                                       section.Day.ToString("yyyy / MM / dd"),
                                       activity.Title, 
                                       activity.Subject,
                                       Url.Action("Quiz", "Activity", new { id = EncryptService.AES.RandomizedEncrypt(register.Id.ToString()) }, Request.Scheme)
                                       ),
                        ToList = new List<MailAddressInfo>() { new MailAddressInfo(register.Email) }
                    });

                    isSuccess = true;
                }
                catch(ValidException ex)
                {
                    TempData["TempMsg"] = ex.Message;
                }
                catch (Exception ex)
                {
                    _message += ex.ToString();
                    TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                    unCaughtError = true;
                }
            }
            catch (Exception ex)
            {
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "簽到成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
            {
                return RedirectToAction("Detail", new { id = EncryptService.AES.RandomizedEncrypt(ActivityId) }); //導向回活動頁面
            }
            else
            {
                return View(datapost);
            }

        }
    }
}
