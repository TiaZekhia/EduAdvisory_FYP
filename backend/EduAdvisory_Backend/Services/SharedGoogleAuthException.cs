namespace EduAdvisory_Backend.Services
{
    public class SharedGoogleAuthException : Exception
    {
        public bool ReconnectRequired { get; }

        public SharedGoogleAuthException(string message, bool reconnectRequired = false)
            : base(message)
        {
            ReconnectRequired = reconnectRequired;
        }
    }
}