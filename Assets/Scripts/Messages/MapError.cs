using System.Collections.Generic;

namespace Messages
{
    public struct MapError
    {
        public enum ErrorType
        {
            WrongPosition,
            PathNotFound
        }

        public readonly ErrorType Error;

        public MapError(ErrorType error)
        {
            Error = error;
        }
    }

    internal static class MapErrorExtension
    {
        private static readonly IReadOnlyDictionary<MapError.ErrorType, string> ERROR_DESCRIPTION =
            new Dictionary<MapError.ErrorType, string>
            {
                { MapError.ErrorType.WrongPosition, "Wrong Position" },
                { MapError.ErrorType.PathNotFound, "Path Not Found" }
            };

        public static string ToText(this MapError.ErrorType errorType)
        {
            return ERROR_DESCRIPTION[errorType];
        }
    }
}