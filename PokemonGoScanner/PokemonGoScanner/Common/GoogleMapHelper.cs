namespace PokemonGoScanner.Common
{
    using System.Collections.Generic;
    using System.Linq;

    using Google.Common.Geometry;
    using Models;
    public class GoogleMapHelper
    {
        public static List<ulong> GetNearbyCellIds(UserSetting user)
        {
            var nearbyCellIds = new List<S2CellId>();
            var cellId = S2CellId.FromLatLng(S2LatLng.FromDegrees(user.Latitude, user.Longitude)).ParentForLevel(15);

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