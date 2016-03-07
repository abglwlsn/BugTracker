using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class FormatHelpers
    {
        public static string FormatDateTimeOffsetCondensed(this DateTimeOffset? date)
        {
            string datestring;

            if (date != null)
                datestring = date.Value.ToString("MM/dd/yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }

        public static string FormatDateTimeOffsetCondensed(this DateTimeOffset date)
        {
            string datestring;

            if (date != null)
                datestring = date.DateTime.ToString("MM/dd/yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }

        public static string FormatDateTimeOffset(this DateTimeOffset? date)
        {
            string datestring;

            if (date != null)
                datestring = date.Value.ToString("ddd, MMMM dd, yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }
    }
}