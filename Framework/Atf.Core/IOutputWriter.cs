//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for components that write output messages, e.g. messages to System.Console</summary>
    public interface IOutputWriter
    {
        /// <summary>
        /// Writes an output message of the given type</summary>
        /// <param name="type">Message type (Error, Warning or Info)</param>
        /// <param name="message">Message</param>
        void Write(OutputMessageType type, string message);

        /// <summary>
        /// Clears the writer</summary>
        void Clear();
    }

    /// <summary>
    /// Contains extension methods for IOutputWriter</summary>
    public static class OutputWriters
    {
        /// <summary>
        /// Formats and writes an output message of the given type</summary>
        /// <param name="writer">Output writer</param>
        /// <param name="type">Message type (Error, Warning or Info)</param>
        /// <param name="formatString">Message format string</param>
        /// <param name="args">Message arguments</param>
        /// <remarks>Use writer formatting when possible to help writers correctly classify
        /// output messages. For example, a writer that presents a dialog to the user can
        /// suppress messages of a given class, even though they may differ in specifics such
        /// as file name, exception message, etc.</remarks>
        public static void Write(this IOutputWriter writer, OutputMessageType type, string formatString, params object[] args)
        {
            writer.Write(type, string.Format(formatString, args));
        }
    }
}

