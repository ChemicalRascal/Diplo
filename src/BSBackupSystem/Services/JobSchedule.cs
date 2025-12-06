using BSBackupSystem.Jobs;
using Quartz;

namespace BSBackupSystem.Services;

public class JobSchedule()
{
    public static IEnumerable<(Type jobType, string cronSchedule)> GetJobs()
    {
        yield return (typeof(ScrapingJob), "0 */2 * * * ?");
        yield break;
    }
}

public static class QuartzExtensions
{
    extension(IServiceCollectionQuartzConfigurator q)
    {
        public void RegisterCoreJobs()
        {
            foreach (var job in JobSchedule.GetJobs())
            {
                var jobKey = new JobKey(job.jobType.FullName!);

                q.AddJob(job.jobType, jobKey, c =>
                {
                    c.DisallowConcurrentExecution();
                });

                q.AddTrigger(c =>
                {
                    c.ForJob(jobKey);
                    c.StartNow().WithCronSchedule(job.cronSchedule);
                });
            }
        }
    }
}
