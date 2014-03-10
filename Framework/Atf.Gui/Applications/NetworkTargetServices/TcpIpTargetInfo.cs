//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Class to describe TCP/IP target information</summary>
    [GroupAttribute("TcpIpTargetInfo", Header = @"TCP/IP Targets", ReadOnlyProperties = "Protocol")]
    public class TcpIpTargetInfo : TargetInfo, IPropertyValueValidator
    {
        public TcpIpTargetInfo()
        {
            Name = "LocalHost";
            Platform = "<undefined>";
            Endpoint = "127.0.0.1:12345"; // local host
            Protocol = ProtocolName;
            Scope = TargetScope.PerApp;
        }

        /// <summary>
        /// Gets or sets the fixed port value for the  TCP/IP target</summary>
        /// <remarks>If this value is set to a positive integer, then IPEndPoint will be constructed 
        /// using the IPAddress from  endpoint value(in string format) , combined with this fixed port number.
        /// The port number in the endpoint string value will be ignored even it includes one.</remarks>
        public int FixedPort { get; set; }

        public const string ProtocolName = @"Tcp";

        /// <summary>
        /// Gets or sets the endpoint</summary>
        public IPEndPoint IPEndPoint
        {
            // Gets the endpoint as an IPEndPoint object, return null for invalid address</summary>
            get
            {
                if (FixedPort > 0)
                    return TryParseEndPointUsingPort(Endpoint, FixedPort);
                return TryParseIPEndPoint(Endpoint);
            }
            set
            {
                // Sets the endpoint from an IPEndPoint object</summary>
                if (value == null) return;
                if (FixedPort > 0 && FixedPort != value.Port)
                    throw new InvalidDataException("The port number does not match expected fixed value " + FixedPort);
                    
                Endpoint = value.Address.ToString() + ":"+ value.Port;
            }
        }

        

        /// <summary>
        /// Convenience method to parse a string as a network endpoint; try to handle IPv4, IPv6, and host name notation</summary>
        /// <param name="endPoint">The string to parse</param>
        /// <returns>End point or null for invalid address</returns>
        /// <example>Example:
        ///  IPEndPoint ipv4Addr = TcpIpTargetInfo.TryParseIPEndPoint(@"192.168.0.1:12345");
        ///  IPEndPoint ipv6Addr = TcpIpTargetInfo.TryParseIPEndPoint(@"2001:740:8deb:0::1:12345");
        /// </example>
        public static IPEndPoint TryParseIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
              
            int port;
            if (!int.TryParse(ep[ep.Length - 1], out port))
                return null;
           
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                return null;

            IPAddress ip = TryParseIPAddress(endPoint);
            if (ip != null)
                return new IPEndPoint(ip, port);
            return null;
        }


        /// <summary>
        /// Convenience method to parse a string as a network endpoint; try to handle IPv4, IPv6, and host name notation</summary>
        /// <param name="endPoint">The string to parse, accept either a fully-qualified endpoint (IP address + port #) or just an IP address.</param>
        /// <param name="port">The port to combine with the IPAddress, ignore the port number in the endpoint even it includes one</param>
        /// <returns>End point or null for invalid address</returns>
        public static IPEndPoint TryParseEndPointUsingPort(string endPoint, int port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                return null;

            IPAddress ip = TryParseIPAddress(endPoint);
            if (ip!= null)
                return new IPEndPoint(ip, port);
            return null;
        }

        private static  IPAddress TryParseIPAddress(string ipAddress)
        {
            IPAddress ip;
            string[] ep = ipAddress.Split(':');
            if (ep.Length > 2) // must be an IPv6 address
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    return null;
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    // try as host name
                    try
                    {
                        ip = Dns.GetHostEntry(ep[0])
                            .AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork || x.AddressFamily == AddressFamily.InterNetworkV6);
                        if (ip == null)
                            return null;
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                }
            }
            return ip;
        }

        #region IPropertyValueValidator Members

        /// <summary>
        /// Validates target property</summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="formattedValue">Property value</param>
        /// <param name="errorMessage">Error message</param>
        /// <returns>True iff target property is valid</returns>
        public virtual bool Validate(string propertyName, object formattedValue, out string errorMessage)
        {
            bool result;
            errorMessage = string.Empty;

            if (propertyName.Equals("Endpoint"))
            {
                if (FixedPort > 0)
                    result = TryParseEndPointUsingPort(formattedValue.ToString(), FixedPort) != null;
                else
                    result = TryParseIPEndPoint(formattedValue.ToString()) != null;
                if (!result)
                    errorMessage = "The IP address format should be like \"192.168.0.1:12345\" or \"2001:740:8deb:0::1:12345\"".Localize();
                return result;
            }

            if (propertyName.Equals("Scope"))
            {
                result = Enum.IsDefined(typeof(TargetScope), formattedValue.ToString());
                if (!result)
                    errorMessage = "The scope type is unknown".Localize();
            }
            else if (propertyName.Equals("Name"))
            {
                result = !StringUtil.IsNullOrEmptyOrWhitespace(formattedValue.ToString());
                if (!result)
                    errorMessage = "The name must not be empty or all whitespace".Localize();
            }
            else
            {
                result = false;
                errorMessage = "The property name is unknown: ".Localize() + propertyName;
            }

            return result;
        }

        #endregion

    }

    /// <summary>
    /// Class to describe X86 target information</summary>
    [GroupAttribute("X86TargetInfo", Header = @"TCP/IP Targets", ReadOnlyProperties = "Protocol")]
    public class X86TargetInfo : TcpIpTargetInfo, IPropertyValueValidator
    {
        public const string PlatformName = @"x86";

        public X86TargetInfo()
            : base()
        {
            Name = "localhost";
            Platform = PlatformName;
            Endpoint = "127.0.0.1:1338";
        }
    }

    /// <summary>
    /// Class to describe PS3 target information</summary>
    [GroupAttribute("Ps3TargetInfo", Header = @"TCP/IP Targets", ReadOnlyProperties = "Protocol")]
    public class Ps3TargetInfo : TcpIpTargetInfo, IPropertyValueValidator
    {
        public const string PlatformName = @"Ps3";

        public Ps3TargetInfo()
            : base()
        {
            Name = "Ps3Host";
            Platform = PlatformName;
            Endpoint = "10.89.0.0:1338";
        }
    }

    /// <summary>
    /// Class to describe PS4 target information</summary>
    [GroupAttribute("Ps4TargetInfo", Header = @"TCP/IP Targets", ReadOnlyProperties = "Protocol")]
    public class Ps4TargetInfo : TcpIpTargetInfo, IPropertyValueValidator
    {
        public const string PlatformName = @"Ps4";

        public Ps4TargetInfo()
            : base()
        {
            Name = "Ps4Host";
            Platform = PlatformName;
            Endpoint = "10.89.0.0:1338";
        }
    }
}
