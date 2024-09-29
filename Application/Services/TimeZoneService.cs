using NodaTime;

namespace Application.Services;

public class TimeZoneService(IDateTimeZoneProvider dateTimeZoneProvider)
{
    public DateTime ConvertToUtc(DateTime dateTime, string timeZoneId)
    {
        var zone = dateTimeZoneProvider[timeZoneId];
        var localDateTime = LocalDateTime.FromDateTime(dateTime);
        var zonedDateTime = zone.AtLeniently(localDateTime);
        return zonedDateTime.ToDateTimeUtc();
    }

    public DateTime ConvertFromUtc(DateTime utcDateTime, string timeZoneId)
    {
        var zone = dateTimeZoneProvider[timeZoneId];
        var instant = Instant.FromDateTimeUtc(utcDateTime);
        var zonedDateTime = instant.InZone(zone);
        return zonedDateTime.ToDateTimeUnspecified();
    }

    public bool IsAvailable(Availability availability, DateTime localDateTime, string timeZoneId)
    {
        var utcDateTime = ConvertToUtc(localDateTime, timeZoneId);
        return availability.IsAvailableAt(utcDateTime);
    }

    public List<string> GetSupportedTimeZones()
    {
        return dateTimeZoneProvider.Ids.ToList();
    }
}