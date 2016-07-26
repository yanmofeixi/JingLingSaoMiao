﻿namespace PokemonGoScanner.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using Google.Common.Geometry;
    public class GoogleMapHelper
    {
        public static List<ulong> GetNearbyCellIds()
        {
            var nearbyCellIds = new List<S2CellId>();
            var cellId = S2CellId.FromLatLng(S2LatLng.FromDegrees(Constant.DefaultLatitude, Constant.DefaultLongitude)).ParentForLevel(Constant.ScanRange);

            nearbyCellIds.Add(cellId);
            for (var i = 0; i < 10; i++)
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