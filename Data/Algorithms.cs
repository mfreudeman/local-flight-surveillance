using System;

namespace LFS.Data
{
    public static class Algorithms
    {
        /// <summary>
        /// Calculate the final cone of a runway.
        /// </summary>
        /// <param name="thresholdCenter">Center of the landing end runway threshold.</param>
        /// <param name="runwayTrueHeading">True heading of the runway.</param>
        /// <param name="finalDistance">Length of final cone in nautical miles.</param>
        /// <param name="thresholdWidth">Total width of cone at runway threshold in feet.</param>
        /// <param name="coneAngle">Total width of cone at runway threshold in feet.</param>
        /// <returns>Final cone polygon.</returns>
        public static GeoPoint[] MakeFinalPolygon(GeoPoint thresholdCenter, double runwayTrueHeading, double finalDistance, double thresholdWidth = 200.0, double coneAngle = 3.0)
        {
            const double halfPiRadian = Math.PI / 2;
            const double DEG2RAD = Math.PI / 180.0;
            double coneAngleRadian = coneAngle * DEG2RAD;
            double headingAsRadian = ((450.0 - (runwayTrueHeading + 180.0) % 360.0) % 360.0) * DEG2RAD;
            double leftThresholdRadian = headingAsRadian - halfPiRadian;
            double rightThresholdRadian = headingAsRadian + halfPiRadian;
            double leftSideAngleRadian = headingAsRadian - coneAngleRadian;
            double rightSideAngleRadian = headingAsRadian + coneAngleRadian;
            double thresholdArmDegreesLength = (1.0 / 6076.115 / 60.0) * (thresholdWidth / 2.0);
            double finalArmDegreesLength = finalDistance / 60.0;
            GeoPoint thresholdRightPoint = new GeoPoint(
                (Math.Sin(rightThresholdRadian) * thresholdArmDegreesLength) + thresholdCenter.Latitude, 
                (Math.Cos(rightThresholdRadian) * thresholdArmDegreesLength) + thresholdCenter.Longitude
            );
            GeoPoint thresholdLeftPoint = new GeoPoint(
                (Math.Sin(leftThresholdRadian) * thresholdArmDegreesLength) + thresholdCenter.Latitude,
                (Math.Cos(leftThresholdRadian) * thresholdArmDegreesLength) + thresholdCenter.Longitude
            );
            return new GeoPoint[5] {
                thresholdRightPoint, 
                thresholdLeftPoint,
                new GeoPoint(
                    (Math.Sin(leftSideAngleRadian) * finalArmDegreesLength) + thresholdLeftPoint.Latitude,
                    (Math.Cos(leftSideAngleRadian) * finalArmDegreesLength) + thresholdLeftPoint.Longitude
                ),
                new GeoPoint(
                    (Math.Sin(headingAsRadian) * finalArmDegreesLength) + thresholdCenter.Latitude,
                    (Math.Cos(headingAsRadian) * finalArmDegreesLength) + thresholdCenter.Longitude
                ),
                new GeoPoint(
                    (Math.Sin(rightSideAngleRadian) * finalArmDegreesLength) + thresholdRightPoint.Latitude,
                    (Math.Cos(rightSideAngleRadian) * finalArmDegreesLength) + thresholdRightPoint.Longitude
                )
            };
        }

        public static GeoPoint[] MakeRunwayPolygon(GeoPoint thresholdCenter, double runwayTrueHeading, double finalDistance, double thresholdWidth = 200.0)
        {
            return new GeoPoint[4];
        }

        /// <summary>
        /// Calculate the distance between two points on a sphere.
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="Radius">Radius of sphere</param>
        /// <returns></returns>
        public static double Haversine(GeoPoint p1, GeoPoint p2, double Radius = 3440.0648)
        {
            const double TORAD = Math.PI / 180.0;
            double lat = (p2.Latitude - p1.Latitude) * TORAD;
            double lon = (p2.Longitude - p1.Longitude) * TORAD;
            double h = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(lat / 2), 2) + Math.Cos(p1.Latitude * TORAD) * Math.Cos(p2.Latitude * TORAD) * Math.Pow(Math.Sin(lon / 2), 2)));
            return h * Radius;
        }

        /// <summary>
        /// Determine if a point is within a polygon.
        /// </summary>
        /// <param name="point">Point to test.</param>
        /// <param name="polygon">Polygon to use.</param>
        /// <returns>True if the point is within the polygon, otherwise false.</returns>
        public static bool PointInPolygon(GeoPoint point, GeoPoint[] polygon)
        {
            int counter = 0;
            GeoPoint p1, p2;

            p1 = polygon[0];
            for (int i = 1; i < polygon.Length; i++)
            {
                p2 = polygon[i % polygon.Length];
                if (point.Longitude > Math.Min(p1.Longitude, p2.Longitude))
                {
                    if (point.Longitude <= Math.Max(p1.Longitude, p2.Longitude))
                    {
                        if (point.Latitude <= Math.Max(p1.Latitude, p2.Latitude))
                        {
                            if (p1.Longitude != p2.Longitude)
                            {
                                double xinters = (point.Longitude - p1.Longitude) * (p2.Latitude - p1.Latitude) / (p2.Longitude - p1.Longitude) + p1.Latitude;
                                if (p1.Latitude == p2.Latitude || point.Latitude <= xinters)
                                {
                                    counter++;
                                }
                            }
                        }
                    }
                }
                p1 = p2;
            }

            if (counter % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
