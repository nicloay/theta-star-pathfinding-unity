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
}