using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Application.UnitTests.Services;

[TestClass]
public class TimeZoneServiceTests
{
    private TimeZoneService _timeZoneService = null!;
    private readonly ILogger<TimeZoneService> _loggerMock = Substitute.For<ILogger<TimeZoneService>>();

    [TestInitialize]
    public void TestInitialize()
    {
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));
        _timeZoneService = new TimeZoneService(timeProvider, _loggerMock);
    }

    [TestMethod]
    public void ConvertToUtc_FromLondon_ShouldConvertCorrectly()
    {
        // Arrange
        var londonTime = new DateTime(2023, 6, 1, 12, 0, 0);

        // Act
        var utcTime = _timeZoneService.ConvertToUtc(londonTime, "Europe/London");

        // Assert
        utcTime.Should().Be(new DateTime(2023, 6, 1, 11, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void ConvertFromUtc_ToLondon_ShouldConvertCorrectly()
    {
        // Arrange
        var utcTime = new DateTime(2023, 6, 1, 11, 0, 0, DateTimeKind.Utc);

        // Act
        var londonTime = _timeZoneService.ConvertFromUtc(utcTime, "Europe/London");

        // Assert
        londonTime.Should().Be(new DateTime(2023, 6, 1, 12, 0, 0));
    }

    [TestMethod]
    public void ConvertToUtc_FromDubai_ShouldConvertCorrectly()
    {
        // Arrange
        var dubaiTime = new DateTime(2023, 6, 1, 16, 0, 0);

        // Act
        var utcTime = _timeZoneService.ConvertToUtc(dubaiTime, "Asia/Dubai");

        // Assert
        utcTime.Should().Be(new DateTime(2023, 6, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void ConvertFromUtc_ToDubai_ShouldConvertCorrectly()
    {
        // Arrange
        var utcTime = new DateTime(2023, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var dubaiTime = _timeZoneService.ConvertFromUtc(utcTime, "Asia/Dubai");

        // Assert
        dubaiTime.Should().Be(new DateTime(2023, 6, 1, 16, 0, 0));
    }

    [TestMethod]
    public void ConvertTimeToUtc_FromLondon_ShouldConvertCorrectly()
    {
        // Arrange
        var londonTime = new TimeSpan(12, 0, 0);

        // Act
        var utcTime = _timeZoneService.ConvertTimeToUtc(londonTime, "Europe/London", DayOfWeek.Thursday);

        // Assert
        utcTime.Should().Be(new TimeSpan(11, 0, 0));
    }

    [TestMethod]
    public void ConvertTimeFromUtc_ToLondon_ShouldConvertCorrectly()
    {
        // Arrange
        var utcTime = new TimeSpan(11, 0, 0);

        // Act
        var londonTime = _timeZoneService.ConvertTimeFromUtc(utcTime, "Europe/London", DayOfWeek.Thursday);

        // Assert
        londonTime.Should().Be(new TimeSpan(12, 0, 0));
    }

    [TestMethod]
    public void ConvertToUtc_DuringDST_ShouldConvertCorrectly()
    {
        // Arrange
        var londonDSTTime = new DateTime(2023, 7, 1, 12, 0, 0);

        // Act
        var utcTime = _timeZoneService.ConvertToUtc(londonDSTTime, "Europe/London");

        // Assert
        utcTime.Should().Be(new DateTime(2023, 7, 1, 11, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void ConvertFromUtc_DuringDST_ShouldConvertCorrectly()
    {
        // Arrange
        var utcTime = new DateTime(2023, 7, 1, 11, 0, 0, DateTimeKind.Utc);

        // Act
        var londonDSTTime = _timeZoneService.ConvertFromUtc(utcTime, "Europe/London");

        // Assert
        londonDSTTime.Should().Be(new DateTime(2023, 7, 1, 12, 0, 0));
    }

    [TestMethod]
    public void ConvertToUtc_DSTTransition_ShouldHandleCorrectly()
    {
        // Arrange
        var londonPreDST = new DateTime(2023, 3, 26, 0, 59, 59);
        var londonPostDST = new DateTime(2023, 3, 26, 2, 0, 0);

        // Act
        var utcPreDST = _timeZoneService.ConvertToUtc(londonPreDST, "Europe/London");
        var utcPostDST = _timeZoneService.ConvertToUtc(londonPostDST, "Europe/London");

        // Assert
        utcPreDST.Should().Be(new DateTime(2023, 3, 26, 0, 59, 59, DateTimeKind.Utc));
        utcPostDST.Should().Be(new DateTime(2023, 3, 26, 1, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void ConvertFromUtc_DSTTransition_ShouldHandleCorrectly()
    {
        // Arrange
        var utcPreDST = new DateTime(2023, 3, 26, 0, 59, 59, DateTimeKind.Utc);
        var utcPostDST = new DateTime(2023, 3, 26, 1, 0, 0, DateTimeKind.Utc);

        // Act
        var londonPreDST = _timeZoneService.ConvertFromUtc(utcPreDST, "Europe/London");
        var londonPostDST = _timeZoneService.ConvertFromUtc(utcPostDST, "Europe/London");

        // Assert
        londonPreDST.Should().Be(new DateTime(2023, 3, 26, 0, 59, 59));
        londonPostDST.Should().Be(new DateTime(2023, 3, 26, 2, 0, 0));
    }
    
    [TestMethod]
    public void ConvertTimeToUtc_AllDaysOfWeek_ShouldConvertCorrectly()
    {
        // Arrange
        var londonTime = new TimeSpan(12, 0, 0);

        // Act & Assert
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var utcTime = _timeZoneService.ConvertTimeToUtc(londonTime, "Europe/London", day);
            utcTime.Should().Be(new TimeSpan(11, 0, 0), $"Failed for {day}");
        }
    }
    
      [TestMethod]
    public void ConvertTimeFromUtc_AllDaysOfWeek_ShouldConvertCorrectly()
    {
        // Arrange
        var utcTime = new TimeSpan(11, 0, 0);

        // Act & Assert
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var londonTime = _timeZoneService.ConvertTimeFromUtc(utcTime, "Europe/London", day);
            londonTime.Should().Be(new TimeSpan(12, 0, 0), $"Failed for {day}");
        }
    }

    [TestMethod]
    public void ConvertTimeToUtc_MidnightCrossover_ShouldHandleCorrectly()
    {
        // Arrange
        var lateNightTime = new TimeSpan(23, 30, 0);

        // Act
        var utcTime = _timeZoneService.ConvertTimeToUtc(lateNightTime, "America/New_York", DayOfWeek.Saturday);

        // Assert
        utcTime.Should().Be(new TimeSpan(3, 30, 0)); // 23:30 New York time on Saturday is 03:30 UTC on Sunday
    }

    [TestMethod]
    public void ConvertTimeFromUtc_MidnightCrossover_ShouldHandleCorrectly()
    {
        // Arrange
        var earlyMorningUtc = new TimeSpan(3, 30, 0);

        // Act
        var newYorkTime = _timeZoneService.ConvertTimeFromUtc(earlyMorningUtc, "America/New_York", DayOfWeek.Sunday);

        // Assert
        newYorkTime.Should().Be(new TimeSpan(23, 30, 0)); // 03:30 UTC on Sunday is 23:30 New York time on Saturday
    }

    [TestMethod]
    public void ConvertToUtc_InvalidTimeZone_ShouldThrowException()
    {
        // Arrange
        var time = new DateTime(2023, 6, 1, 12, 0, 0);

        // Act & Assert
        Action act = () => _timeZoneService.ConvertToUtc(time, "Invalid/TimeZone");
        act.Should().Throw<TimeZoneNotFoundException>();
    }

    [TestMethod]
    public void ConvertFromUtc_InvalidTimeZone_ShouldThrowException()
    {
        // Arrange
        var time = new DateTime(2023, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Action act = () => _timeZoneService.ConvertFromUtc(time, "Invalid/TimeZone");
        act.Should().Throw<TimeZoneNotFoundException>();
    }

    [TestMethod]
    public void ConvertTimeToUtc_DSTTransition_ShouldHandleCorrectly()
    {
        // Arrange
        var dstTransitionDate = new DateTime(2023, 3, 26); // Date of DST transition in 2023 for Europe/London
        var timeBeforeDST = new TimeSpan(1, 0, 0);
        var timeAfterDST = new TimeSpan(3, 0, 0);

        // Act
        var utcBeforeDST = _timeZoneService.ConvertTimeToUtc(timeBeforeDST, "Europe/London", dstTransitionDate.DayOfWeek);
        var utcAfterDST = _timeZoneService.ConvertTimeToUtc(timeAfterDST, "Europe/London", dstTransitionDate.DayOfWeek);

        // Assert
        utcAfterDST.Should().Be(utcBeforeDST + TimeSpan.FromHours(2),
            "After DST transition, local time 3:00 AM should be 2 hours ahead of 1:00 AM in UTC");

        Console.WriteLine($"Before DST: {timeBeforeDST} London time is {utcBeforeDST} UTC");
        Console.WriteLine($"After DST: {timeAfterDST} London time is {utcAfterDST} UTC");
    }

    [TestMethod]
    public void ConvertTimeFromUtc_DSTTransition_ShouldHandleCorrectly()
    {
        // Arrange
        var dstTransitionDate = new DateTime(2023, 3, 26); // Date of DST transition in 2023 for Europe/London
        var utcBeforeDST = new TimeSpan(0, 30, 0);
        var utcAtDST = new TimeSpan(1, 0, 0);
        var utcAfterDST = new TimeSpan(2, 30, 0);

        // Act
        var londonBeforeDST = _timeZoneService.ConvertTimeFromUtc(utcBeforeDST, "Europe/London", dstTransitionDate.DayOfWeek);
        var londonAtDST = _timeZoneService.ConvertTimeFromUtc(utcAtDST, "Europe/London", dstTransitionDate.DayOfWeek);
        var londonAfterDST = _timeZoneService.ConvertTimeFromUtc(utcAfterDST, "Europe/London", dstTransitionDate.DayOfWeek);

        // Assert
        londonBeforeDST.Should().Be(new TimeSpan(1, 30, 0), "0:30 UTC should be 1:30 London time before DST");
        londonAtDST.Should().Be(new TimeSpan(2, 0, 0), "1:00 UTC should be 2:00 London time at DST transition");
        londonAfterDST.Should().Be(new TimeSpan(3, 30, 0), "2:30 UTC should be 3:30 London time after DST");

        Console.WriteLine($"Before DST: {utcBeforeDST} UTC is {londonBeforeDST} London time");
        Console.WriteLine($"At DST transition: {utcAtDST} UTC is {londonAtDST} London time");
        Console.WriteLine($"After DST: {utcAfterDST} UTC is {londonAfterDST} London time");
    }

    [TestMethod]
    public void GetTimeZoneByCountryCode_ValidCode_ShouldReturnCorrectTimeZone()
    {
        // Act
        var timeZone = TimeZoneService.GetTimeZoneByCountryCode("826");

        // Assert
        timeZone.Should().Be("Europe/London");
    }

    [TestMethod]
    public void GetTimeZoneByCountryCode_InvalidCode_ShouldReturnNull()
    {
        // Act
        var timeZone = TimeZoneService.GetTimeZoneByCountryCode("999");

        // Assert
        timeZone.Should().BeNull();
    }
}