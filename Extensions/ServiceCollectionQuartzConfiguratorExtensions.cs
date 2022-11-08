using BASE.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace BASE.Extensions
{
    /// <summary>
    /// Quartz 擴充
    /// sample code:
    /// 檢查傳入 string 是否有為 null or empty 的：StringExtensions.CheckIsNullOrEmpty(param1, param2, ...);
    /// </summary>
    public static class ServiceCollectionQuartzConfiguratorExtensions
    {
        /// <summary>
        /// 建立排程器，並採用 appsetting.json 中 設定的 Quartz.JobName 的 Cron Expression 時間
        /// 需要於 appsetting.json 設定 
        /// "Quartz": {
        ///     "HelloWorldJob": "0/5 * * * * ?",
        ///     // "Job 名稱": "CronExpression"
        /// }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="quartz"></param>
        /// <param name="config">appsetting.json Configuration</param>
        /// <exception cref="Exception"></exception>
        public static void AddJobAndTrigger<T>(
                this IServiceCollectionQuartzConfigurator quartz,
                IConfiguration config)
                where T : IJob
        {
            // Job 名稱 =  appsettings.json key 名稱 = class 名稱
            string jobName = typeof(T).Name;

            // 讀取 config
            var configKey = $"Quartz:{jobName}";
            var cronExpression = config[configKey];

            // 如果沒讀取到對應 Job 設定 Cron，throw
            if (string.IsNullOrEmpty(cronExpression))
            {
                throw new Exception($"No Quartz.NET Cron schedule found for job in configuration at {configKey}");
            }

            // 使用 Job 名稱 當作 Key 名稱 
            var jobKey = new JobKey(jobName);

            // 建立 Job
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            // 建立 Trigger
            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-trigger")
                .WithSchedule(CronScheduleBuilder
                             .CronSchedule(cronExpression)
                             .WithMisfireHandlingInstructionIgnoreMisfires()));
        }

        /// <summary>
        /// 僅建立Job，回傳排程器做後續設定(只設定任務，未設定時間)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="quartz"></param>
        /// <param name="config">appsetting.json Configuration</param>
        /// <exception cref="Exception"></exception>
        public static IServiceCollectionQuartzConfigurator AddJob<T>(
                this IServiceCollectionQuartzConfigurator quartz)
                where T : IJob
        {
            // Job 名稱 =  appsettings.json key 名稱 = class 名稱
            string jobName = typeof(T).Name;

            // 使用 Job 名稱 當作 Key 名稱 
            var jobKey = new JobKey(jobName);

            // 建立 Job
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            return quartz;
        }

    }

}
