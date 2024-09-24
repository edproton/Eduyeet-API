using System;

namespace API.Utils;

public readonly struct MemorySize : IFormattable
{
    private static readonly string[] SizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

    public long Bytes { get; }

    private MemorySize(long bytes)
    {
        Bytes = bytes;
    }

    public static MemorySize FromBytes(long bytes) => new(bytes);
    public static MemorySize FromKilobytes(long kb) => new(kb * 1024);
    public static MemorySize FromMegabytes(long mb) => new(mb * 1024 * 1024);
    public static MemorySize FromGigabytes(long gb) => new(gb * 1024 * 1024 * 1024);

    public static MemorySize operator +(MemorySize a, MemorySize b) => new(a.Bytes + b.Bytes);
    public static MemorySize operator -(MemorySize a, MemorySize b) => new(a.Bytes - b.Bytes);
    public static MemorySize operator *(MemorySize a, int multiplier) => new(a.Bytes * multiplier);
    public static MemorySize operator /(MemorySize a, int divisor) => new(a.Bytes / divisor);

    public static bool operator <(MemorySize left, MemorySize right) => left.Bytes < right.Bytes;
    public static bool operator >(MemorySize left, MemorySize right) => left.Bytes > right.Bytes;
    public static bool operator <=(MemorySize left, MemorySize right) => left.Bytes <= right.Bytes;
    public static bool operator >=(MemorySize left, MemorySize right) => left.Bytes >= right.Bytes;

    public override string ToString()
    {
        return ToString(null, null);
    }

    public string ToString(MemorySizeFormat format)
    {
        return ToString(format, null);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
        {
            if (Bytes == 0)
                return "0 B";

            int magnitude = (int)Math.Log(Bytes, 1024);
            decimal adjustedSize = (decimal)Bytes / (1L << (magnitude * 10));

            return $"{adjustedSize:n2} {SizeSuffixes[magnitude]}";
        }

        if (Enum.TryParse<MemorySizeFormat>(format, true, out var memoryFormat))
        {
            return ToString(memoryFormat, formatProvider);
        }

        throw new FormatException($"The {format} format string is not supported.");
    }

    public string ToString(MemorySizeFormat format, IFormatProvider? formatProvider)
    {
        return format switch
        {
            MemorySizeFormat.B => $"{Bytes} B",
            MemorySizeFormat.KB => $"{Bytes / 1024.0:n2} KB",
            MemorySizeFormat.MB => $"{Bytes / (1024.0 * 1024):n2} MB",
            MemorySizeFormat.GB => $"{Bytes / (1024.0 * 1024 * 1024):n2} GB",
            _ => throw new FormatException($"The {format} format is not supported.")
        };
    }
}

public enum MemorySizeFormat
{
    B,
    KB,
    MB,
    GB
}

public static class MemorySizeExtensions
{
    public static MemorySize GB(this int value) => MemorySize.FromGigabytes(value);
    public static MemorySize MB(this int value) => MemorySize.FromMegabytes(value);
    public static MemorySize KB(this int value) => MemorySize.FromKilobytes(value);
    public static MemorySize Bytes(this int value) => MemorySize.FromBytes(value);
}