using System;

namespace VSBuildTimeReport.Domain
{
    public interface IDateTimeProvider
    {
        DateTime GetNow();
    }
}