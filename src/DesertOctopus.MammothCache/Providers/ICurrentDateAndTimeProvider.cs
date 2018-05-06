using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Providers
{
    internal interface ICurrentDateAndTimeProvider
    {
        DateTime GetCurrentDateTime();
    }
}