//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for transaction contexts, which bracket changes to data between
    /// Begin/End calls and allow transactions to be cancelled. Throw
    /// InvalidTransactionException to cancel the transaction and rollback the
    /// changes made to the data.</summary>
    public interface ITransactionContext
    {
        /// <summary>
        /// Begins a transaction</summary>
        /// <param name="transactionName">Transaction name</param>
        void Begin(string transactionName);

        /// <summary>
        /// Gets whether the context is in a transaction; i.e., if Begin has
        /// been called but not End or Cancel</summary>
        bool InTransaction
        {
            get;
        }

        /// <summary>
        /// Cancels the transaction</summary>
        void Cancel();

        /// <summary>
        /// Ends the transaction and commits the results</summary>
        void End();
    }

    /// <summary>
    /// Useful static methods on transaction contexts</summary>
    public static class TransactionContexts
    {
        /// <summary>
        /// Performs the given action as a transaction with the given name</summary>
        /// <param name="context">Transaction context or null</param>
        /// <param name="transaction">Transaction action</param>
        /// <param name="transactionName">Transaction name</param>
        /// <returns><c>True</c> if the transaction succeeded and false if it was cancelled (i.e.,
        /// InvalidTransactionException was thrown)</returns>
        /// <remarks>In the implementation of 'transaction', throw InvalidTransactionException
        /// to cancel the transaction and log a warning message to the user (unless the
        /// InvalidTransactionException's ReportError is false).</remarks>
        public static bool DoTransaction(this ITransactionContext context, 
            Action transaction, string transactionName)
        {
            // If we are already in a transaction just perform the action
            // Let all exceptions "bubble up" and be handled by the outer transaction
            if (context != null && context.InTransaction)
            {
                transaction();
                return true;
            }
            
            try
            {
                if (context != null)
                    context.Begin(transactionName);

                //Transactions can be canceled in the call to Begin. When this occurs,
                //we want to skip doing the transaction and the end calls.
                if (context != null && !context.InTransaction)
                    return false;

                transaction();

                if (context != null)
                    context.End();
            }
            catch (InvalidTransactionException ex)
            {
                if (context != null && context.InTransaction)
                    context.Cancel();

                if (ex.ReportError)
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                return false;
            }
            return true;
        }
    }
}
