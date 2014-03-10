//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// An interface for using the Open Sound Control (OSC) protocol to communicate with OSC-compatible
    /// devices. This interface allows for sending and receiving name-value pairs, asynchronously.</summary>
    /// <remarks>See http://opensoundcontrol.org/spec-1_0 and http://opensoundcontrol.org/spec-1_1 </remarks>
    public interface IOscService
    {
        /// <summary>
        /// Sends the OSC addresses and data objects to each destination endpoint, asynchronously</summary>
        /// <param name="addressesAndData">Each pair is an OSC address string and the non-null data payload
        /// that will be sent to each destination device. Valid types of data are 32-bit floats, arrays of
        /// 32-bit floats, 32-bit integers, strings, and arrays of bytes.</param>
        /// <remarks>This method will return quickly. If there's a network slowdown of some kind, it may be
        /// possible that new values for a particular OSC address will replace the old unsent values. The
        /// order of the pairs can be changed before they are sent. The pairs may not be sent all in one
        /// OSC bundle, if the size is too large.</remarks>
        void Send(IEnumerable<Tuple<string, object>> addressesAndData);

        /// <summary>
        /// Notifies listeners that an OSC message has been received. The OSC address and data payload
        /// will be provided and listeners can set the Handled property to true if no further processing
        /// should be done on this message.</summary>
        event EventHandler<OscMessageReceivedArgs> MessageReceived;
    }

    /// <summary>
    /// Provides the OSC address and data payload for an OSC message. Setting the Handled property to
    /// true will prevent other listeners from receiving this object.</summary>
    public class OscMessageReceivedArgs : HandledEventArgs
    {
        /// <summary>
        /// Constructor, setting the Handled property to false.</summary>
        /// <param name="address">OSC address</param>
        /// <param name="data">Data payload</param>
        public OscMessageReceivedArgs(string address, object data)
            : base(false)
        {
            Address = address;
            Data = data;
        }

        /// <summary>
        /// Gets the OSC address</summary>
        public readonly string Address;

        /// <summary>
        /// Gets the data payload</summary>
        public readonly object Data;
    }

    /// <summary>
    /// Useful extension methods and utilities for working with IOscService objects</summary>
    public static class OscServices
    {
        /// <summary>
        /// Sends a single OSC message to each destination endpoint, asynchronously</summary>
        /// <param name="service"></param>
        /// <param name="oscAddress">The OSC address to which the data will be assigned to</param>
        /// <param name="data">The data object or "value" to set the OSC address to</param>
        public static void Send(this IOscService service, string oscAddress, object data)
        {
            service.Send(new[] { new Tuple<string, object>(oscAddress, data) });
        }

        /// <summary>
        /// Fixes an OSC address to make it compliant. Adds a leading '/' if it's missing and removes
        /// illegal characters</summary>
        /// <param name="oscAddress">e.g., "/root/category/Property Name With Spaces"</param>
        /// <returns>The corrected OSC address, e.g., "/root/category/PropertyNameWithSpaces"</returns>
        /// <remarks>Liine Lemur removes spaces, so let's follow that pattern with all illegal characters.</remarks>
        public static string FixPropertyAddress(string oscAddress)
        {
            if (!oscAddress.StartsWith("/"))
                oscAddress = '/' + oscAddress;

            int i = 0;
            while (true)
            {
                i = oscAddress.IndexOfAny(s_illegalOscAddressChars, i);
                if (i < 0)
                    break;

                oscAddress = oscAddress.Remove(i, 1);
            }

            return oscAddress;
        }

        private static readonly char[] s_illegalOscAddressChars = { ' ', '{', '}', '[', ']' };
    }
}
