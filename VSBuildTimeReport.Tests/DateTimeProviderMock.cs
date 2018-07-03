using System;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport.Tests
{
    public class DateTimeProviderMock : IDateTimeProvider
    {
        private readonly DateTime _dateTime;

        public DateTimeProviderMock(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        public DateTime GetNow()
        {
            return _dateTime;
        }
    }
}