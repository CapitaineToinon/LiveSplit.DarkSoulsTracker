using LiveSplit.TimeFormatters;
using System;

namespace LiveSplit.DarkSoulsTracker
{
    internal static class PercentageFormatter
    {
        public static string Format(double Percentage, TimeAccuracy timeAccuracy)
        {
            if (timeAccuracy == TimeAccuracy.Seconds)
            {
                return string.Format("{0}%", (Math.Truncate(Percentage)).ToString());
            }
            else if (timeAccuracy == TimeAccuracy.Tenths)
            {
                double tmp = Math.Truncate(Percentage * 10);
                return string.Format("{0}%", (tmp / 10).ToString("0.0"));
            }
            else
            {
                double tmp = Math.Truncate(Percentage * 100);
                return string.Format("{0}%", (tmp / 100).ToString("0.00"));
            }
        }
    }
}
