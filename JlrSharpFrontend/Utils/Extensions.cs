using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JlrSharpFrontend.Utils
{
    public static class Extensions
    {
        public static bool TryGetParamAsString(this IQueryCollection query, string key, out string value)
        {
            value = null;
            if (!query.ContainsKey(key)) return false;
            value = query[key];
            return true;
        }
    }
}
