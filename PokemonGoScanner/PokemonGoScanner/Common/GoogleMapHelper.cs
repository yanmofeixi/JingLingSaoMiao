﻿namespace PokemonGoScanner.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AppModels;
    using Google.Common.Geometry;
    using POGOProtos.Map.Fort;
    public class GoogleMapHelper
    {
        public static string GetGMapLink(Location location, string text = "Google Map")
        {
            var sb = new StringBuilder();
            sb.Append($"<a href=\"{Constant.GoogleMapUrl}");
            sb.Append(location.Latitude);
            sb.Append(",");
            sb.Append(location.Longitude);
            sb.Append($"&z=17\">{text}</a>");
            return sb.ToString();
        }

        public static string GetGMapLink(FortData pokestop, string text = "this pokestop")
        {
            var sb = new StringBuilder();
            sb.Append($"<a href=\"{Constant.GoogleMapUrl}");
            sb.Append(pokestop.Latitude);
            sb.Append(",");
            sb.Append(pokestop.Longitude);
            sb.Append($"&z=17\">{text}</a>");
            return sb.ToString();
        }

        public static List<ulong> GetNearbyCellIds(Location location)
        {
            var nearbyCellIds = new List<S2CellId>();
            var cellId = S2CellId.FromLatLng(S2LatLng.FromDegrees(location.Latitude, location.Longitude)).ParentForLevel(15);

            nearbyCellIds.Add(cellId);
            for (var i = 0; i < Constant.ScanRange; i++)
            {
                nearbyCellIds.Add(GetPrevious(cellId, i));
                nearbyCellIds.Add(GetNext(cellId, i));
            }

            return nearbyCellIds.Select(c => c.Id).OrderBy(c => c).ToList();
        }

        private static S2CellId GetNext(S2CellId cellId, int depth)
        {
            while (true)
            {
                if (depth < 0)
                    return cellId;

                depth--;
                cellId = cellId.Next;
            }
        }

        private static S2CellId GetPrevious(S2CellId cellId, int depth)
        {
            while (true)
            {
                if (depth < 0)
                    return cellId;

                depth--;
                cellId = cellId.Previous;
            }
        }
    }
}