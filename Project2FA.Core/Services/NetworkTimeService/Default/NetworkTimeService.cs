using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Project2FA.Core.Services.NTP
{
    //based on https://github.com/michaelschwarz/NETMF-Toolkit/blob/master/NTP/NtpClient.cs
    public class NetworkTimeService : INetworkTimeService
    {
        #region NTPServer
        /// <summary>
        /// Gets the current DateTime from time.windows.com.
        /// </summary>
        /// <returns>A DateTime containing the current time.</returns>
        public async Task<DateTime> GetNetworkTimeAsync()
        {
            return await GetNetworkTimeAsync("time.windows.com");
        }

        /// <summary>
        /// Gets the current DateTime from <paramref name="ntpServer"/>.
        /// </summary>
        /// <param name="ntpServer">The hostname of the NTP server.</param>
        /// <returns>A DateTime containing the current time.</returns>
        public async Task<DateTime> GetNetworkTimeAsync(string ntpServer)
        {
            IPAddress[] address = Dns.GetHostEntry(ntpServer).AddressList;

            if (address == null || address.Length == 0)
                throw new ArgumentException("Could not resolve ip address from '" + ntpServer + "'.", "ntpServer");

            IPEndPoint ep = new IPEndPoint(address[0], 123);

            return await GetNetworkTimeAsync(ep);
        }

        /// <summary>
        /// Gets the current DateTime form <paramref name="ep"/> IPEndPoint.
        /// </summary>
        /// <param name="ep">The IPEndPoint to connect to.</param>
        /// <returns>A DateTime containing the current time.</returns>
        public async Task<DateTime> GetNetworkTimeAsync(IPEndPoint ep)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            await s.ConnectAsync(ep);

            byte[] ntpData = new byte[48]; // RFC 2030 
            ntpData[0] = 0x1B;
            for (int i = 1; i < 48; i++)
                ntpData[i] = 0;

            //Stops code hang if NTP is blocked
            s.ReceiveTimeout = 2000;

            s.Send(ntpData);
            s.Receive(ntpData);

            byte offsetTransmitTime = 40;
            ulong intpart = 0;
            ulong fractpart = 0;

            for (int i = 0; i <= 3; i++)
                intpart = 256 * intpart + ntpData[offsetTransmitTime + i];

            for (int i = 4; i <= 7; i++)
                fractpart = 256 * fractpart + ntpData[offsetTransmitTime + i];

            ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);
            s.Close();

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }

        #endregion
    }
}
