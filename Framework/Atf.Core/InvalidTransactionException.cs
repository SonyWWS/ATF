//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Exception that is thrown when an invalid transaction occurs</summary>
    /// <remarks>
    /// Only throw this during a transaction, like if an ITransactionContext's InTransaction is true or if an
    /// IValidationContext has raised the Beginning event but hasn’t raised the Ended or Cancelled events.
    /// Consider throwing InvalidTransactionException in response to these events or in these methods:
    ///     DomNode.ChildInserting
    ///     DomNode.ChildRemoving
    ///     DomNode.AttributeChanging
    ///     Validator.OnEnding()
    ///     Validator.OnEnded()
    /// 
    /// It is generally unsafe to throw this exception in response to the following "-ed" events because
    /// you will be preventing the remaining listeners from receiving that event and if one of those
    /// remaining listeners is TransactionContext, then the DOM change can't be undone.
    ///     DomNode.ChildRemoved
    ///     DomNode.ChildInserted
    ///     DomNode.AttributeChanged
    /// </remarks>
    public class InvalidTransactionException : Exception
    {
        /// <summary>
        /// Constructor using message</summary>
        /// <param name="message">Message describing exception</param>
        public InvalidTransactionException(string message)
            : this(message, true, null) { }

        /// <summary>
        /// Constructor using message and inner exception</summary>
        /// <param name="message">Message describing exception</param>
        /// <param name="innerException">Inner exception or null</param>
        public InvalidTransactionException(string message, Exception innerException)
            : this(message, true, innerException) { }

        /// <summary>
        /// Constructor using message and error indicator</summary>
        /// <param name="message">Message describing exception</param>
        /// <param name="reportError">Value indicating whether or not this exception should be reported as an error</param>
        public InvalidTransactionException(string message, bool reportError)
            : this(message, reportError, null) { }

        /// <summary>
        /// Constructor using message, error indicator and inner exception</summary>
        /// <param name="message">Message describing exception</param>
        /// <param name="reportError">Value indicating whether or not this exception should be reported as an error</param>
        /// <param name="innerException">Inner exception, or null</param>
        public InvalidTransactionException(string message, bool reportError, Exception innerException)
            : base(message, innerException)
        {
            ReportError = reportError;
        }

        /// <summary>
        /// Value indicating whether or not this exception should be reported as an error. For example,
        /// if the user has cancelled a command, ReportError might be false. Is 'true' by default.</summary>
        public readonly bool ReportError;
    }
}
