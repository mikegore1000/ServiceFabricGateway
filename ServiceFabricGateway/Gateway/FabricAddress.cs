using System;

namespace Gateway
{
    public class FabricAddress
    {
        public Uri Uri { get; }

        public FabricAddress(Uri uri)
        {
            if (uri.Segments.Length < 3)
            {
                throw new InvalidFabricAddressException();
            }

            var domain = uri.Segments[1].Replace("/", "").ToLowerInvariant();
            var service = uri.Segments[2].Replace("/", "").ToLowerInvariant();

            if (service.EndsWith("internal", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InternalServiceFabricAddressProvidedException();
            }

            this.Uri = new Uri($"fabric:/{domain}-{service}/{service}");
        }

        public override bool Equals(object obj)
        {
            return Uri.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        public class InvalidFabricAddressException : Exception
        {
        }

        public class InternalServiceFabricAddressProvidedException : InvalidFabricAddressException
        {
        }
    }
}
