using System;

namespace Gateway.Handlers
{
    internal static class UriExtensions
    {
        internal static string ToFabricAddress(this Uri uri)
        {
            if (uri.Segments.Length < 3)
            {
                return null;
            }

            var domain = uri.Segments[1].Replace("/", "").ToLowerInvariant();
            var service = uri.Segments[2].Replace("/", "").ToLowerInvariant();

            return $"fabric:/{domain}-{service}/{service}";
        }
    }
}
