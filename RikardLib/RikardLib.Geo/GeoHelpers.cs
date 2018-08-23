using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RikardLib.Geo
{
    public static class GeoHelpers
    {
        public static double DegreeBearing(GeoJsonPoint<GeoJson2DGeographicCoordinates> p1, GeoJsonPoint<GeoJson2DGeographicCoordinates> p2)
        {
            var dLon = ToRad(p2.Coordinates.Longitude - p1.Coordinates.Longitude);
            var dPhi = Math.Log(Math.Tan(ToRad(p2.Coordinates.Latitude) / 2.0d + Math.PI / 4.0d) 
                / Math.Tan(ToRad(p1.Coordinates.Latitude) / 2.0d + Math.PI / 4.0d));

            if (Math.Abs(dLon) > Math.PI)
            {
                dLon = dLon > 0 ? -(2.0d * Math.PI - dLon) : (2.0d * Math.PI + dLon);
            }

            return ToBearing(Math.Atan2(dLon, dPhi));

            double ToRad(double degrees)
            {
                return degrees * (Math.PI / 180.0d);
            }

            double ToDegrees(double radians)
            {
                return radians * 180.0d / Math.PI;
            }

            double ToBearing(double radians)
            {
                return (ToDegrees(radians) + 360.0d) % 360.0d;
            }
        }

        public static double CalcDistance(GeoJsonPoint<GeoJson2DGeographicCoordinates> p1, GeoJsonPoint<GeoJson2DGeographicCoordinates>  p2)
        {
            double R = 6371;

            double dLat = toRadian(p2.Coordinates.Latitude - p1.Coordinates.Latitude);
            double dLon = toRadian(p2.Coordinates.Longitude - p1.Coordinates.Longitude);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) 
                + Math.Cos(toRadian(p1.Coordinates.Latitude)) 
                * Math.Cos(toRadian(p2.Coordinates.Latitude)) 
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));

            return (R * c) * 1000.0d;

            double toRadian(double val)
            {
                return (Math.PI / 180) * val;
            }
        }

        public static GeoJsonPoint<GeoJson2DGeographicCoordinates> GetPolygonCentroid(GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates> poly)
        {
            double accumulatedArea = 0.0d;
            double centerX = 0.0d;
            double centerY = 0.0d;

            if (poly == null)
            {
                return null;
            }

            var coords = poly.Exterior.Positions.ToList();

            for (int i = 0, j = coords.Count - 1; i < coords.Count; j = i++)
            {
                double temp = coords[i].Longitude * coords[j].Latitude - coords[j].Longitude * coords[i].Latitude;
                accumulatedArea += temp;
                centerX += (coords[i].Longitude + coords[j].Longitude) * temp;
                centerY += (coords[i].Latitude + coords[j].Latitude) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
            {
                return null;
            }

            accumulatedArea *= 3f;

            return GeoJson.Point(GeoJson.Geographic(centerX / accumulatedArea, centerY / accumulatedArea));
        }
    }
}
