namespace PokemonGoScanner.Common
{
    using System.Linq;
    using Enum;
    using Models;
    using POGOProtos.Networking.Requests;
    public static class NianticRequestSender
    {
        public static Request GetInitialRequest(UserSetting user, string token, params RequestType[] customRequestTypes)
        {
            var request = new Request
            {
                Altitude = Utility.FloatAsUlong(Constant.DefaultAltitude),
                Auth = new Request.Types.AuthInfo
                {
                    Provider = "google",
                    Token = new Request.Types.AuthInfo.Types.JWT
                    {
                        Contents = token,
                        Unknown13 = 14
                    }
                },
                Latitude = Utility.FloatAsUlong(user.Latitude),
                Longitude = Utility.FloatAsUlong(user.Longitude),
                RpcId = 1469378659230941192,
                Unknown1 = 2,
                Unknown12 = 989,
                Requests =
                {
                    customRequestTypes.ToList().Select(c => new Request.Types.Requests { Type = (int)c })
                }
            };
            return request;
        }

        public static Request GetRequest(UserSetting user, Request.Types.UnknownAuth unknownAuth, params Request.Types.Requests[] customRequests)
        {
            return new Request
            {
                Altitude = Utility.FloatAsUlong(Constant.DefaultAltitude),
                Unknownauth = unknownAuth,
                Latitude = Utility.FloatAsUlong(user.Latitude),
                Longitude = Utility.FloatAsUlong(user.Longitude),
                RpcId = 1469378659230941192,
                Unknown1 = 2,
                Unknown12 = 989,
                Requests =
                {
                    customRequests
                }
            };
        }

        public static Request GetRequest(UserSetting user, Request.Types.UnknownAuth unknownAuth,  params RequestType[] customRequestTypes)
        {
            var customRequests = customRequestTypes.ToList().Select(c => new Request.Types.Requests { Type = (int)c });
            return GetRequest(user, unknownAuth, customRequests.ToArray());
        }
    }
}
