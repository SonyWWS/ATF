//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Application utilities to reduce duplicate client code and to implement common patterns of
    /// using the interfaces in Sce.Atf.Applications</summary>
    public static class ApplicationUtil
    {
        /// <summary>
        /// Inserts the specified child into the specified parent if possible and returns true
        /// iff successful</summary>
        /// <param name="context">Should be ITransactionContext to support undo/redo. Must be
        /// IHierarchicalInsertionContext and/or IInstancingContext to succeed.</param>
        /// <param name="parent">Optional. Parent object to which the new child is added. Can be
        /// null if the context supports it.</param>
        /// <param name="child">New child object to be inserted into the specified parent</param>
        /// <param name="operationName">Used to register the operation in the transaction context
        /// and to update the status if successful. Can be the empty string, but must not be null.</param>
        /// <param name="statusService">Optional. Status service that is updated if the operation
        /// was successful. Can be null.</param>
        /// <returns>True iff the insertion was successful</returns>
        /// <remarks>The context must implement IHierarchicalInsertionContext and/or IInstancingContext
        /// to allow insertion. If the context implements both, IHierarchicalInsertionContext is preferred and
        /// any insertion logic in the IInstancingContext implementation is ignored!</remarks>
        public static bool Insert(object context, object parent, object child, string operationName, IStatusService statusService)
        {
            ITransactionContext transactionContext = context.As<ITransactionContext>();

            if (CanInsert(context, parent, child))
            {
                if (transactionContext != null)
                {
                    // If we have a TransactionContext perform a transaction to make sure undo/redo is supported
                    transactionContext.DoTransaction(delegate
                        {
                            DoInsert(context, parent, child);
                        },
                        operationName);
                }
                else
                {
                    // If we don't have a transaction context just perform the insert
                    // and assume that our client does not support undo/redo functionality
                    DoInsert(context, parent, child);
                }

                // Update the status service if available.
                if (statusService != null)
                    statusService.ShowStatus(operationName);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Indicates if the specified child can be inserted into the specified parent object</summary>
        /// <param name="context">Must implement IHierarchicalInsertionContext and/or
        /// IInstancingContext to succeed</param>
        /// <param name="parent">Optional. Parent object we want to insert into. Can be null if the
        /// context supports it.</param>
        /// <param name="child">Child object to be inserted into the specified parent</param>
        /// <returns>True iff the child can be inserted into the parent (or the context supports
        /// parent-less insertion)</returns>
        /// <remarks>The context must implement IHierarchicalInsertionContext and/or IInstancingContext
        /// to allow insertion. If the context implements both, IHierarchicalInsertionContext is preferred and
        /// any insertion logic in the IInstancingContext implementation is ignored!</remarks>
        public static bool CanInsert(object context, object parent, object child)
        {
            IHierarchicalInsertionContext hierarchicalInsertionContext = context.As<IHierarchicalInsertionContext>();
            if (hierarchicalInsertionContext != null)
                return hierarchicalInsertionContext.CanInsert(parent, child);
            else
            {
                IInstancingContext instancingContext = context.As<IInstancingContext>();
                if (instancingContext != null)
                    return instancingContext.CanInsert(child);
            }

            return false;
        }

        /// <summary>
        /// Perform the actual insertion</summary>
        /// <param name="context">Must implement IHierarchicalInsertionContext and/or
        /// IInstancingContext to succeed</param>
        /// <param name="parent">Optional. Parent object to which the new child is added. Can be null
        /// if the context supports it.</param>
        /// <param name="child">New child object to be inserted into the specified parent</param>
        private static void DoInsert(object context, object parent, object child)
        {
            IHierarchicalInsertionContext hierarchicalInsertionContext = context.As<IHierarchicalInsertionContext>();
            if (hierarchicalInsertionContext != null)
                hierarchicalInsertionContext.Insert(parent, child);
            else
            {
                IInstancingContext instancingContext = context.As<IInstancingContext>();
                if (instancingContext != null)
                    instancingContext.Insert(child);
            }
        }

        /// <summary>  
        /// Inserts the specified item into 'parent', immediately after the 'before' object.</summary>  
        /// <param name="context">Should be adaptable to ITransactionContext to support undo/redo.  
        /// Must be adaptable to IOrderedInsertionContext to succeed.</param>  
        /// <param name="parent">The parent object of 'before' and the object that will become the parent of 'item'.  
        /// Can be null if the objects are in a flat list or if there are multiple roots and the objects are to be  
        /// inserted into the top level.</param>  
        /// <param name="before">The object that will be the sibling to 'item' and be before 'item'.</param>  
        /// <param name="item">The object to be inserted.</param>  
        /// <param name="operationName">Used to register the operation in the transaction context  
        /// and to update the status if successful. Can be the empty string, but must not be null.</param>  
        /// <param name="statusService">Optional. Status service that is updated if the operation  
        /// was successful. Can be null.</param>  
        /// <returns>True iff the insertion was successful</returns>  
        public static bool InsertBetween(object context, object parent, object before, object item, string operationName, IStatusService statusService)
        {
            ITransactionContext transactionContext = context.As<ITransactionContext>();

            if (CanInsertBetween(context, parent, before, item))
            {
                if (transactionContext != null)
                {
                    // If we have a TransactionContext perform a transaction to make sure undo/redo is supported  
                    transactionContext.DoTransaction(delegate
                    {
                        DoInsert(context, parent, before, item);
                    },
                        operationName);
                }
                else
                {
                    // If we don't have a transaction context just perform the insert  
                    // and assume that our client does not support undo/redo functionality  
                    DoInsert(context, parent, before, item);
                }

                // Update the status service if available.  
                if (statusService != null)
                    statusService.ShowStatus(operationName);

                return true;
            }

            return false;
        }

        /// <summary>  
        /// Tests whether or not the specified item can be inserted under 'parent' and after 'before'.</summary>  
        /// <param name="context">Should be adaptable to ITransactionContext to support undo/redo.  
        /// Must be adaptable to IOrderedInsertionContext to succeed.</param>  
        /// <param name="parent">The parent object of 'before' and the object that will become the parent of 'item'.  
        /// Can be null if the objects are in a flat list or if there are multiple roots and the objects are to be  
        /// inserted into the top level.</param>  
        /// <param name="before">The object that will be the sibling to 'item' and be before 'item'.</param>  
        /// <param name="item">The object to be inserted.</param>  
        /// <returns>True if 'item' can be inserted and false otherwise</returns>  
        public static bool CanInsertBetween(object context, object parent, object before, object item)
        {
            var orderedInsertionContext = context.As<IOrderedInsertionContext>();
            if (orderedInsertionContext != null)
                return orderedInsertionContext.CanInsert(parent, before, item);

            return false;
        }

        private static void DoInsert(object context, object parent, object before, object item)
        {
            var orderedInsertionContext = context.Cast<IOrderedInsertionContext>();
            orderedInsertionContext.Insert(parent, before, item);
        }


    }
}
