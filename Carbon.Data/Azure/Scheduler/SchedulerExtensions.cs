using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Scheduler.Models;
using NCrontab;

namespace Carbon.Data.Azure.Scheduler
{
    public static class SchedulerExtensions
    {
        public static JobRecurrence InitializeFromCron(this JobRecurrence recurrence, string cronExpression, DateTime? startDate = null)
        {
            var schedule = CrontabSchedule.Parse(cronExpression);
            var minutes = new HashSet<int>();
            var hours = new HashSet<int>();
            var monthDays = new HashSet<int>();
            var months = new HashSet<int>();
            var days = new HashSet<JobScheduleDay>();
            startDate = startDate ?? DateTime.UtcNow;
            
            var occurences = schedule.GetNextOccurrences(startDate.Value, startDate.Value.AddYears(1));
            foreach (var next in occurences)
            {                
                minutes.Add(next.Minute);
                hours.Add(next.Hour);
                monthDays.Add(next.Day);
                months.Add(next.Month);
                days.Add((JobScheduleDay)next.DayOfWeek);                
            }

            recurrence.Interval = 1;
            recurrence.Frequency = JobRecurrenceFrequency.Month;
            recurrence.Schedule = new JobRecurrenceSchedule
            {
                MonthDays = monthDays.ToList(),
                Hours = hours.ToList(),
                Minutes = minutes.ToList()
            };

            return recurrence;
        }
    }
}