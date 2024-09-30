using Application.Services;

namespace Application.Extensions;

public static class TimezoneExtensions
{
    public static DateTime MapToUserTimezone(this DateTime dateTime, string fromTimezoneId, string toTimezoneId, TimeZoneService timeZoneService)
    {
        var utcDateTime = timeZoneService.ConvertToUtc(dateTime, fromTimezoneId);

        return timeZoneService.ConvertFromUtc(utcDateTime, toTimezoneId);
    }
}
