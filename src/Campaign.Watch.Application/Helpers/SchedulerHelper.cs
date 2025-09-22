using NCrontab;
using System;

namespace Campaign.Watch.Application.Helpers
{
    public static class SchedulerHelper
    {
        public static DateTime? GetNextExecution(string crontabExpression)
        {
            if (string.IsNullOrWhiteSpace(crontabExpression))
            {
                return null;
            }

            try
            {
                var schedule = CrontabSchedule.Parse(crontabExpression);
                return schedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch (CrontabException)
            {
                return null;
            }
        }
    }
}
