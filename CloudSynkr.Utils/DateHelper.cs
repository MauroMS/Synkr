namespace CloudSynkr.Utils;

public static class DateHelper
{
    public static bool CheckIfDateIsNewer(DateTimeOffset? date1, DateTimeOffset? date2)
    {
        if (!date1.HasValue)
            return false;

        if (!date2.HasValue)
            return true;

        return DateTimeOffset.Compare(date1.Value, date2.Value) == 1;
    }
}