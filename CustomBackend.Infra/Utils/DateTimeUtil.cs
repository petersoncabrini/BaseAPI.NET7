using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics;
using System.Globalization;
using TimeZoneConverter;

namespace CustomBackend.Infra.Utils
{
    public static class DateTimeUtil
    {
        private static string brazilTimezoneId = "E. South America Standard Time";

        public static DateTime UtcToBrazil(this DateTime dataHoraUtc) => TimeZoneInfo.ConvertTimeFromUtc(dataHoraUtc, TZConvert.GetTimeZoneInfo(brazilTimezoneId));

        public static DateTime GetDateTimeFromBrazil() => UtcToBrazil(DateTime.UtcNow);

        public static string FormatDate(this DateTime data) => data.ToString("dd/MM/yyyy");

        public static string FormatDateTime(this DateTime data) => data.ToString("dd/MM/yyyy HH:mm:ss");

        public static string FormatDateTimeFull(this DateTime data) => data.ToString("dd/MM/yyyy HH:mm:ss:ffffff");

        public static DateTime? ToDateTime(this string value, string format = null)
        {
            try
            {
                if (value == null)
                    return null;

                if (format == null)
                    return Convert.ToDateTime(value);

                var result = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
                return result;
            }
            catch
            {
                Debug.WriteLine($"Erro ao converter data {value}; formato {format}");
                return null;
            }

        }

        public static DateTime? TryGetDate(this object value)
        {
            if (value == null)
                return null;

            try
            {
                return DateTime.ParseExact(value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        public static string FormatDateTime(this DateTime value, string dateFormat) => value.ToString(dateFormat);

        public static void MapDatesAsUtc<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class
        {
            typeof(TEntity)
                            .GetProperties()
                            .Where(e => new[] { typeof(DateTime), typeof(DateTime?) }
                            .Contains(e.PropertyType))
                            .ToList()
                            .ForEach(e =>
                            {
                                if (Nullable.GetUnderlyingType(e.PropertyType) != null)
                                    builder.Property<DateTime?>(e.Name).HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
                                else
                                    builder.Property<DateTime>(e.Name).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                            });
        }

        public static PropertyBuilder<DateTime> ToUtc(this PropertyBuilder<DateTime> p)
        {
            p.HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            return p;
        }

        public static PropertyBuilder<DateTime?> ToUtc(this PropertyBuilder<DateTime?> p)
        {
            p.HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
            return p;
        }
    }
}
