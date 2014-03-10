//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Sce.Atf
{
    /// <summary>
    /// Provides static methods for easy access to the output message facilities in the application.
    /// Automatically finds objects that implement IOutputWriter, if both this class type and
    /// an implementer of IOutputWriter (such as OutputService) are added to a MEF catalog.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(Outputs))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Outputs : IInitializable, IPartImportsSatisfiedNotification
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // implement IInitializable to bring component into existence
        }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            // collect output writers and set up static array
            List<IOutputWriter> writers = new List<IOutputWriter>(m_outputWriters.GetValues());
            if (writers.Count > 0)
                s_outputWriters = writers.ToArray();
            else
                s_outputWriters = EmptyArray<IOutputWriter>.Instance;
        }

        #endregion

        /// <summary>
        /// Gets the ATF outputs tracer (a System.Diagnostics tool)</summary>
        /// <remarks>The default is not to trace. Set the switch level to turn on tracing,
        /// for example, TraceSource.Switch.Level = SourceLevels.Error</remarks>
        public static TraceSource TraceSource
        {
            get { return s_atfOutputTracer; }
        }

        /// <summary>
        /// Writes an output message of the given type using all available IOutputWriters</summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message</param>
        public static void Write(OutputMessageType type, string message)
        {
            Write(type, 0, message);

            if (type == OutputMessageType.Error)
                ErrorCount++;
            else if (type == OutputMessageType.Warning)
                WarningCount++;
        }

        /// <summary>
        /// Writes an output message of the given type and id, using all available IOutputWriters</summary>
        /// <param name="type">Message type</param>
        /// <param name="id">A numeric identifier for the message; is only used with TraceSource</param>
        /// <param name="message">Message</param>
        public static void Write(OutputMessageType type, int id, string message)
        {
            switch (type)
            {
                case OutputMessageType.Error:
                    s_atfOutputTracer.TraceEvent(TraceEventType.Error, id, message);
                    break;
                case OutputMessageType.Warning:
                    s_atfOutputTracer.TraceEvent(TraceEventType.Warning, id, message);
                    break;
                case OutputMessageType.Info:
                    s_atfOutputTracer.TraceEvent(TraceEventType.Information, id, message);
                    break;
            }

            foreach (IOutputWriter writer in s_outputWriters)
                writer.Write(type, message);
        }

        /// <summary>
        /// Formats and writes an output message of the given type using all available
        /// IOutputWriters</summary>
        /// <param name="type">Message type</param>
        /// <param name="formatString">Message format string</param>
        /// <param name="args">Message arguments</param>
        /// <remarks>Use writer formatting, when possible, to help writers correctly classify
        /// output messages. For example, a writer that presents a dialog to the user can
        /// suppress messages of a given class, even though they may differ in specifics such
        /// as file name, exception message, etc.</remarks>
        public static void Write(OutputMessageType type, string formatString, params object[] args)
        {
            string message = string.Format(formatString, args);
            Write(type, message);
        }

        /// <summary>
        /// Writes a message of the given type, ending in a new line character sequence (if necessary), using all
        /// available IOutputWriters</summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message</param>
        public static void WriteLine(OutputMessageType type, string message)
        {
            // don't add redundant newline character unless needed
            if (!message.EndsWith(Environment.NewLine))
                message += Environment.NewLine;

            Write(type, message);
        }

        /// <summary>
        /// Formats and writes a message of the given type, ending in a new line character sequence (if necessary), 
        /// using all available IOutputWriters</summary>
        /// <param name="type">Message type</param>
        /// <param name="formatString">Message format string</param>
        /// <param name="args">Message arguments</param>
        /// <remarks>Use writer formatting, when possible, to help writers correctly classify
        /// output messages. For example, a writer that presents a dialog to the user can
        /// suppress messages of a given class, even though they may differ in specifics such
        /// as file name, exception message, etc.</remarks>
        public static void WriteLine(OutputMessageType type, string formatString, params object[] args)
        {
            string message = string.Format(formatString, args);
            WriteLine(type, message);
        }

        /// <summary>
        /// Clears all available output writers</summary>
        public static void Clear()
        {
            foreach (IOutputWriter writer in s_outputWriters)
                writer.Clear();
            ResetCounters();
        }

        /// <summary>
        /// Set error/warning count back to zero</summary>
        public static void ResetCounters()
        {
            ErrorCount = 0;
            WarningCount = 0;
        }

        /// <summary>
        /// Gets the count of the number of error messages that have been output since the
        /// last time Clear() or ResetCounters() has been called.</summary>
        public static uint ErrorCount { get; private set; }

        /// <summary>
        /// Gets the count of the number of warning messages that have been output since the
        /// last time Clear() or ResetCounters() has been called.</summary>
        public static uint WarningCount { get; private set; }

        /// <summary>
        /// Configures the application to use the given output writers</summary>
        /// <param name="outputWriters">Output writers</param>
        public static void Configure(params IOutputWriter[] outputWriters)
        {
            s_outputWriters = outputWriters;
        }

        private static IOutputWriter[] s_outputWriters = EmptyArray<IOutputWriter>.Instance;

        // Note: the default for a TraceSource is to *not* trace. You need to set the switch level to trace different types of messages, for example: 
        // TraceSource.Switch.Level = SourceLevels.Error;
        private static TraceSource s_atfOutputTracer = new TraceSource("Sce.Atf.OutputsTracer");// create the TraceSource for Atf outputs.     

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<Lazy<IOutputWriter>> m_outputWriters;
    }
}

