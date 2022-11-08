using BASE.Service;
using Quartz;

namespace BASE.Scheduler
{
    /// <summary>
    /// 測試用
    /// </summary>
    /// [DisallowConcurrentExecution]: 防止相同 Job 並發
    [DisallowConcurrentExecution]
    public class HelloWorldJob : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger;
        private readonly AllCommonService _allCommonService;
        public HelloWorldJob(ILogger<HelloWorldJob> logger,
                             AllCommonService allCommonService)
        {
            _logger = logger;
            _allCommonService = allCommonService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello world!");
            _allCommonService.TestRecord("API", "Test_Scheduler", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Wait();
            
            return Task.CompletedTask;
        }
    }

}
