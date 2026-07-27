using System.Diagnostics;

namespace TrocoPoints.Infrastructure.Messaging
{
    public class TrocoPointsActivitySource
    {
        public static readonly ActivitySource Instance = new("TrocoPoints.Messaging");
    }
}
