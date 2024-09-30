using NodaTime;

namespace Application.Services;

public class TimeZoneService(IDateTimeZoneProvider dateTimeZoneProvider)
{
    private static IDictionary<string, string> CountryCodesTimezones = new Dictionary<string, string>
    {
        { "826", "Europe/London" }, // United Kingdom
        { "784", "Asia/Dubai" }, // United Arab Emirates
        { "156", "Asia/Shanghai" }, // China (using the time zone for Beijing)
        { "620", "Europe/Lisbon" } // Portugal
    };

    private readonly IDateTimeZoneProvider _dateTimeZoneProvider = dateTimeZoneProvider
                                                                   ?? throw new ArgumentNullException(
                                                                       nameof(dateTimeZoneProvider));
    
    public static string? GetTimeZoneId(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return null;

        return !CountryCodesTimezones.TryGetValue(countryCode, out var timeZoneId) ? null : timeZoneId;
    }

    public DateTime ConvertToUtc(DateTime dateTime, string timeZoneId)
    {
        var zone = GetTimeZone(timeZoneId);
        var localDateTime = LocalDateTime.FromDateTime(dateTime);
        var zonedDateTime = zone.AtLeniently(localDateTime);
        return zonedDateTime.ToDateTimeUtc();
    }

    public DateTime ConvertFromUtc(DateTime utcDateTime, string timeZoneId)
    {
        var zone = GetTimeZone(timeZoneId);
        var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc));
        var zonedDateTime = instant.InZone(zone);
        return zonedDateTime.ToDateTimeUnspecified();
    }

    public bool IsAvailable(
        Availability availability,
        DateTime localDateTime,
        string timeZoneId)
    {
        if (availability == null)
            throw new ArgumentNullException(nameof(availability));

        var utcDateTime = ConvertToUtc(localDateTime, timeZoneId);
        return availability.IsAvailableAt(utcDateTime); // Assuming Availability has this method
    }

    public IReadOnlyList<string> GetSupportedTimeZones()
    {
        return _dateTimeZoneProvider.Ids.ToList().AsReadOnly();
        // Return a read-only list for safety
    }

    public TimeSpan ConvertTimeToUtc(
        TimeSpan localTime,
        string timeZoneId,
        DayOfWeek dayOfWeek)
    {
        var zone = GetTimeZone(timeZoneId);
        var now = SystemClock.Instance.GetCurrentInstant();
        var todayInZone = now.InZone(zone).Date;

        var targetDate = todayInZone.Next((IsoDayOfWeek)((int)dayOfWeek + 1));

        var localDateTime = targetDate +
                            new LocalTime(localTime.Hours,
                                localTime.Minutes,
                                localTime.Seconds,
                                localTime.Milliseconds);

        var utcDateTime = zone.AtLeniently(localDateTime).ToInstant().ToDateTimeUtc();
        return utcDateTime.TimeOfDay;
    }

    public TimeSpan ConvertTimeFromUtc(
        TimeSpan utcTime,
        string timeZoneId,
        DayOfWeek dayOfWeek)
    {
        var zone = GetTimeZone(timeZoneId);
        var now = SystemClock.Instance.GetCurrentInstant();
        var todayInUtc = now.InUtc().Date;

        var targetDateUtc = todayInUtc.Next((IsoDayOfWeek)((int)dayOfWeek + 1));

        var utcDateTime = targetDateUtc.AtStartOfDayInZone(DateTimeZone.Utc)
            .PlusTicks(utcTime.Ticks);
        var localDateTime = utcDateTime.WithZone(zone);

        return new TimeSpan(
            localDateTime.TimeOfDay.Hour,
            localDateTime.TimeOfDay.Minute,
            localDateTime.TimeOfDay.Second);
    }

    private DateTimeZone GetTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Time zone ID cannot be null or empty.", nameof(timeZoneId));

        if (!_dateTimeZoneProvider.Ids.Contains(timeZoneId))
            throw new ArgumentException($"Invalid time zone ID: {timeZoneId}", nameof(timeZoneId));

        return _dateTimeZoneProvider[timeZoneId];
    }
}