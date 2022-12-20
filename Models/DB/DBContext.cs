using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BASE.Models.DB
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<NtuFaq> NtuFaq { get; set; } = null!;
        public virtual DbSet<TbActionLog> TbActionLog { get; set; } = null!;
        public virtual DbSet<TbActivity> TbActivity { get; set; } = null!;
        public virtual DbSet<TbActivityQuizResponse> TbActivityQuizResponse { get; set; } = null!;
        public virtual DbSet<TbActivityRegister> TbActivityRegister { get; set; } = null!;
        public virtual DbSet<TbActivityRegisterSection> TbActivityRegisterSection { get; set; } = null!;
        public virtual DbSet<TbActivitySection> TbActivitySection { get; set; } = null!;
        public virtual DbSet<TbAdvertise> TbAdvertise { get; set; } = null!;
        public virtual DbSet<TbAlbum> TbAlbum { get; set; } = null!;
        public virtual DbSet<TbApiAuth> TbApiAuth { get; set; } = null!;
        public virtual DbSet<TbApiLog> TbApiLog { get; set; } = null!;
        public virtual DbSet<TbBackActionLog> TbBackActionLog { get; set; } = null!;
        public virtual DbSet<TbBackendOperateLog> TbBackendOperateLog { get; set; } = null!;
        public virtual DbSet<TbBasicColumn> TbBasicColumn { get; set; } = null!;
        public virtual DbSet<TbCity> TbCity { get; set; } = null!;
        public virtual DbSet<TbConsultRegister> TbConsultRegister { get; set; } = null!;
        public virtual DbSet<TbContactUs> TbContactUs { get; set; } = null!;
        public virtual DbSet<TbCountry> TbCountry { get; set; } = null!;
        public virtual DbSet<TbExperience> TbExperience { get; set; } = null!;
        public virtual DbSet<TbFileInfo> TbFileInfo { get; set; } = null!;
        public virtual DbSet<TbGroupInfo> TbGroupInfo { get; set; } = null!;
        public virtual DbSet<TbGroupRight> TbGroupRight { get; set; } = null!;
        public virtual DbSet<TbHrArticle> TbHrArticle { get; set; } = null!;
        public virtual DbSet<TbHrPackage> TbHrPackage { get; set; } = null!;
        public virtual DbSet<TbIdSummary> TbIdSummary { get; set; } = null!;
        public virtual DbSet<TbLanguage> TbLanguage { get; set; } = null!;
        public virtual DbSet<TbLog> TbLog { get; set; } = null!;
        public virtual DbSet<TbLoginRecord> TbLoginRecord { get; set; } = null!;
        public virtual DbSet<TbMailLog> TbMailLog { get; set; } = null!;
        public virtual DbSet<TbMailQueue> TbMailQueue { get; set; } = null!;
        public virtual DbSet<TbMenuBack> TbMenuBack { get; set; } = null!;
        public virtual DbSet<TbMenuFront> TbMenuFront { get; set; } = null!;
        public virtual DbSet<TbNews> TbNews { get; set; } = null!;
        public virtual DbSet<TbOnePage> TbOnePage { get; set; } = null!;
        public virtual DbSet<TbProfession> TbProfession { get; set; } = null!;
        public virtual DbSet<TbProject> TbProject { get; set; } = null!;
        public virtual DbSet<TbProjectModify> TbProjectModify { get; set; } = null!;
        public virtual DbSet<TbPromotion> TbPromotion { get; set; } = null!;
        public virtual DbSet<TbPwdLog> TbPwdLog { get; set; } = null!;
        public virtual DbSet<TbQuiz> TbQuiz { get; set; } = null!;
        public virtual DbSet<TbQuizOption> TbQuizOption { get; set; } = null!;
        public virtual DbSet<TbRelationLink> TbRelationLink { get; set; } = null!;
        public virtual DbSet<TbScheduleLog> TbScheduleLog { get; set; } = null!;
        public virtual DbSet<TbScheduleLogDetail> TbScheduleLogDetail { get; set; } = null!;
        public virtual DbSet<TbSubScript> TbSubScript { get; set; } = null!;
        public virtual DbSet<TbSystemSetting> TbSystemSetting { get; set; } = null!;
        public virtual DbSet<TbUserInGroup> TbUserInGroup { get; set; } = null!;
        public virtual DbSet<TbUserInfo> TbUserInfo { get; set; } = null!;
        public virtual DbSet<TbUserInfoExperience> TbUserInfoExperience { get; set; } = null!;
        public virtual DbSet<TbUserRight> TbUserRight { get; set; } = null!;
        public virtual DbSet<TbYouTubeVideo> TbYouTubeVideo { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<NtuFaq>(entity =>
            {
                entity.HasKey(e => e.Fid);

                entity.ToTable("NtuFAQ");

                entity.HasComment("FAQ資料表");

                entity.Property(e => e.Fid)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("FID")
                    .HasComment("FAQ+流水碼*7");

                entity.Property(e => e.Answer).HasComment("答案");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.IsDelete).HasComment("是否刪除：是、否");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.Order).HasComment("排序");

                entity.Property(e => e.Question)
                    .HasMaxLength(300)
                    .HasComment("問題 (程式控管200個字)");
            });

            modelBuilder.Entity<TbActionLog>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_TB_ActionLoginRecord");

                entity.HasComment("後臺操作紀錄");

                entity.Property(e => e.Pid)
                    .HasColumnName("PID")
                    .HasComment("主鍵");

                entity.Property(e => e.Action)
                    .HasMaxLength(100)
                    .HasComment("動作");

                entity.Property(e => e.Data)
                    .HasColumnType("text")
                    .HasComment("當下異動的資料");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasComment("操作時間");

                entity.Property(e => e.Ip)
                    .HasMaxLength(50)
                    .HasColumnName("IP")
                    .HasComment("IP");

                entity.Property(e => e.Platform)
                    .HasMaxLength(20)
                    .HasComment("位置");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("操作者");
            });

            modelBuilder.Entity<TbActivity>(entity =>
            {
                entity.HasComment("活動訊息");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID")
                    .HasComment("主鍵");

                entity.Property(e => e.ActivityImage)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("活動圖片");

                entity.Property(e => e.Category)
                    .HasMaxLength(5)
                    .HasComment("類型: 課程, 講座, 活動");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DateType)
                    .HasMaxLength(5)
                    .HasComment("活動時長；半日, 全日");

                entity.Property(e => e.Description).HasComment("活動簡介");

                entity.Property(e => e.FileForEntity)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("上傳行前通知信壓縮檔(for實體參與者)");

                entity.Property(e => e.FileForOnline)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("上傳行前通知信壓縮檔(for線上參與者)");

                entity.Property(e => e.HandoutFile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("上傳講義");

                entity.Property(e => e.IsDelete).HasComment("是否已註記刪除");

                entity.Property(e => e.IsPublish).HasComment("是否開放報名");

                entity.Property(e => e.IsSend10dayNotify)
                    .HasDefaultValueSql("((0))")
                    .HasComment("此活動是否已經發布10天前活動通知");

                entity.Property(e => e.IsValid).HasComment("是否審核通過");

                entity.Property(e => e.LecturerInfo)
                    .HasMaxLength(200)
                    .HasComment("講師資訊");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Place)
                    .HasMaxLength(200)
                    .HasComment("活動地點");

                entity.Property(e => e.Qid)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("QID")
                    .HasComment("問卷主鍵編號");

                entity.Property(e => e.Quota)
                    .HasMaxLength(200)
                    .HasComment("報名名額");

                entity.Property(e => e.RegEndDate)
                    .HasColumnType("datetime")
                    .HasComment("報名結束日期");

                entity.Property(e => e.RegStartDate)
                    .HasColumnType("datetime")
                    .HasComment("報名開始日期");

                entity.Property(e => e.RegisterFor)
                    .HasMaxLength(200)
                    .HasComment("報名對象");

                entity.Property(e => e.Subject)
                    .HasMaxLength(200)
                    .HasComment("活動主題");

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("活動標題");
            });

            modelBuilder.Entity<TbActivityQuizResponse>(entity =>
            {
                entity.HasComment("活動參加問卷回覆");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.QuizDescription)
                    .HasMaxLength(200)
                    .HasComment("原始問項(落地資料)，避免後來異動後無法察看結果；其餘ID欄位僅用於紀錄，不拿來用於查詢");

                entity.Property(e => e.QuizId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("QuizID")
                    .HasComment("問卷編號");

                entity.Property(e => e.QuizOptionId)
                    .HasColumnName("QuizOptionID")
                    .HasComment("選項主鍵編號");

                entity.Property(e => e.RegisterId)
                    .HasColumnName("RegisterID")
                    .HasComment("活動報名主鍵");

                entity.Property(e => e.ResponseText)
                    .HasMaxLength(200)
                    .HasComment("簡答題文字");

                entity.Property(e => e.SelctedOption)
                    .HasMaxLength(500)
                    .HasComment("使用者回覆選項；格式: {選項}|{選項}|...；採用落地儲存；不論單選、複選均適用");
            });

            modelBuilder.Entity<TbActivityRegister>(entity =>
            {
                entity.HasComment("活動報名資訊");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.ActivityId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ActivityID")
                    .HasComment("活動主鍵編號");

                entity.Property(e => e.CellPhone)
                    .HasMaxLength(200)
                    .HasComment("手機");

                entity.Property(e => e.CompanyEmpAmount)
                    .HasMaxLength(200)
                    .HasComment("公司員工人數");

                entity.Property(e => e.CompanyLocation)
                    .HasMaxLength(200)
                    .HasComment("企業所在地");

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(200)
                    .HasComment("企業名稱");

                entity.Property(e => e.CompanyType)
                    .HasMaxLength(200)
                    .HasComment("產業別");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(500)
                    .HasComment("電子郵件");

                entity.Property(e => e.FileIdHealth)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID_Health")
                    .HasComment("健康聲明回傳檔案");

                entity.Property(e => e.InfoFrom)
                    .HasMaxLength(200)
                    .HasComment("訊息來源");

                entity.Property(e => e.IsBackup).HasComment("是否為備取(寄信判斷用)");

                entity.Property(e => e.IsValid).HasComment("是否審核通過(null預設；true通過；false不通過)");

                entity.Property(e => e.JobTitle)
                    .HasMaxLength(200)
                    .HasComment("職稱");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .HasComment("姓名");

                entity.Property(e => e.Phone)
                    .HasMaxLength(200)
                    .HasComment("連絡電話");
            });

            modelBuilder.Entity<TbActivityRegisterSection>(entity =>
            {
                entity.HasComment("場次資訊子表(含簽到記錄)，此表於報名完成時即產生");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號");

                entity.Property(e => e.ActivityId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ActivityID")
                    .HasComment("活動編號");

                entity.Property(e => e.IsSigninAm)
                    .HasColumnName("IsSigninAM")
                    .HasComment("是否已簽到(上午場)");

                entity.Property(e => e.IsSigninPm)
                    .HasColumnName("IsSigninPM")
                    .HasComment("是否已簽到(下午場)");

                entity.Property(e => e.IsVegin).HasComment("是否為素食者；true: 素食者; false:葷食者");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.RegisterId)
                    .HasColumnName("RegisterID")
                    .HasComment("活動註冊主表編號");

                entity.Property(e => e.RegisterSectionId)
                    .HasColumnName("RegisterSectionID")
                    .HasComment("活動註冊場次編號");

                entity.Property(e => e.RegisterSectionType)
                    .HasMaxLength(20)
                    .HasComment("參加場次；線上、實體");

                entity.Property(e => e.SigninDateAm)
                    .HasColumnType("datetime")
                    .HasColumnName("SigninDate_AM")
                    .HasComment("簽到日期(上午)");

                entity.Property(e => e.SigninDatePm)
                    .HasColumnType("datetime")
                    .HasColumnName("SigninDate_PM")
                    .HasComment("簽到日期(下午)");
            });

            modelBuilder.Entity<TbActivitySection>(entity =>
            {
                entity.HasComment("活動時段");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("時段流水號主鍵");

                entity.Property(e => e.ActivityId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ActivityID")
                    .HasComment("活動主鍵編號");

                entity.Property(e => e.Day)
                    .HasColumnType("date")
                    .HasComment("場次日期");

                entity.Property(e => e.EndTime).HasComment("場次結束時間，EF內請使用TimeSpan儲存");

                entity.Property(e => e.SectionType)
                    .HasMaxLength(20)
                    .HasComment("活動參與模式: 實體, 線上, 實體加線上");

                entity.Property(e => e.StartTime).HasComment("場次開始時間，EF內請使用TimeSpan儲存");
            });

            modelBuilder.Entity<TbAdvertise>(entity =>
            {
                entity.HasComment("輪播廣告");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID");

                entity.Property(e => e.IsDelete).HasComment("是否註記刪除");

                entity.Property(e => e.IsPublish).HasComment("是否上架(true:是, false: 否)");

                entity.Property(e => e.Link)
                    .HasMaxLength(500)
                    .HasComment("超連結");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("圖片標題");
            });

            modelBuilder.Entity<TbAlbum>(entity =>
            {
                entity.HasComment("活動花絮");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID")
                    .HasComment("主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayDate)
                    .HasColumnType("datetime")
                    .HasComment("顯示日期");

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("標題");
            });

            modelBuilder.Entity<TbApiAuth>(entity =>
            {
                entity.HasKey(e => e.Aid)
                    .HasName("PK_API_Auth");

                entity.HasComment("API授權清單");

                entity.Property(e => e.Aid)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("AID")
                    .HasComment("API代碼");

                entity.Property(e => e.AuthToken)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("解密後");

                entity.Property(e => e.Contactor)
                    .HasMaxLength(50)
                    .HasComment("聯絡人");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.EnAuthTk)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("EnAuthTK")
                    .HasComment("解密前");

                entity.Property(e => e.ExpiredDate)
                    .HasColumnType("datetime")
                    .HasComment("過期日");

                entity.Property(e => e.IsActive).HasComment("是否啟用");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .HasComment("聯絡人電話");

                entity.Property(e => e.UseUnit)
                    .HasMaxLength(50)
                    .HasComment("單位");
            });

            modelBuilder.Entity<TbApiLog>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_API_Log");

                entity.Property(e => e.Pid).HasColumnName("PID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Ipaddr)
                    .HasMaxLength(45)
                    .IsUnicode(false)
                    .HasColumnName("IPAddr");

                entity.Property(e => e.Request).HasColumnType("text");

                entity.Property(e => e.Response).HasColumnType("text");

                entity.Property(e => e.RoutePath).IsUnicode(false);
            });

            modelBuilder.Entity<TbBackActionLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PK_TB_BackActionLog");

                entity.Property(e => e.LogId).HasColumnName("LogID");

                entity.Property(e => e.Action)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Data).HasColumnType("text");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Ipaddress)
                    .HasMaxLength(45)
                    .IsUnicode(false)
                    .HasColumnName("IPAddress");

                entity.Property(e => e.Message).HasColumnType("text");

                entity.Property(e => e.Url).IsUnicode(false);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TbBackendOperateLog>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_TbOperateLog");

                entity.HasIndex(e => e.UserId, "IX_TbBackendOperateLog_UserID");

                entity.Property(e => e.Pid).HasColumnName("PID");

                entity.Property(e => e.Action).HasMaxLength(20);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DataKey)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasComment("異動資料 Key 值");

                entity.Property(e => e.Feature).HasMaxLength(20);

                entity.Property(e => e.Ip)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("IP");

                entity.Property(e => e.Message).HasColumnType("text");

                entity.Property(e => e.Request).HasColumnType("text");

                entity.Property(e => e.Response).HasColumnType("text");

                entity.Property(e => e.Url).IsUnicode(false);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasComment("登入裝置");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbBackendOperateLog)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TbBackendOperateLog_TbUserInfo");
            });

            modelBuilder.Entity<TbBasicColumn>(entity =>
            {
                entity.HasKey(e => new { e.BacolId, e.LangId });

                entity.Property(e => e.BacolId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("BacolID");

                entity.Property(e => e.LangId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("LangID");

                entity.Property(e => e.BacolCode)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FileId)
                    .IsUnicode(false)
                    .HasColumnName("FileID");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(255);
            });

            modelBuilder.Entity<TbCity>(entity =>
            {
                entity.HasKey(e => e.CityId)
                    .HasName("PK_TB_City");

                entity.HasComment("縣市鄉鎮表");

                entity.Property(e => e.CityId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("CityID");

                entity.Property(e => e.CityLevel).HasComment("縣市鄉鎮類型(1縣市2行政區)");

                entity.Property(e => e.CityName)
                    .HasMaxLength(50)
                    .HasComment("縣市鄉鎮名稱");

                entity.Property(e => e.CountryId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("CountryID")
                    .HasDefaultValueSql("('C000000223')");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.IsDelete).HasComment("是否刪除");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.ParentId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ParentID")
                    .HasComment("父類別");

                entity.Property(e => e.PostCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TbConsultRegister>(entity =>
            {
                entity.HasComment("諮詢輔導服務報名");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address)
                    .HasMaxLength(300)
                    .HasComment("企業登記地址");

                entity.Property(e => e.AssignAdviser1)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("指派顧問1");

                entity.Property(e => e.AssignAdviser2)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("指派顧問2");

                entity.Property(e => e.AssignAdviser3)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("指派顧問3");

                entity.Property(e => e.AssignAdviserAssistant)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("指派輔導助理");

                entity.Property(e => e.BusinessId)
                    .HasMaxLength(20)
                    .HasColumnName("BusinessID")
                    .HasComment("企業統編");

                entity.Property(e => e.ConsultAddress)
                    .HasMaxLength(300)
                    .HasComment("輔導地址");

                entity.Property(e => e.ConsultSubjects)
                    .HasMaxLength(200)
                    .HasComment("諮詢主題；若有多筆則以逗號串聯");

                entity.Property(e => e.ConsultTime)
                    .HasMaxLength(200)
                    .HasComment("預計可諮詢時間");

                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(300)
                    .HasComment("聯絡人EMAIL");

                entity.Property(e => e.ContactJobTitle)
                    .HasMaxLength(100)
                    .HasComment("聯繫人職稱");

                entity.Property(e => e.ContactName)
                    .HasMaxLength(50)
                    .HasComment("聯繫人姓名");

                entity.Property(e => e.ContactPhone).HasMaxLength(50);

                entity.Property(e => e.CounselingLogFile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("輔導紀錄檔案");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasComment("問題陳述");

                entity.Property(e => e.IsApprove).HasComment("是否審核通過");

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .HasComment("企業所在地");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .HasComment("企業名稱全銜");

                entity.Property(e => e.ReAssignDate)
                    .HasColumnType("date")
                    .HasComment("協調後可輔導的日期");

                entity.Property(e => e.ReAssignTime).HasComment("協調後可輔導的時段");

                entity.Property(e => e.RequireSurveyFile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("企業需求調查表回傳檔案");

                entity.Property(e => e.SatisfySurveyFile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("滿意度調查檔案");

                entity.Property(e => e.SigninFormFile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("已填寫簽到表");
            });

            modelBuilder.Entity<TbContactUs>(entity =>
            {
                entity.HasComment("聯絡我們");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(300);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Response).HasMaxLength(500);
            });

            modelBuilder.Entity<TbCountry>(entity =>
            {
                entity.HasKey(e => new { e.CountryId, e.LangCode })
                    .HasName("PK_TB_Country");

                entity.HasComment("國家資料");

                entity.Property(e => e.CountryId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("CountryID")
                    .HasComment("國家代碼");

                entity.Property(e => e.LangCode)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("語系");

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(5)
                    .HasComment("國家編碼");

                entity.Property(e => e.CountryName)
                    .HasMaxLength(50)
                    .HasComment("名稱");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.IsDelete).HasComment("是否刪除");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("修改者");
            });

            modelBuilder.Entity<TbExperience>(entity =>
            {
                entity.HasKey(e => e.Exid);

                entity.Property(e => e.Exid)
                    .HasColumnName("EXID")
                    .HasComment("主鍵");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.JobTitle)
                    .HasMaxLength(50)
                    .HasComment("職稱");

                entity.Property(e => e.Period)
                    .HasMaxLength(50)
                    .HasComment("服務期間");

                entity.Property(e => e.ServiceUnit)
                    .HasMaxLength(50)
                    .HasComment("服務單位");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("使用者主鍵");
            });

            modelBuilder.Entity<TbFileInfo>(entity =>
            {
                entity.HasKey(e => e.FileId)
                    .HasName("PK_TB_FileInfo");

                entity.HasComment("檔案(共用)");

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID")
                    .HasComment("檔案代碼");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.FileDescription).HasComment("檔案描述");

                entity.Property(e => e.FileName)
                    .HasMaxLength(300)
                    .HasComment("名稱");

                entity.Property(e => e.FilePath).HasComment("檔案路徑");

                entity.Property(e => e.FilePathM).HasComment("檔案描述");

                entity.Property(e => e.FileRealName)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasComment("實際檔名");

                entity.Property(e => e.IsDelete).HasComment("是否刪除");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");
            });

            modelBuilder.Entity<TbGroupInfo>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("PK_GroupInfo");

                entity.HasComment("群組");

                entity.Property(e => e.GroupId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("GroupID")
                    .HasComment("群組代碼");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.GroupLevel).HasComment("層級");

                entity.Property(e => e.GroupName)
                    .HasMaxLength(20)
                    .HasComment("群組名稱");

                entity.Property(e => e.IsDefault).HasComment("是否為預設群組\r\n程式未開放修改，只能設定一筆");

                entity.Property(e => e.IsDelete).HasComment("是否刪除");

                entity.Property(e => e.IsShowInApply).HasComment("是否顯示於\"功能權限申請\"裡");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.ParentId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ParentID")
                    .HasComment("父節點");
            });

            modelBuilder.Entity<TbGroupRight>(entity =>
            {
                entity.HasKey(e => new { e.GroupId, e.MenuId })
                    .HasName("PK_TB_GroupRight");

                entity.HasComment("群組權限");

                entity.HasIndex(e => e.MenuId, "IX_TbGroupRight_MenuID");

                entity.Property(e => e.GroupId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("GroupID")
                    .HasComment("群組代碼");

                entity.Property(e => e.MenuId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MenuID")
                    .HasComment("選單代碼");

                entity.Property(e => e.AddEnabled)
                    .HasColumnName("Add_Enabled")
                    .HasComment("新增");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.DeleteEnabled)
                    .HasColumnName("Delete_Enabled")
                    .HasComment("刪除");

                entity.Property(e => e.DownloadEnabled)
                    .HasColumnName("Download_Enabled")
                    .HasComment("下載");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyEnabled)
                    .HasColumnName("Modify_Enabled")
                    .HasComment("修改");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.UploadEnabled)
                    .HasColumnName("Upload_Enabled")
                    .HasComment("上傳");

                entity.Property(e => e.ViewEnabled)
                    .HasColumnName("View_Enabled")
                    .HasComment("檢視");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.TbGroupRight)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_TB_GroupRight_TB_GroupInfo");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.TbGroupRight)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_TB_GroupRight_TB_MenuBack");
            });

            modelBuilder.Entity<TbHrArticle>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.Category)
                    .HasMaxLength(30)
                    .HasComment("文章所屬類別");

                entity.Property(e => e.Contents).HasComment("內容");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("標題");
            });

            modelBuilder.Entity<TbHrPackage>(entity =>
            {
                entity.HasComment("HR材料包");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayDate).HasColumnType("datetime");

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID");

                entity.Property(e => e.IsPublish).HasComment("是否上架");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(100);
            });

            modelBuilder.Entity<TbIdSummary>(entity =>
            {
                entity.HasKey(e => e.TableName)
                    .HasName("PK_TbIdSummary_1");

                entity.Property(e => e.TableName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MaxId).HasColumnName("MaxID");

                entity.Property(e => e.Prefix)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TbLanguage>(entity =>
            {
                entity.HasKey(e => e.LangId)
                    .HasName("PK_TB_Lang");

                entity.Property(e => e.LangId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("LangID");

                entity.Property(e => e.LangCode)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LangName).HasMaxLength(30);
            });

            modelBuilder.Entity<TbLog>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_TB_LoginRecord");

                entity.HasComment("系統紀錄");

                entity.Property(e => e.Pid)
                    .HasColumnName("PID")
                    .HasComment("主鍵");

                entity.Property(e => e.Action)
                    .HasMaxLength(200)
                    .HasComment("動作");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasComment("時間");

                entity.Property(e => e.Ip)
                    .HasMaxLength(50)
                    .HasColumnName("IP")
                    .HasComment("IP");

                entity.Property(e => e.Message)
                    .HasColumnType("text")
                    .HasComment("訊息");

                entity.Property(e => e.Platform)
                    .HasMaxLength(50)
                    .HasComment("位置");

                entity.Property(e => e.Url).IsUnicode(false);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasComment("登入裝置");
            });

            modelBuilder.Entity<TbLoginRecord>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_TB_S_LoginRecord");

                entity.HasComment("登入記錄");

                entity.Property(e => e.Pid)
                    .HasColumnName("PID")
                    .HasComment("主鍵");

                entity.Property(e => e.Account)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasComment("會員編號");

                entity.Property(e => e.Inputaua8)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("輸入密碼");

                entity.Property(e => e.Ip)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("IP")
                    .HasComment("IP");

                entity.Property(e => e.LoginMsg)
                    .HasMaxLength(20)
                    .HasComment("訊息");

                entity.Property(e => e.LoginTime)
                    .HasColumnType("datetime")
                    .HasComment("登入時間");

                entity.Property(e => e.MemberId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MemberID")
                    .HasComment("會員主鍵");

                entity.Property(e => e.Platform)
                    .HasMaxLength(50)
                    .HasComment("位置");

                entity.Property(e => e.Sso)
                    .HasColumnName("SSO")
                    .HasComment("是否SSO登入");

                entity.Property(e => e.Ssoresult)
                    .HasColumnName("SSOResult")
                    .HasComment("SSO回傳XMLResult");

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasComment("登入裝置");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID");
            });

            modelBuilder.Entity<TbMailLog>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_TB_ISMS_MailLog");

                entity.Property(e => e.Pid).HasColumnName("PID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Sender).HasMaxLength(100);
            });

            modelBuilder.Entity<TbMailQueue>(entity =>
            {
                entity.HasComment("信件");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("主鍵");

                entity.Property(e => e.Contents).HasComment("信件內容，可包含HTML");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FileId1)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID_1")
                    .HasComment("附件檔案");

                entity.Property(e => e.FileId2)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID_2")
                    .HasComment("附件檔案");

                entity.Property(e => e.FileId3)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID_3")
                    .HasComment("附件檔案");

                entity.Property(e => e.IsSend).HasComment("是否已發送");

                entity.Property(e => e.MailFrom)
                    .HasMaxLength(500)
                    .HasComment("寄件者");

                entity.Property(e => e.MailTo)
                    .HasMaxLength(500)
                    .HasComment("收件者，限定一位");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.PlanSendDate)
                    .HasColumnType("datetime")
                    .HasComment("預計發送日期；若無排定日期，則盡快發送");

                entity.Property(e => e.RelationId)
                    .HasMaxLength(100)
                    .HasColumnName("RelationID")
                    .HasComment("關聯資料主鍵；用於當例如取消活動時，要一併刪除使用");

                entity.Property(e => e.SendDate)
                    .HasColumnType("datetime")
                    .HasComment("實際發送日期");

                entity.Property(e => e.Subject)
                    .HasMaxLength(300)
                    .HasComment("信件主旨");
            });

            modelBuilder.Entity<TbMenuBack>(entity =>
            {
                entity.HasKey(e => e.MenuId)
                    .HasName("PK_TB_MenuBack");

                entity.HasComment("後台選單");

                entity.Property(e => e.MenuId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MenuID")
                    .HasComment("選單代碼");

                entity.Property(e => e.AddEnabled)
                    .HasColumnName("Add_Enabled")
                    .HasComment("該功能是否有\"新增\"功能");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.DeleteEnabled)
                    .HasColumnName("Delete_Enabled")
                    .HasComment("該功能是否有\"刪除\"功能");

                entity.Property(e => e.DownloadEnabled)
                    .HasColumnName("Download_Enabled")
                    .HasComment("該功能是否有\"下載\"功能");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("選單小圖示");

                entity.Property(e => e.IsDelete).HasComment("是否刪除");

                entity.Property(e => e.MenuLevel).HasComment("層級");

                entity.Property(e => e.MenuName)
                    .HasMaxLength(50)
                    .HasComment("名稱");

                entity.Property(e => e.MenuOrder).HasComment("排序");

                entity.Property(e => e.MenuParent)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("父階層");

                entity.Property(e => e.MenuUrl)
                    .HasMaxLength(255)
                    .HasColumnName("MenuURL")
                    .HasComment("超連結");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyEnabled)
                    .HasColumnName("Modify_Enabled")
                    .HasComment("該功能是否有\"修改\"功能");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.SettingFlag).HasComment("是否針對此功能項設定權限");

                entity.Property(e => e.ShowInMainSideBar).HasComment("是否顯示在左側選單");

                entity.Property(e => e.TagName)
                    .HasMaxLength(20)
                    .HasComment("標籤名稱");

                entity.Property(e => e.UploadEnabled)
                    .HasColumnName("Upload_Enabled")
                    .HasComment("該功能是否有\"上傳\"功能");

                entity.Property(e => e.ViewEnabled)
                    .HasColumnName("View_Enabled")
                    .HasComment("該功能是否有\"檢視\"功能");
            });

            modelBuilder.Entity<TbMenuFront>(entity =>
            {
                entity.HasKey(e => new { e.MenuId, e.LangId })
                    .HasName("PK_TB_Menu");

                entity.Property(e => e.MenuId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MenuID")
                    .HasComment("選單Key");

                entity.Property(e => e.LangId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("LangID");

                entity.Property(e => e.IsActive).HasComment("是否啟動");

                entity.Property(e => e.MenuIcon).HasMaxLength(255);

                entity.Property(e => e.MenuLevel).HasComment("選單層級");

                entity.Property(e => e.MenuName).HasMaxLength(255);

                entity.Property(e => e.MenuOrder).HasComment("選單排序");

                entity.Property(e => e.MenuParent)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("父選單Key");

                entity.Property(e => e.MenuUrl)
                    .HasMaxLength(255)
                    .HasColumnName("MenuURL")
                    .HasComment("選單路徑");
            });

            modelBuilder.Entity<TbNews>(entity =>
            {
                entity.HasComment("最新消息");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID")
                    .HasComment("主鍵");

                entity.Property(e => e.Category)
                    .HasMaxLength(10)
                    .HasComment("類型");

                entity.Property(e => e.Contents).HasComment("最新消息內文，包含HTML");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayDate)
                    .HasColumnType("datetime")
                    .HasComment("顯示日期");

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID")
                    .HasComment("附件主鍵編號");

                entity.Property(e => e.IsDelete).HasComment("是否註記刪除");

                entity.Property(e => e.IsKeepTop).HasComment("是否置頂顯示；若有多個置頂，則以日期降冪排序");

                entity.Property(e => e.IsPublish).HasComment("是否上架");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("標題");
            });

            modelBuilder.Entity<TbOnePage>(entity =>
            {
                entity.HasComment("一頁式頁面內容");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID");

                entity.Property(e => e.Contents).HasComment("內容");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasComment("是哪一個頁面的OnePage");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TbProfession>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.BacolId });

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("使用者主鍵");

                entity.Property(e => e.BacolId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("BacolID")
                    .HasComment("領域主鍵");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");
            });

            modelBuilder.Entity<TbProject>(entity =>
            {
                entity.HasComment("計劃管理主檔");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID");

                entity.Property(e => e.Category)
                    .HasMaxLength(20)
                    .HasComment("計畫所屬類別");

                entity.Property(e => e.Contact)
                    .HasMaxLength(100)
                    .HasComment("聯繫窗口");

                entity.Property(e => e.Contents).HasComment("內容");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Link)
                    .HasMaxLength(300)
                    .HasComment("計畫連結");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasComment("計畫名稱");

                entity.Property(e => e.Purpose).HasComment("目的");

                entity.Property(e => e.Target).HasComment("申請對象");
            });

            modelBuilder.Entity<TbProjectModify>(entity =>
            {
                entity.HasComment("課程變更管理");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(300)
                    .HasComment("聯絡信箱");

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID")
                    .HasComment("課程變更檔案");

                entity.Property(e => e.IsApprove).HasComment("是否同意; null: 尚未回覆, 1: 同意, 0:不同意");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("經辦");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ProjectID")
                    .HasComment("外鍵: 計畫編號");
            });

            modelBuilder.Entity<TbPromotion>(entity =>
            {
                entity.HasComment("推廣管理");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Applicant)
                    .HasMaxLength(20)
                    .HasComment("申請人");

                entity.Property(e => e.BusinessId)
                    .HasMaxLength(20)
                    .HasColumnName("BusinessID")
                    .HasComment("統一編號");

                entity.Property(e => e.CompanyLocation)
                    .HasMaxLength(20)
                    .HasComment("企業所在地");

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(200)
                    .HasComment("公司名稱");

                entity.Property(e => e.Email)
                    .HasMaxLength(300)
                    .HasComment("信箱");

                entity.Property(e => e.EmpoyeeAmount).HasComment("企業人數");

                entity.Property(e => e.Project)
                    .HasMaxLength(100)
                    .HasComment("計畫");
            });

            modelBuilder.Entity<TbPwdLog>(entity =>
            {
                entity.HasKey(e => e.Plid);

                entity.Property(e => e.Plid)
                    .HasColumnName("PLID")
                    .HasComment("主鍵");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("密碼");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("使用者主鍵");
            });

            modelBuilder.Entity<TbQuiz>(entity =>
            {
                entity.HasComment("問卷主檔");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID")
                    .HasComment("主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasComment("問卷描述");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasComment("問卷名稱");
            });

            modelBuilder.Entity<TbQuizOption>(entity =>
            {
                entity.HasComment("問卷選項");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.FillDirection)
                    .HasMaxLength(200)
                    .HasComment("填寫說明");

                entity.Property(e => e.Options)
                    .HasMaxLength(500)
                    .HasComment("選項們；儲存格是: {選項}|{選項}|...；順序即代表顯示順序");

                entity.Property(e => e.QuizDescription)
                    .HasMaxLength(200)
                    .HasComment("若類型為標題，則用來儲存標題列文字；若類型為其他，則儲存問題描述");

                entity.Property(e => e.QuizId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("QuizID");

                entity.Property(e => e.Type).HasComment("題目類型: 1:標題 2:簡答題 3:單選 4:複選");
            });

            modelBuilder.Entity<TbRelationLink>(entity =>
            {
                entity.HasComment("相關連結");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FileId)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("FileID");

                entity.Property(e => e.IsDelete).HasComment("是否註記刪除");

                entity.Property(e => e.IsPublish).HasComment("是否上架(true:是, false: 否)");

                entity.Property(e => e.Link)
                    .HasMaxLength(500)
                    .HasComment("超連結");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("標題");
            });

            modelBuilder.Entity<TbScheduleLog>(entity =>
            {
                entity.HasKey(e => e.Slid)
                    .HasName("PK_TB_ScheduleLog");

                entity.Property(e => e.Slid)
                    .HasColumnName("SLID")
                    .HasComment("主鍵 (yyMMddHHmm)");

                entity.Property(e => e.IsSuccess).HasComment("是否成功");

                entity.Property(e => e.ProcessingTime)
                    .HasColumnType("datetime")
                    .HasComment("執行時間");

                entity.Property(e => e.ResponseMessage)
                    .HasColumnType("text")
                    .HasComment("回傳結果");

                entity.Property(e => e.ScheduleName)
                    .HasMaxLength(50)
                    .HasComment("排程名稱");

                entity.Property(e => e.SettingId)
                    .HasColumnName("SettingID")
                    .HasComment("對應設定檔ID，TB_SY_MemberSetting.SettingID");
            });

            modelBuilder.Entity<TbScheduleLogDetail>(entity =>
            {
                entity.HasKey(e => new { e.Slid, e.AffectedId })
                    .HasName("PK_TB_ScheduleLogDetail");

                entity.Property(e => e.Slid)
                    .HasColumnName("SLID")
                    .HasComment("TB_ScheduleLog.SLID");

                entity.Property(e => e.AffectedId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("AffectedID")
                    .HasComment("受影響項目ID");

                entity.Property(e => e.AffectedAfter)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.AffectedBefore)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Message)
                    .HasColumnType("text")
                    .HasComment("詳細資訊");
            });

            modelBuilder.Entity<TbSubScript>(entity =>
            {
                entity.HasComment("訂閱服務");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.Email)
                    .HasMaxLength(500)
                    .HasComment("訂閱信箱；若重複訂閱，則自動覆蓋成最後一次選的訂閱項目");
            });

            modelBuilder.Entity<TbSystemSetting>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .HasName("PK_TB_SysSetting");

                entity.Property(e => e.Pid).HasColumnName("PID");

                entity.Property(e => e.Key)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(50);

                entity.Property(e => e.Value).IsUnicode(false);
            });

            modelBuilder.Entity<TbUserInGroup>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.GroupId })
                    .HasName("PK_UserInGroup");

                entity.HasComment("後台帳號所屬群組");

                entity.HasIndex(e => e.GroupId, "IX_TbUserInGroup_GroupID");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("使用者代碼");

                entity.Property(e => e.GroupId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("GroupID")
                    .HasComment("群組代碼");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.TbUserInGroup)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_TB_UserInGroup_TB_GroupInfo");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbUserInGroup)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TB_UserInGroup_TB_UserInfo");
            });

            modelBuilder.Entity<TbUserInfo>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_TB_UserInfo");

                entity.HasComment("使用者(後台帳號)");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("代碼");

                entity.Property(e => e.Account)
                    .HasMaxLength(50)
                    .HasComment("帳號");

                entity.Property(e => e.AccountStatusCode)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.AccountTypeCode)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Aua8)
                    .HasMaxLength(50)
                    .HasComment("密碼");

                entity.Property(e => e.CellPhone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("手機");

                entity.Property(e => e.ContactAddr)
                    .HasMaxLength(100)
                    .HasComment("通訊地址");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.Education).HasComment("學歷");

                entity.Property(e => e.EffectiveEnddate)
                    .HasColumnType("datetime")
                    .HasComment("失效日期");

                entity.Property(e => e.EffectiveStartdate)
                    .HasColumnType("datetime")
                    .HasComment("生效日期");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasComment("信箱");

                entity.Property(e => e.Expertise).HasComment("專長");

                entity.Property(e => e.IdNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("身分證");

                entity.Property(e => e.Industry)
                    .HasMaxLength(100)
                    .HasComment("產/行業");

                entity.Property(e => e.IsActive).HasComment("是否啟用");

                entity.Property(e => e.JobTitle)
                    .HasMaxLength(200)
                    .HasComment("職稱");

                entity.Property(e => e.LockTime).HasColumnType("datetime");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.PermanentAddr)
                    .HasMaxLength(100)
                    .HasComment("戶籍地址");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Photo)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasComment("照片");

                entity.Property(e => e.Seq)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceUnit)
                    .HasMaxLength(100)
                    .HasComment("服務單位");

                entity.Property(e => e.Sex)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasComment("性別");

                entity.Property(e => e.Skill)
                    .HasMaxLength(300)
                    .HasComment("專業領域, 使用逗號分隔；可能包含以下值: 組織經營,組織轉型,人才培育,職能分析,員工職涯發展,人力資源管理,勞資關係、法令");

                entity.Property(e => e.Title).HasMaxLength(30);

                entity.Property(e => e.Token)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnitA).HasMaxLength(50);

                entity.Property(e => e.UnitB).HasMaxLength(50);

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasComment("使用者名稱");
            });

            modelBuilder.Entity<TbUserInfoExperience>(entity =>
            {
                entity.HasComment("使用者經歷");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Durinration).HasMaxLength(100);

                entity.Property(e => e.JobTitle).HasMaxLength(200);

                entity.Property(e => e.ServDepartment)
                    .HasMaxLength(100)
                    .HasComment("服務/輔導單位");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("使用者主鍵");
            });

            modelBuilder.Entity<TbUserRight>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.MenuId })
                    .HasName("PK_TB_UserRight");

                entity.HasComment("使用者權限(後台帳號)");

                entity.HasIndex(e => e.MenuId, "IX_TbUserRight_MenuID");

                entity.Property(e => e.UserId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("UserID")
                    .HasComment("使用者代碼");

                entity.Property(e => e.MenuId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MenuID")
                    .HasComment("選單代碼");

                entity.Property(e => e.AddEnabled)
                    .HasColumnName("Add_Enabled")
                    .HasComment("新增");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasComment("建立日期");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("建立者");

                entity.Property(e => e.DeleteEnabled)
                    .HasColumnName("Delete_Enabled")
                    .HasComment("刪除");

                entity.Property(e => e.DownloadEnabled)
                    .HasColumnName("Download_Enabled")
                    .HasComment("下載");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasComment("修改日期");

                entity.Property(e => e.ModifyEnabled)
                    .HasColumnName("Modify_Enabled")
                    .HasComment("修改");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("修改者");

                entity.Property(e => e.UploadEnabled)
                    .HasColumnName("Upload_Enabled")
                    .HasComment("上傳");

                entity.Property(e => e.ViewEnabled)
                    .HasColumnName("View_Enabled")
                    .HasComment("檢視");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.TbUserRight)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_TB_UserRight_TB_MenuBack");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbUserRight)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TB_UserRight_TB_UserInfo");
            });

            modelBuilder.Entity<TbYouTubeVideo>(entity =>
            {
                entity.HasComment("YouTube影片");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasComment("流水號主鍵");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayDate)
                    .HasColumnType("datetime")
                    .HasComment("顯示日期");

                entity.Property(e => e.IsDelete).HasComment("是否註記刪除");

                entity.Property(e => e.IsPublish).HasComment("是否上架");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasComment("標題");

                entity.Property(e => e.YouTubeId)
                    .HasMaxLength(20)
                    .HasColumnName("YouTubeID")
                    .HasComment("YouTube連結ID，如:https://www.youtube.com/watch?v=_ztCw228eSU中的_ztCw228eSU部分");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
