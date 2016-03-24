using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class IdentityHelpers
    {
        public static string GetFirstName(this IIdentity user)
        {
            var ClaimUser = (ClaimsIdentity)user;
            var Claim = ClaimUser.Claims.FirstOrDefault(c => c.Type == "FirstName");
            if (Claim != null)
                return Claim.Value;
            else
                return null;
        }
    }
}