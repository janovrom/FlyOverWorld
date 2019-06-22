using System;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class GM
    {
        private const int EarthRadiusM = 6378137;
        private const double EquatorialLengthM = 40075016.686;
        private const double MetersPerPixelAt0 = 156543.03;
        private const float PI_F = (float)Math.PI;
        private const double OriginShift = 2 * Math.PI * EarthRadiusM / 2;
        private const double InitialResolution = 2 * Math.PI * EarthRadiusM / 256;

        /// <summary>
        /// The mapping between latitude, longitude and pixels is defined by the web
        /// mercator projection.
        /// </summary>
        /// <param name="latLon"></param>
        /// <param name="TileSize"></param>
        /// <returns></returns>
        public static Vector2 Project(Vector2 latLon, int TileSize)
        {
            double siny = Math.Sin(latLon.x * PI_F / 180);

            // Truncating to 0.9999 effectively limits latitude to 89.189. This is
            // about a third of a tile past the edge of the world tile.
            siny = Math.Min(Math.Max(siny, -0.9999), 0.9999);

            return new Vector2(
                (float) (TileSize * (0.5 + latLon.y / 360.0)),
                (float) (TileSize * (0.5 - Math.Log((1 + siny) / (1 - siny)) / (4 * Math.PI))));
        }

        /// <summary>
        /// Maps Mercator zoom level 0 coordinates to GPS latitude/longitude.
        /// </summary>
        /// <param name="pixel">Mercator coordinates at zoom level 0</param>
        /// <param name="TileSize">Size of one tile</param>
        /// <returns>GPS (Latitude, Longitude)</returns>
        public static Vector2 Unproject(Vector2 pixel, int TileSize)
        {
            float a = (-4 * Mathf.PI) * (pixel.y / (float)TileSize - 0.5f);
            float ea = Mathf.Exp(a);

            return new Vector2(
                Mathf.Asin((ea - 1) / (ea + 1)) * 180.0f / Mathf.PI,
                (pixel.x / (float)TileSize - 0.5f) * 360.0f
                );
        }

        public static float MetersPerPixel(float lat, int zoom)
        {
            return (float) ((MetersPerPixelAt0 * Math.Cos(Math.PI * lat / 180.0)) / (double)(1 << zoom));
        }

        /// <summary>
        /// Converts from gps to tile position.
        /// </summary>
        /// <param name="latLon"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static Vector2 WorldToTilePos(Vector2 latLon, int zoom)
        {
            Vector2 p = new Vector2();
            p.x = (latLon.y + 180.0f) / 360.0f * (1 << zoom);
            p.y = (float) ((1.0 - (float)Math.Log(Math.Tan(latLon.x * Math.PI / 180.0) +
                1.0 / (float) Math.Cos(latLon.x * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        /// <summary>
        /// Converts tile position to gps position.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static Vector2 TileToWorldPos(Vector2i tile, int zoom)
        {
            Vector2 latLon = new Vector2();
            double n = Math.PI - ((2.0 * Math.PI * tile.y) / Math.Pow(2.0, zoom));

            latLon.y = (float)((tile.x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            latLon.x = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return latLon;
        }
    }
}
