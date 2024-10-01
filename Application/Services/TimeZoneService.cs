using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class TimeZoneService
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TimeZoneService> _logger;
    private static readonly ConcurrentDictionary<string, TimeZoneInfo> TimeZoneCache = new();

    private static readonly IDictionary<string, string> CountryCodesTimezones = new Dictionary<string, string>
    {
        { "826", "Europe/London" },
        { "784", "Asia/Dubai" },
        { "156", "Asia/Shanghai" },
        { "620", "Europe/Lisbon" }
    };

    public TimeZoneService(TimeProvider timeProvider, ILogger<TimeZoneService> logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public static string? GetTimeZoneByCountryCode(string countryCode)
    {
        return CountryCodesTimezones.TryGetValue(countryCode, out var timeZone) ? timeZone : null;
    }

    public DateTime ConvertToUtc(DateTime localTime, string timeZoneId)
    {
        var timeZoneInfo = GetTimeZoneInfo(timeZoneId);
        return TimeZoneInfo.ConvertTimeToUtc(localTime, timeZoneInfo);
    }

    public DateTime ConvertFromUtc(DateTime utcTime, string timeZoneId)
    {
        var timeZoneInfo = GetTimeZoneInfo(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
    }
    
    public DateTimeOffset ConvertFromUtc(DateTimeOffset utcTime, string timeZoneId)
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTime(utcTime, timeZoneInfo);
    }

    public TimeSpan ConvertTimeToUtc(TimeSpan localTime, string timeZoneId, DayOfWeek dayOfWeek)
    {
        var timeZoneInfo = GetTimeZoneInfo(timeZoneId);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var targetDate = GetTargetDate(now, dayOfWeek);

        var localDateTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, localTime.Hours, localTime.Minutes, localTime.Seconds, DateTimeKind.Unspecified);

        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZoneInfo);

        return utcDateTime.TimeOfDay;
    }

    public TimeSpan ConvertTimeFromUtc(TimeSpan utcTime, string timeZoneId, DayOfWeek dayOfWeek)
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var targetDateUtc = GetTargetDate(now, dayOfWeek);

        var utcDateTime = new DateTimeOffset(targetDateUtc.Date + utcTime, TimeSpan.Zero);
        var localDateTime = TimeZoneInfo.ConvertTime(utcDateTime, timeZoneInfo);

        return localDateTime.TimeOfDay;
    }

    private DateTime GetTargetDate(DateTime referenceDate, DayOfWeek targetDayOfWeek)
    {
        int daysToAdd = ((int)targetDayOfWeek - (int)referenceDate.DayOfWeek + 7) % 7;
        return referenceDate.AddDays(daysToAdd);
    }

    private TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("Time zone ID cannot be null or empty.", nameof(timeZoneId));
        }

        return TimeZoneCache.GetOrAdd(timeZoneId, id =>
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogError("Time zone not found: {TimeZoneId}", id);
                throw;
            }
            catch (InvalidTimeZoneException)
            {
                _logger.LogError("Invalid time zone: {TimeZoneId}", id);
                throw;
            }
        });
    }

    // Method to update country code to time zone mapping
    public static void UpdateCountryCodeTimeZone(string countryCode, string timeZoneId)
    {
        CountryCodesTimezones[countryCode] = timeZoneId;
    }
}