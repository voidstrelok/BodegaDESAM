using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BodegaDESAM
{
    public class DateOnlyValueConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyValueConverter() : base(
            dateonly => dateonly.ToDateTime(TimeOnly.MinValue),
            datetime => DateOnly.FromDateTime(datetime))
        {

        }
    }
}
