namespace Project2FA.Uno.Core.Network
{
    public class NetworkAvailabilityChangedMessage
    {
        public NetworkAvailabilityChangedMessage(ConnectionTypes connectionType)
        {
            ConnectionType = connectionType;
        }

        public ConnectionTypes ConnectionType { get; set; }
    }
}
