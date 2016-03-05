using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class FormatHelpers
    {
        public static string FormatDateTimeOffset(this DateTimeOffset? date)
        {
            string datestring;

            if (date != null)
                datestring = date.Value.ToString("MM/dd/yyyy, hh:mm");
            else
                datestring = "No date provided";

            return datestring;
        }
    }
}