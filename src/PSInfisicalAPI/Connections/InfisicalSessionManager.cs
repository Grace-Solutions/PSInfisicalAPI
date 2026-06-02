using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Connections
{
    public static class InfisicalSessionManager
    {
        private static readonly object Sync = new object();
        private static InfisicalConnection _current;

        public static InfisicalConnection Current
        {
            get { lock (Sync) { return _current; } }
        }

        public static void SetCurrent(InfisicalConnection connection)
        {
            lock (Sync)
            {
                _current = connection;
            }
        }

        public static InfisicalConnection RequireCurrent()
        {
            InfisicalConnection connection = Current;
            if (connection == null || !connection.IsConnected)
            {
                throw new InfisicalConfigurationException("No active Infisical connection. Call Connect-Infisical first.");
            }

            return connection;
        }

        public static void Disconnect()
        {
            lock (Sync)
            {
                if (_current != null)
                {
                    _current.AccessToken = null;
                    _current.IsConnected = false;
                    _current = null;
                }
            }
        }
    }
}
