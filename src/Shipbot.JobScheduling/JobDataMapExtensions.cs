using System.Collections.Generic;
using Quartz;

namespace Shipbot.JobScheduling
{
    public static class JobDataMapExtensions
    {
        internal const string JobDataKey = "7261D22B-BF60-4C6E-86A1-4A3E5ABA6034";
        
        public static T GetJobData<T>(this JobDataMap map)
        {
            if (map.ContainsKey(JobDataKey))
            {
                if (map[JobDataKey] is T actual)
                    return actual;
            }
            
            throw new KeyNotFoundException();
        } 
    }
}