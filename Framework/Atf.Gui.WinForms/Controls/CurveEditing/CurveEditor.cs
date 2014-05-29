//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Component to edit curves using the CurveEditingControl</summary>
    [Export(typeof(CurveEditor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CurveEditor : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">ICommandService</param>
        /// <param name="controlHostService">IControlHostService</param>
        /// <param name="contextRegistry">IContextRegistry</param>
        [ImportingConstructor]
        public CurveEditor(ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry)
        {
            // The commandService parameter is currently unused 
            // but kept for backwards compatibility & potential future use

            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;
            
            m_curveEditorControl = new CurveEditingControl();
            m_controlInfo = new ControlInfo(
                "Curve Editor".Localize(),
                "Edits selected object curves".Localize(),
                StandardControlGroup.Bottom);
        }

        /// <summary>
        /// Constructor that a derived class can use to provide additional customizations</summary>
        /// <param name="commandService">ICommandService</param>
        /// <param name="controlHostService">IControlHostService</param>
        /// <param name="contextRegistry">IContextRegistry</param>
        /// <param name="curveEditingControl">CurveEditingControl to use</param>
        /// <param name="controlInfo">ControlInfo for CurveEditingControl</param>
        public CurveEditor(ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry,
            CurveEditingControl curveEditingControl,
            ControlInfo controlInfo)
        {
            // The commandService parameter is currently unused 
            // but kept for backwards compatibility & potential future use

            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;
            m_curveEditorControl = curveEditingControl;
            m_controlInfo = controlInfo;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
            m_controlHostService.RegisterControl(m_curveEditorControl, m_controlInfo, this);            

            // register settings for input modes.
            if (m_settingsService != null)
            {
                // create setting to store command shortcuts
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(
                        m_curveEditorControl, () => m_curveEditorControl.InputMode, "Input mode".Localize(), null, null)
                );

                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(
                        m_curveEditorControl, () => m_curveEditorControl.LockOrigin,
                        "Lock origin".Localize("This is the name of a command. Lock is a verb. Origin is like the origin of a graph."), null, null)
                );

                m_settingsService.RegisterSettings(this,
                   new BoundPropertyDescriptor(
                       m_curveEditorControl, () => m_curveEditorControl.FlipY, "Flip Y-axis".Localize("same as 'flip vertical axis'"), null, null)
               );
            }
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>True if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Gets the main CurveEditingControl</summary>
        public CurveEditingControl Control { get { return m_curveEditorControl; } }

        /// <summary>
        /// Gets or sets whether curves from multiple selected objects are shown overlayed
        /// in the same editor. If false (= default), only curves from the last
        /// selected object are displayed.</summary>
        public bool MultiSelectionOverlay { get; set; }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {            
            m_curveEditorControl.Context = m_contextRegistry.ActiveContext;

            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;
            m_selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;

            if (m_validationContext != null)
            {
                m_validationContext.Ended -= RefreshCurveControl;
                m_validationContext.Cancelled -= RefreshCurveControl;
            }

            m_validationContext = m_contextRegistry.GetActiveContext<IValidationContext>();
            if (m_validationContext != null)
            {
                m_validationContext.Ended += RefreshCurveControl;
                m_validationContext.Cancelled += RefreshCurveControl;
            }

            if (m_observableContext != null)
                m_observableContext.ItemChanged -= ObservableContextItemChanged;
            m_observableContext = m_contextRegistry.GetActiveContext<IObservableContext>();
            if (m_observableContext != null)
                m_observableContext.ItemChanged += ObservableContextItemChanged;
        }

        void RefreshCurveControl(object sender, EventArgs e)
        {
            if(m_curveItemChanged)
                m_curveEditorControl.Refresh();
            m_curveItemChanged = false;
        }

        /// <summary>
        /// Performs custom actions on SelectionChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void selectionContext_SelectionChanged(object sender, EventArgs e)
        {
            IList<ICurve> curves;
            if (MultiSelectionOverlay)
            {
                // Restore original curve colors
                foreach (KeyValuePair<ICurve, Color> pair in m_originalCurveColors)
                    pair.Key.CurveColor = pair.Value;
                m_originalCurveColors.Clear();
                
                // Merge curves from all selected objects
                List<ICurve> curveList = new List<ICurve>();
                foreach (object obj in m_selectionContext.Selection)
                    curveList.AddRange(GetCurves(obj));

                // Auto-assign curve colors (equally spaced in the color spectrum)
                if (m_selectionContext.SelectionCount > 1)
                {   
                    for (int i = 0; i < curveList.Count; i++)
                    {
                        ICurve curve = curveList[i];
                        float hue = (i * 360.0f) / (float)curveList.Count;
                        m_originalCurveColors[curve] = curve.CurveColor; // Remember original curve color
                        curve.CurveColor = ColorUtil.FromAhsb(255, hue, 1.0f, 0.5f);
                    }
                }

                curves = curveList;
            }
            else
                curves = GetCurves(m_selectionContext.LastSelected);

            foreach (ICurve curve in curves)
                CurveUtils.ComputeTangent(curve);

            m_curveEditorControl.Curves = new ReadOnlyCollection<ICurve>(curves);
        }

        /// <summary>
        /// Gets curves from a selected object, obtaining an empty list if none</summary>
        /// <param name="selectedObject">ICurveSet, ICurve or Path with either as last curve</param>
        /// <returns>Curves from a selected object, empty list if none</returns>
        private static IList<ICurve> GetCurves(object selectedObject)
        {
            Path<object> path = selectedObject as Path<object>;
            object selected = path != null ? path.Last : selectedObject;
            
            ICurveSet curveSet = selected.As<ICurveSet>();
            if (curveSet != null && curveSet.Curves != null)
                return curveSet.Curves;
            
            ICurve curve = selected.As<ICurve>();
            if (curve != null)
                return new List<ICurve> {curve};

            // Return empty list if object is incompatible
            return new List<ICurve>();
        }

        /// <summary>
        /// Performs custom actions on ItemChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void ObservableContextItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            m_curveItemChanged = e.Item.Is<ICurve>() || e.Item.Is<IControlPoint>();            
        }

        // Required MEF Imports
        private readonly IControlHostService m_controlHostService;
        private readonly IContextRegistry m_contextRegistry;

        // Optional MEF Imports
        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        // Contexts
        private ISelectionContext m_selectionContext;
        private IObservableContext m_observableContext;
        private IValidationContext m_validationContext;
        private bool m_curveItemChanged;
        private readonly CurveEditingControl m_curveEditorControl;
        private readonly ControlInfo m_controlInfo;
        private readonly Dictionary<ICurve, Color> m_originalCurveColors = new Dictionary<ICurve, Color>();
    }
}
