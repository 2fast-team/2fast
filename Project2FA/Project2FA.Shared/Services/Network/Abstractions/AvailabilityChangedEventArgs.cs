using System;

namespace Project2FA.Uno.Core.Network
{
    public class AvailabilityChangedEventArgs : EventArgs
    {
        public ConnectionTypes ConnectionType { get; set; }
    }
}
