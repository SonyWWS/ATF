//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

namespace Sce.Atf
{
    /// <summary>
    /// Output writer service that routes messages to System.Console</summary>
    [Export(typeof(IOutputWriter))]
    [Export(typeof(ConsoleOutputWriter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ConsoleOutputWriter : IOutputWriter
    {
        /// <summary>
        /// Writes an output message of the given type</summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message</param>
        public void Write(OutputMessageType type, string message)
        {
            switch (type)
            {
                case OutputMessageType.Error:
                    Console.Error.Write("Error".Localize() + ": " + message);
                    break;
                case OutputMessageType.Warning:
                    Console.Error.Write("Warning".Localize() + ": " + message);
                    break;
                default:
                    Console.Write(message);
                    break;
            }
        }

        /// <summary>
        /// Clears the console output writer</summary>
        public void Clear()
        {
        }
    }
}

