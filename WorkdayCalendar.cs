namespace Test;

public class WorkdayCalendar : IWorkdayCalendar
    {
        private List<DateTime> holidays = new List<DateTime>();
        private List<(int month, int day)> recurringHolidays = new List<(int month, int day)>();
        private TimeSpan start;
        private TimeSpan stop;

        public void SetHoliday(DateTime date)
        {
            holidays.Add(date.Date);
        }

        public void SetRecurringHoliday(int month, int day)
        {
            recurringHolidays.Add((month, day));
        }

        public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
        {
            start = new TimeSpan(startHours, startMinutes, 0);
            stop = new TimeSpan(stopHours, stopMinutes, 0);
        }

        public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkdays)
        {
            DateTime offSetStart = OffSetBusinessHours(startDate);

            double offSetMinutes = (stop - start).TotalMinutes;

            double totalOffSet = (double)incrementInWorkdays * offSetMinutes;

            DateTime result = CalculateBusinessHoursIncrement(offSetStart, totalOffSet);

            return result;
        }

        private DateTime OffSetBusinessHours(DateTime date)
        {
            var timeOfDay = date.TimeOfDay;
            if (timeOfDay < start)
            {
                return date.Date + start;
            }
            if (timeOfDay > stop)
            {
                DateTime nextWorkday = date.Date.AddDays(1).Date + start;
                while (IsHoliday(nextWorkday) || IsWeekend(nextWorkday))
                {
                    nextWorkday = nextWorkday.AddDays(1);
                }
                return nextWorkday;
            }
            return date;
        }

        private DateTime CalculateBusinessHoursIncrement(DateTime startDate, double totalMinutesToMove)
        {
            DateTime currentDate = startDate;
            double remainingMinutes = totalMinutesToMove;

            while (remainingMinutes != 0)
            {
                double minutesToday = (stop - currentDate.TimeOfDay).TotalMinutes;
                if (remainingMinutes > 0)
                {
                    if (remainingMinutes <= minutesToday)
                    {
                        return currentDate.AddMinutes(remainingMinutes);
                    }
                    remainingMinutes -= minutesToday;
                    currentDate = GetNextWorkday(currentDate).Date + start;
                }
                else
                {
                    if (-remainingMinutes <= (currentDate.TimeOfDay - start).TotalMinutes)
                    {
                        return currentDate.AddMinutes(remainingMinutes);
                    }
                    remainingMinutes += (currentDate.TimeOfDay - start).TotalMinutes;
                    currentDate = GetPreviousWorkday(currentDate).Date + stop;
                }
            }

            return currentDate;
        }

        private DateTime GetNextWorkday(DateTime date)
        {
            DateTime nextDay = date.Date.AddDays(1);
            while (IsHoliday(nextDay) || IsWeekend(nextDay))
            {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        private DateTime GetPreviousWorkday(DateTime date)
        {
            DateTime previousDay = date.Date.AddDays(-1);
            while (IsHoliday(previousDay) || IsWeekend(previousDay))
            {
                previousDay = previousDay.AddDays(-1);
            }
            return previousDay;
        }

        private bool IsHoliday(DateTime date)
        {
            return holidays.Contains(date.Date) || recurringHolidays.Contains((date.Month, date.Day));
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }   
        
    }
