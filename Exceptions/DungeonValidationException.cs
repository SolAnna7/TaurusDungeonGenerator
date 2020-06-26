namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Exceptions
{
    public class DungeonValidationException : System.Exception
    {
        public DungeonValidationException(string message) : base(message)
        {
        }

        public DungeonValidationException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}