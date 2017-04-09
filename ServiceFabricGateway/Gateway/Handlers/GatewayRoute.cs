using System;

namespace Gateway.Handlers
{
    class GatewayRoute
    {
        public string RouteKey { get; }

        public string FabricAddress { get; }

        public GatewayRoute(string routeKey, string fabricAddress)
        {
            RouteKey = routeKey;
            FabricAddress = fabricAddress;
        }

        public bool Matches(string matchRouteKey)
        {
            string toMatch = matchRouteKey.EndsWith("/")
                ? matchRouteKey.Remove(matchRouteKey.Length - 1, 1)
                : matchRouteKey;

            return string.Compare(toMatch, RouteKey, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}