using System;
using System.Collections.Generic;
using System.Text;

namespace DesertOctopus.MammothCache.Providers
{
    internal class DateNowProvider : ICurrentDateAndTimeProvider
    {
        public static ICurrentDateAndTimeProvider Instance => new DateNowProvider();

        public DateTime GetCurrentDateTime()
        {
            return DateTime.UtcNow;
        }
    }
}
