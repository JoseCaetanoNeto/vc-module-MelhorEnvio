using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CartModule.Core;
using VirtoCommerce.Platform.Core.Settings;

namespace vc_module_MelhorEnvio.Data.BackgroundJobs
{
    public class TrackingJob
    {
        private readonly ILogger _log;

        public TrackingJob(ILogger<TrackingJob> log)
        {
            _log = log;
        }

        [DisableConcurrentExecution(10)]
        public Task Process()
        {
            _log.LogTrace($"Start processing DeleteObsoleteCartsJob job");


            _log.LogTrace($"Complete processing DeleteObsoleteCartsJob job");

            return Task.CompletedTask;
        }
    }
}
