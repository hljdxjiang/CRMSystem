using System;
namespace JHC.ToolKit.Extensions
{
    public static class DateTimeExtentions
    {
        public static DateTime ToCstTime(this DateTime time)
        {
            return TimeUtil.GetCstDateTime();
        }
    }
}
