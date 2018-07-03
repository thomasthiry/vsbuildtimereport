using System;

namespace VSBuildTimeReport.Domain
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeProvider()
        {
            
        }

        public DateTime GetNow()
        {
            return DateTime.Now;
        }
    }
}