using NetTopologySuite.Geometries;

namespace Domain
{
    public static class GeoConstants
    {
        /// <summary>
        /// Şubeye check-in yapılabilmesi için kullanıcının bulunması gereken azami mesafe.
        /// </summary>
        public const int CheckInRadiusInMeters = 100;

        /// <summary>
        /// Yakındaki şubeleri listelerken kullanılan varsayılan keşif yarıçapı.
        /// </summary>
        public const int NearbyBranchesDefaultRadiusInMeters = 1000;

        /// <summary>
        /// SRID 4326 derece mesafesini metreye çevirmek için yaklaşık katsayı.
        /// </summary>
        public const double DegreesToMetersFactor = 111195;

        public static double DistanceInMeters(Point a, Point b)
            => a.Distance(b) * DegreesToMetersFactor;

        public static bool IsWithinCheckInRadius(Point branchLocation, Point userLocation)
            => DistanceInMeters(branchLocation, userLocation) <= CheckInRadiusInMeters;
    }
}
