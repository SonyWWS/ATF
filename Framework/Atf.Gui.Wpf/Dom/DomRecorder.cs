//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Dom
{
    /// <summary>
    /// Component to implement a DOM recorder, which records DOM events on the active context and
    /// displays them in a list. These logged events can also be retrieved programmatically.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(DomRecorder))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class DomRecorder : Sce.Atf.Wpf.Applications.IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public DomRecorder(Sce.Atf.Wpf.Applications.IControlHostService controlHostService)
        {
            m_data = new DataContainer();
            m_controlHostService = controlHostService;

            m_uiDispatcher = Dispatcher.CurrentDispatcher;

            DomNode.DiagnosticAttributeChanged += m_root_AttributeChanged;
            DomNode.DiagnosticChildInserted += m_root_ChildInserted;
            DomNode.DiagnosticChildRemoved += m_root_ChildRemoved;
        }

        /// <summary>
        /// Gets the most recent 'maxEvents' log events, from oldest to newest</summary>
        /// <param name="maxEvents">The maximum number of log events to get. -1 means "get all".</param>
        /// <returns>At most, 'maxEvents' recent log events. Each log event is designed to be human readable.</returns>
        /// <remarks>There are five different kinds of log events:
        /// Transaction begin, Transaction end, DOM child add, DOM child remove, and DOM attribute change.</remarks>
        public IEnumerable<string> GetLogEvents(int maxEvents)
        {
            int startIndex;
            if (maxEvents == -1)
                startIndex = 0;
            else
                startIndex = Math.Max(0, m_data.Count - maxEvents);

            for (int i = startIndex; i < m_data.Count; i++)
            {
                DataItem item = m_data[i];
                yield return item.ReportLine;
            }
        }

        /// <summary>
        /// Gets the number of log events currently in this DomRecorder</summary>
        public int NumLogEvents
        {
            get { return m_data.Count; }
        }

        /// <summary>
        /// Clears all the log data and refreshes the Control</summary>
        public void Clear()
        {
            // Raises the IObservableContext.Reloaded event which rebuilds the TreeListEditor and
            //  TreeListViewAdapter.
            m_data.Clear();
        }

        /// <summary>
        /// Gets or sets the document optional registry. If present, when a document is closed,
        /// all of the DOM event data will be cleared, so as to prevent memory leaks.</summary>
        [Import(AllowDefault = true)]
        public IDocumentRegistry DocumentRegistry
        {
            get { return m_documentRegistry; }
            set
            {
                if (m_documentRegistry != null)
                    m_documentRegistry.DocumentRemoved -= m_documentRegistry_DocumentRemoved;

                m_documentRegistry = value;

                if (m_documentRegistry != null)
                    m_documentRegistry.DocumentRemoved += m_documentRegistry_DocumentRemoved;
            }
        }

        /// <summary>
        /// Gets or sets the context registry to be used to automatically find the root DomNode and
        /// the validation context. Is optional and can be null.</summary>
        [Import(AllowDefault = true)]
        public IContextRegistry ContextRegistry
        {
            get { return m_contextRegistry; }
            set
            {
                if (m_contextRegistry != null)
                    m_contextRegistry.ActiveContextChanged -= m_contextRegistry_ActiveContextChanged;

                m_contextRegistry = value;

                if (m_contextRegistry != null)
                    m_contextRegistry.ActiveContextChanged += m_contextRegistry_ActiveContextChanged;
            }
        }

        /// <summary>
        /// Gets or sets the validation context that is used to record when transactions begin
        /// and end. Is optional and can be null. Is set automatically if the context registry
        /// raises the ActiveContextChanged event and the new context can be adapted to
        /// IValidationContext.</summary>
        public IValidationContext ValidationContext
        {
            get { return m_validationContext; }
            set
            {
                if (m_validationContext != null)
                {
                    m_validationContext.Beginning -= m_validationContext_Beginning;
                    m_validationContext.Cancelled -= m_validationContext_Cancelled;
                    m_validationContext.Ended -= m_validationContext_Ended;
                }

                m_validationContext = value;

                if (m_validationContext != null)
                {
                    m_validationContext.Beginning += m_validationContext_Beginning;
                    m_validationContext.Cancelled += m_validationContext_Cancelled;
                    m_validationContext.Ended += m_validationContext_Ended;
                }
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Finish initializing component by registering control</summary>
        public virtual void Initialize()
        {
            var view = new DomRecorderView { DataContext = m_data };
            
            m_controlHostService.RegisterControl(
                view,
                "DomRecorder".Localize(),
                "Records DOM events on the active context and displays them".Localize(),
                StandardControlGroup.Bottom,
                "{5693ACB5-C323-42FA-914D-7989BD327D18}",
                this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Activates the client control</summary>
        /// <param name="control">Client control to be activated</param>
        void Sce.Atf.Wpf.Applications.IControlHostClient.Activate(object control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void Sce.Atf.Wpf.Applications.IControlHostClient.Deactivate(object control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <param name="mainWindowClosing"></param>
        /// <returns><c>True</c> if the control can close, or false to cancel</returns>
        bool Sce.Atf.Wpf.Applications.IControlHostClient.Close(object control, bool mainWindowClosing)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Performs custom actions on IValidationContext.Beginning events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void m_validationContext_Beginning(object sender, EventArgs e)
        {
            m_uiDispatcher.InvokeIfRequired(() => m_data.AddTransactionBegin(GetCurrentTransactionName()));
        }

        /// <summary>
        /// Performs custom actions on IValidationContext.Cancelled events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void m_validationContext_Cancelled(object sender, EventArgs e)
        {
            m_uiDispatcher.InvokeIfRequired(() => m_data.AddTransactionCancel(GetCurrentTransactionName()));
        }

        /// <summary>
        /// Performs custom actions on IValidationContext.Ended events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void m_validationContext_Ended(object sender, EventArgs e)
        {
            m_uiDispatcher.InvokeIfRequired(() => m_data.AddTransactionEnd(GetCurrentTransactionName()));
        }

        /// <summary>
        /// Performs custom actions on DomNode.ChildRemoved events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void m_root_ChildRemoved(object sender, ChildEventArgs e)
        {
            string analysis = m_analysisEnabled ? AnalyzeRemoved(e) : string.Empty;
            m_uiDispatcher.InvokeIfRequired(() => m_data.AddDomEvent(DataType.ChildRemoved, e, analysis));
        }

        /// <summary>
        /// Performs custom actions on DomNode.ChildInserted events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void m_root_ChildInserted(object sender, ChildEventArgs e)
        {
            string analysis = m_analysisEnabled ? AnalyzeInserted(e) : string.Empty;
            m_uiDispatcher.InvokeIfRequired(() => m_data.AddDomEvent(DataType.ChildAdded, e, analysis));
        }

        /// <summary>
        /// Performs custom actions on DomNode.AttributeChanged events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void m_root_AttributeChanged(object sender, AttributeEventArgs e)
        {
            string analysis = m_analysisEnabled ? AnalyzeAttributeChanged(e) : string.Empty;
            m_uiDispatcher.InvokeIfRequired(() => m_data.AddDomEvent(DataType.AttributeChanged, e, analysis));
        }

        private string AnalyzeRemoved(ChildEventArgs e)
        {
            return AnalyzeListeners(e.Parent.GetChildRemovedHandlers());
        }

        private string AnalyzeInserted(ChildEventArgs e)
        {
            return AnalyzeListeners(e.Parent.GetChildInsertedHandlers());
        }

        private string AnalyzeAttributeChanged(AttributeEventArgs e)
        {
            return AnalyzeListeners(e.DomNode.GetAttributeChangedHandlers());
        }

        // Report the following information and possible problems on a list of event handlers for a
        //  DOM change that has just taken place:
        //  1. How many event listeners are there? If this number drops down, it could indicate that changes
        //  are not being observed that should be. If this number is higher than expected, perhaps unwanted
        //  listeners are hurting performance.
        //  2. Are there any IHistoryContexts listening? If not, then the change is not being recorded for
        //  undo/redo.
        //  3. Are one or more HistoryContexts recording? (This is the concrete ATF class.) If the recording
        //  is not enabled, then the change will not be automatically added to an undo/redo command. This
        //  isn't necessarily an error. For example, the change could be temporarily, like when dragging a
        //  circuit element on a canvas. Or perhaps client code will manually add a Command to the
        //  CommandHistory.
        //  4. Is the current DOM change being made by a listener in response to an earlier DOM change
        //  event? If so, are any IHistoryContexts listening *after* the listener making the change? If so,
        //  this could be a serious error, since the history context will receive the changes out-of-order.
        //  Unfortunately, this analysis is very difficult if not impossible!
        //  http://stackoverflow.com/questions/889310/how-do-i-get-the-executing-object-for-a-stackframe/10530629
        //  5. Are there any duplicate listeners?
        private string AnalyzeListeners<T>(IEnumerable<EventHandler<T>> eventHandlers)
            where T : EventArgs
        {
            int count = 0;
            int numHistoryContexts = 0;
            int numRecorded = 0;
            //MethodInfo nonHistoryContextListeningEarly = null; //to-do: perhaps it's still worthwhile to check the call stack
            //MethodInfo nonHistoryContextListener = null;
            MethodInfo duplicateListener = null;
            var listeners = new Dictionary<MethodInfo, List<object>>(); //method and owning objects
            foreach (var eventHandler in eventHandlers)
            {
                count++;

                // We have to do this explicit way of checking if the owner and the eventHandler.Target
                //  are the same because DomNodeAdapters override GetHashCode() and AreEqual() to say
                //  they are equal if they both adapt the same DomNode.
                List<object> owners;
                if (listeners.TryGetValue(eventHandler.Method, out owners))
                {
                    if (owners.Find(o => o == eventHandler.Target) != null)
                        duplicateListener = eventHandler.Method;
                    else
                        owners.Add(eventHandler.Target);
                }
                else
                    listeners.Add(eventHandler.Method, new List<object>(new[] {eventHandler.Target}));

                if (eventHandler.Target is IHistoryContext)
                {
                    numHistoryContexts++;
                    //if (nonHistoryContextListener != null)
                    //    nonHistoryContextListeningEarly = nonHistoryContextListener;
                    var historyContext = eventHandler.Target as HistoryContext;
                    if (historyContext != null && historyContext.Recording)
                        numRecorded++;
                }
                //else
                //    nonHistoryContextListener = eventHandler.Method;
            }

            // build the report, as a single line of text
            var sb = new StringBuilder();
            sb.AppendFormat("# of listeners: {0}", count);
            if (duplicateListener != null)
                sb.AppendFormat(". DUPLICATE LISTENER: {0} on {1}", duplicateListener.Name, duplicateListener.DeclaringType);
            if (numHistoryContexts > 0)
            {
                sb.AppendFormat(". # of IHistoryContext: {0}", numHistoryContexts);
                sb.AppendFormat(". # recording: {0}", numRecorded);
                //if (nonHistoryContextListeningEarly != null)
                //{
                //    sb.AppendFormat(". Listener received event before a IHistoryContext: {0} on {1}",
                //        nonHistoryContextListeningEarly.Name, nonHistoryContextListeningEarly.DeclaringType);
                //}
            }
            return sb.ToString();
        }

        private void m_contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            ValidationContext = m_contextRegistry.ActiveContext.As<IValidationContext>();
        }

        private void m_documentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
        {
            // to avoid memory leaks, clear all DOM data when a document closes
            Clear();
        }

        //private void TreeListViewAdapter_RetrieveVirtualItem(object sender, TreeListViewAdapter.RetrieveVirtualNodeAdapter e)
        //{
        //    // present the list with the most recent change at the top
        //    //e.Item = Data[Data.Count - e.ItemIndex - 1]; //error! 

        //    e.Item = m_data[e.ItemIndex]; //this works, but is not the sorting I want
        //}

        private IEnumerable<DataItem> GetItemsForReport()
        {
            yield break;
            
            //bool foundSelection = false;
            //foreach (DataItem item in Selection)
            //{
            //    foundSelection = true;
            //    yield return item;
            //}
            
            //if (!foundSelection)
            //{
            //    foreach (DataItem item in m_data.Roots)
            //        yield return item;
            //}
        }

        private void CopyBtnClick(object sender, EventArgs e)
        {
            var report = new StringBuilder();

            foreach (DataItem item in GetItemsForReport())
                report.AppendLine(item.ReportLine);

            // Clipboard.SetText will throw an exception if the report is the empty string.
            string reportString = report.ToString();
            if (!string.IsNullOrEmpty(reportString))
                Clipboard.SetText(reportString);
        }

        private void ClearBtnClick(object sender, EventArgs e)
        {
            Clear();
        }

        private void AnalysisClick(object sender, EventArgs e)
        {
            //m_analysisEnabled = ((CheckBox)sender).Checked;
        }

        private string GetCurrentTransactionName()
        {
            if (m_validationContext == null)
                return string.Empty;

            var transactionContext = m_validationContext.As<TransactionContext>();
            if (transactionContext == null)
                return string.Empty;

            return transactionContext.TransactionName;
        }

        private enum DataType
        {
            Begin,
            Cancel,
            End,
            ChildAdded,
            ChildRemoved,
            AttributeChanged
        }

        private class DataItem
        {
            public DataItem(int transactionNum, DataType transactionType, string transactionName)
            {
                TransactionNum = transactionNum;
                m_transactionType = transactionType;
                m_transactionName = transactionName;
                m_analysis = string.Empty;
            }

            public DataItem(int transactionNum, DataType transactionType, EventArgs domArgs, string analysis)
            {
                TransactionNum = transactionNum;
                m_transactionType = transactionType;
                DomArgs = domArgs;
                m_analysis = analysis;
            }

            public readonly int TransactionNum;

            public string Label
            {
                get { return TransactionNum.ToString(); }
            }

            public string Description
            {
                get
                {
                    switch (m_transactionType)
                    {
                        case DataType.Begin:
                            return string.Format("Transaction began : [{0}]", m_transactionName);
                        case DataType.Cancel:
                            return string.Format("Transaction cancelled : [{0}]", m_transactionName);
                        case DataType.End:
                            return string.Format("Transaction ended : [{0}]", m_transactionName);
                        case DataType.ChildAdded:
                            {
                                var args = (ChildEventArgs)DomArgs;
                                return string.Format("[{0}; id {1}]: add to [{2}; id {3}] at index {4}",
                                                     args.Child, args.Child.GetId() ?? args.Child.GetHashCode().ToString("X"),
                                                     args.Parent, args.Parent.GetId() ?? args.Parent.GetHashCode().ToString("X"), args.Index);
                            }
                        case DataType.ChildRemoved:
                            {
                                var args = (ChildEventArgs)DomArgs;
                                return string.Format("[{0}; id {1}]: remove from [{2} id {3}] at index {4}",
                                                     args.Child, args.Child.GetId() ?? args.Child.GetHashCode().ToString("X"),
                                                     args.Parent, args.Parent.GetId() ?? args.Parent.GetHashCode().ToString("X"), args.Index);
                            }
                        case DataType.AttributeChanged:
                            {
                                var args = (AttributeEventArgs)DomArgs;
                                return string.Format("[{0}; id {1}]: set [{2}] from [{3}] to [{4}]",
                                                     args.DomNode, args.DomNode.GetId() ?? args.DomNode.GetHashCode().ToString("X"),
                                                     args.AttributeInfo.Name, args.OldValue, args.NewValue);
                            }
                        default:
                            throw new InvalidOperationException("unknown enum value");
                    }
                }
            }

            public string Analysis
            {
                get { return m_analysis; }
            }

            private readonly string m_analysis;

            public string ReportLine
            {
                get
                {
                    if (string.IsNullOrEmpty(Analysis))
                        return string.Format("#{0}: {1}", TransactionNum, Description);
                    return string.Format("#{0}: {1}. {2}", TransactionNum, Description, Analysis);
                }
            }

            public EventArgs DomArgs;

            public bool HasChildren
            {
                get { return m_children != null && m_children.Count > 0; }
            }

            public IEnumerable<DataItem> Children
            {
                get { return (IEnumerable<DataItem>)m_children ?? EmptyArray<DataItem>.Instance; }
            }

            private List<DataItem> m_children = null;
            private readonly DataType m_transactionType;
            private readonly string m_transactionName;
        }

        private class DataContainer : NotifyPropertyChangedBase, ITreeListView, IItemView, IObservableContext /*, ISelectionContext*/ /*, IValidationContext*/
        {
            public DataContainer()
            {
                //if (s_dataImageIndex == -1)
                //{
                //    s_dataImageIndex =
                //        ResourceUtil.GetImageList16().Images.IndexOfKey(
                //            Resources.DataImage);
                //}

                //if (s_folderImageIndex == -1)
                //{
                //    s_folderImageIndex =
                //        ResourceUtil.GetImageList16().Images.IndexOfKey(
                //            Resources.FolderImage);
                //}
            }

            public DataItem this[int index]
            {
                get { return m_data[index]; }
            }

            public void AddTransactionBegin(string transactionName)
            {
                m_transactionNum++;
                var item = new DataItem(m_transactionNum, DataType.Begin, transactionName);
                m_transactionParent = item;
                m_data.Add(item);

                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item)); // index is ignored
            }

            public void AddTransactionEnd(string transactionName)
            {
                var item = new DataItem(m_transactionNum, DataType.End, transactionName);

                //doesn't work -- have to figure out how to map index to place in tree
                //m_transactionParent.Add(item);
                m_data.Add(item);

                m_transactionParent = null;

                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item)); // index is ignored
            }

            public void AddTransactionCancel(string transactionName)
            {
                var item = new DataItem(m_transactionNum, DataType.Cancel, transactionName);

                //doesn't work -- have to figure out how to map index to place in tree
                //m_transactionParent.Add(item);
                m_data.Add(item);

                m_transactionParent = null;

                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item)); // index is ignored
            }

            public void AddDomEvent(DataType dataType, EventArgs e, string analysis)
            {
                int transactionNum = m_transactionParent != null ? m_transactionNum : -1;
                var item = new DataItem(transactionNum, dataType, e, analysis);

                //doesn't work -- have to figure out how to map index to place in tree
                //if (m_transactionParent != null)
                //    m_transactionParent.Add(item);
                //else
                //    m_data.Add(item);
                m_data.Add(item);

                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, item)); // index is ignored
            }

            public int Count
            {
                get { return m_data.Count; }
            }

            public void Clear()
            {
                m_data.Clear();

                // Causes TreeListViewAdapter to clear everything, including the columns, and then
                //  rebuild everything.
                Reloaded.Raise(this, EventArgs.Empty);
            }

            #region ITreeListView Interface

            public IEnumerable<object> Roots
            {
                get { return m_data; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                var dataParent = parent.As<DataItem>();
                if (dataParent == null)
                    yield break;

                if (!dataParent.HasChildren)
                    yield break;

                foreach (var data in dataParent.Children)
                    yield return data;
            }

            public string[] ColumnNames
            {
                get { return s_columnNames; }
            }

            #endregion

            #region IItemView Interface

            public void GetInfo(object obj, ItemInfo info)
            {
                var data = obj.As<DataItem>();
                if (data == null)
                    return;

                info.Label = data.TransactionNum.ToString();
                info.Properties = new object[]
                    {
                        data.Description,
                        data.Analysis
                    };

                info.AllowLabelEdit = false;
                info.IsLeaf = !data.HasChildren;
                info.ImageIndex = data.HasChildren ? s_folderImageIndex : s_dataImageIndex;
            }

            #endregion

            #region IObservableContext Interface

            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
            {
                add { }
                remove { }
            }

            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
            {
                add { }
                remove { }
            }

            public event EventHandler Reloaded;

            #endregion

            private readonly ObservableCollection<DataItem> m_data =
                new ObservableCollection<DataItem>();

            private int m_transactionNum; //to count the transaction #s, starting with 1
            private DataItem m_transactionParent;

            private static int s_dataImageIndex = -1;
            private static int s_folderImageIndex = -1;

            private static readonly string[] s_columnNames =
                new[]
                    {
                        "Trans.#",
                        "Description",
                        "Analysis"
                    };
        }

        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private readonly Sce.Atf.Wpf.Applications.IControlHostService m_controlHostService;
        private readonly DataContainer m_data = new DataContainer();
        private bool m_analysisEnabled = false;
        private Dispatcher m_uiDispatcher;

        private IValidationContext m_validationContext;
    }
}