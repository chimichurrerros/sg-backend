namespace BackEnd.Utils;

public static class DateTimeUtils
{
    public static DateTime AddWorkingDays(DateTime date, int days)
    {
        var sign = days >= 0 ? 1 : -1;
        var absDays = Math.Abs(days);
        var current = date;
        var added = 0;

        while (added < absDays)
        {
            current = current.AddDays(sign);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                added++;
        }

        return current;
    }

    public static int WorkingDaysToCalendarDays(int workingDays)
    {
        var weeks = workingDays / 5;
        var extra = workingDays % 5;
        return weeks * 7 + extra;
    }
}
