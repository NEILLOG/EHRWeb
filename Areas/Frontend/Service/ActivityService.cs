﻿using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.PTG;
using NPOI.XWPF.UserModel;

namespace BASE.Areas.Frontend.Service
{
    public class ActivityService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;

        public ActivityService(DBContext context,
            AllCommonService allCommonService,
            FileService fileService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _fileService = fileService;
        }

        public IQueryable<ActivityExtend>? GetActivityList(ref String ErrMsg, VM_ActivityQueryParam? vmParam)
        {
            try
            {
                IQueryable<ActivityExtend>? dataList = 
                    (from Activity in _context.TbActivity.Where(x => !x.IsDelete && x.IsValid)

                     join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on Activity.ActivityImage equals FileInfo.FileId into Activty_File
                     from FileInfo in Activty_File.DefaultIfEmpty()

                     orderby Activity.CreateDate descending
                     select new ActivityExtend
                     {
                        Header = Activity,
                        FileInfo = FileInfo,
                     });

                Boolean IsShowPassedActivity = false; //是否顯示截止報名活動
                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        dataList = dataList.Where(x => x.Header.Title.Contains(vmParam.Keyword) || x.Header.Description.Contains(vmParam.Keyword));
                    }

                    if (!String.IsNullOrEmpty(vmParam.Category))
                        switch (vmParam.Category)
                        {
                            case "講座":
                            case "課程":
                            case "活動":
                                dataList = dataList.Where(x => x.Header.Category == vmParam.Category);
                                break;
                            case "截止報名":
                                IsShowPassedActivity = true;
                                break;
                        }
                }

                if (IsShowPassedActivity)
                    dataList = dataList.Where(x => x.Header.RegEndDate <= DateTime.Now || x.Header.IsPublish == false);
                else
                    dataList = dataList.Where(x => x.Header.RegEndDate > DateTime.Now && x.Header.IsPublish == true);

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        public IQueryable<TbActivitySection> GetSections(ref String ErrMsg, string id)
        {
            try
            {
                IQueryable<TbActivitySection>? dataList =
                    (from Activity in _context.TbActivity.Where(x => !x.IsDelete && x.IsPublish && x.IsValid)
                     join Section in _context.TbActivitySection on Activity.Id equals Section.ActivityId
                     where Activity.Id == id
                     select Section);
               
                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        public ActivityExtend GetActivityExtendItem(ref String ErrMsg, string id)
        {
            try
            {
                id = id.Replace("'", "").Replace("=", "").Replace("\"", "").Replace("!", "").Replace("+", "").Replace("-", "");

                IQueryable<ActivityExtend>? dataList =
                    (from Activity in _context.TbActivity.Where(x => !x.IsDelete && x.IsValid)

                     join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on Activity.ActivityImage equals FileInfo.FileId into Activty_File
                     from FileInfo in Activty_File.DefaultIfEmpty()

                     orderby Activity.CreateDate descending
                     select new ActivityExtend
                     {
                         Header = Activity,
                         FileInfo = FileInfo
                     });

                // 找出該筆資料
                ActivityExtend data = dataList.Where(x => x.Header.Id == id).FirstOrDefault();
                               data.Sections = _context.TbActivitySection.Where(x => x.ActivityId == id).ToList();

                return data;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        public Boolean IsCellphoneRegisterd(ref String ErrMsg, string cellphone, long section_id)
        {
            try
            {
                var dataList =
                    (from register in _context.TbActivityRegister
                     join sections in _context.TbActivityRegisterSection on register.Id equals sections.RegisterId
                     where register.CellPhone == cellphone && sections.RegisterSectionId == section_id
                     select register);

                return dataList.Any();
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return true;
            }
        }

        //產生健康聲明檔案並上傳
        public void GenerateHealthUpload(String ActiveId, string filePath)
        {
            try
            {
                String message = "";
                var activity = GetActivityExtendItem(ref message, ActiveId);

                string sampleFilePath = _fileService.MapPath("Sample/HealthUpload.docx");
                using (FileStream stream = System.IO.File.OpenRead(sampleFilePath))
                {
                    XWPFDocument doc = new XWPFDocument(stream);

                    //段落
                    foreach (var para in doc.Paragraphs)
                    {
                        string key = $"$[ActivityTitle]";
                        if (para.Text.Contains(key))
                        {
                            try
                            {
                                para.ReplaceText(key, activity.Header.Title);
                            }
                            catch (Exception ex)
                            {
                                para.ReplaceText(key, "");
                            }
                        }

                        string key_subject = $"$[ActivitySubject]";
                        if (para.Text.Contains(key_subject))
                        {
                            try
                            {
                                para.ReplaceText(key_subject, activity.Header.Subject);
                            }
                            catch (Exception ex)
                            {
                                para.ReplaceText(key, "");
                            }
                        }
                    }

                    FileStream Fs = new FileStream(filePath, FileMode.OpenOrCreate);
                    doc.Write(Fs);
                    Fs.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
