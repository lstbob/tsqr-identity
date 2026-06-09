using System.Data;

namespace TSQR.Identity.Infrastructure.Data;

public static class TypeHandlerRegistrations
{
    private static bool _registered;

    public static void EnsureRegistered()
    {
        if (_registered) return;
        _registered = true;

        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
    }

    private sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
    {
        public override DateTimeOffset? Parse(object value)
        {
            if (value is DateTime dt)
                return new DateTimeOffset(dt, TimeSpan.Zero);
            return null;
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
        {
            parameter.Value = value?.UtcDateTime ?? (object)DBNull.Value;
        }
    }
}
