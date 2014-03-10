//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Canvas for drawing and picking curves</summary>
    public class CurveCanvas : Cartesian2dCanvas
    {
        #region Constructor(s)
        
        /// <summary>
        /// Default constructor</summary>
        public CurveCanvas()
        {
            AutoComputeCurveLimitsEnabled = true;
            m_renderer = new CurveRenderer();
            m_renderer.SetCartesian2dCanvas(this);
            Dock = DockStyle.Fill;
            m_selection.Changed += m_selection_SelectionChanged;
            m_readonlySelection = new ReadOnlyCollection<IControlPoint>(m_selection);
                        
            Controls.Add(new Control());
            m_cursors.Add(CursorType.Default, Cursors.Default);
            m_cursors.Add(CursorType.AddPoint, Cursors.Cross);
            m_cursors.Add(CursorType.CantDo, Cursors.No);
            m_cursors.Add(CursorType.MoveHz, Cursors.SizeWE);
            m_cursors.Add(CursorType.MoveVert, Cursors.SizeNS);
            m_cursors.Add(CursorType.Move, Cursors.SizeAll);
            m_cursors.Add(CursorType.Pan, new Cursor(typeof(CurveUtils), "Resources.Panhv.cur"));
            m_cursors.Add(CursorType.Selection, new Cursor(typeof(CurveUtils), "Resources.SelectionCursor.cur"));
            m_cursors.Add(CursorType.QuestionMark, new Cursor(typeof(CurveUtils), "Resources.QuestionMark.cur"));
            Cursor = m_cursors[CursorType.Selection];
            m_activeCursor = Cursor;

            m_contextMenu = new ContextMenuStrip();
            m_contextMenu.AutoClose = true;
            m_contextMenu.Opening += menustrip_Opening;


            m_undoMenuItem = new ToolStripMenuItem("Undo".Localize());
            m_undoMenuItem.Click += delegate { Undo(); };
            m_undoMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Undo, true);

            m_redoMenuItem = new ToolStripMenuItem("Redo".Localize());
            m_redoMenuItem.Click += delegate { Redo(); };
            m_redoMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Redo, true);

            //m_cutMenuItem = new ToolStripMenuItem("Cut".Localize());
            //m_cutMenuItem.Click += delegate { Cut(); };
            //m_cutMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Cut, true);

            //m_copyMenuItem = new ToolStripMenuItem("Copy".Localize());
            //m_copyMenuItem.Click += delegate { Copy(); };
            //m_copyMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Copy, true);

            //m_pasteMenuItem = new ToolStripMenuItem("Paste".Localize());
            //m_pasteMenuItem.Click += delegate { Paste(); };
            //m_pasteMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Paste, true);

            m_deleteMenuItem = new ToolStripMenuItem("Delete".Localize());
            m_deleteMenuItem.Click += delegate { Delete(); };
            m_deleteMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Delete, true);

            m_contextMenu.Items.Add(m_undoMenuItem);
            m_contextMenu.Items.Add(m_redoMenuItem);
            m_contextMenu.Items.Add(new ToolStripSeparator());
            m_contextMenu.Items.AddRange(new ToolStripItem[]
            {
                //m_cutMenuItem,
                //m_copyMenuItem,
                //m_pasteMenuItem,
                m_deleteMenuItem
            });
        }
       
        /// <summary>
        /// Initialize static members</summary>
        static CurveCanvas()
        {
            s_marqueePen = new Pen(Color.FromArgb(40, 40, 40));
            s_marqueePen.DashPattern = new float[] { 3, 3 };
            s_noAction = new Bitmap(typeof(CurveUtils), "Resources.NoAction.png");
        }
        #endregion

        #region Events

        /// <summary>
        /// Event that is raised after the edit mode changes</summary>
        public event EventHandler EditModeChanged = delegate { };

        /// <summary>
        /// Event that is raised after the selection changes</summary>
        public event EventHandler SelectionChanged = delegate { };


        /// <summary>
        /// Event that is raised after Curves property is set </summary>
        public event EventHandler CurvesChanged = delegate { };

        #endregion

        #region public and protected methods

        /// <summary>
        /// Removes all the control points of the curve from the selection</summary>
        /// <param name="curve">Curve whose control points are removed</param>
        public void RemoveCurveFromSelection(ICurve curve)
        {
            if (!m_curves.Contains(curve))
                throw new ArgumentException("curve not found");
            if (m_selection.Count > 0)
            {
                m_selection.BeginUpdate();
                IControlPoint[] snapShot = m_selection.GetSnapshot();
                foreach (IControlPoint cpt in snapShot)
                {
                    if (cpt.Parent == curve)
                    {
                        m_selection.Remove(cpt);
                        cpt.EditorData.SelectedRegion = PointSelectionRegions.None;
                    }
                }
                m_selection.EndUpdate();
            }
        }
        /// <summary>
        /// Undoes last operation</summary>
        public void Undo()
        {
            if (!m_historyContext.CanUndo)
                return;
            m_historyContext.Undo();
            UpdateCurveLimits();            
            Invalidate();
        }

        /// <summary>
        /// Redoes last operation</summary>        
        public void Redo()
        {
            if (!m_historyContext.CanRedo)
                return;
            m_historyContext.Redo();
            UpdateCurveLimits();
            Invalidate();
        }
        /// <summary>
        /// Cuts selected points</summary>
        public void Cut()
        {
            if (m_selection.Count > 0)
            {
                s_clipboard = new IControlPoint[m_selection.Count];
                m_transactionContext.DoTransaction(delegate
                     {
                         int i = 0;
                         foreach (IControlPoint cp in m_selection)
                         {
                             ICurve curve = cp.Parent;
                             curve.RemoveControlPoint(cp);
                             s_clipboard[i++] = cp;
                         }
                         foreach (ICurve curve in m_selectedCurves)
                             CurveUtils.ComputeTangent(curve);
                     }, "Cut".Localize());                     
                ClearSelection();
                UpdateCurveLimits();                     
                Invalidate();                
            }
        }

        /// <summary>
        /// Copies selected points to internal clipboard</summary>
        public void Copy()
        {
            if (m_selection.Count > 0)
            {
                s_clipboard = new IControlPoint[m_selection.Count];
                int i = 0;
                foreach (IControlPoint cp in m_selection)
                {
                    s_clipboard[i++] = cp.Clone();
                }
            }
        }

        /// <summary>
        /// Pastes into selected curves from internal clipboard</summary>
        public void Paste()
        {
            if (AutoComputeCurveLimitsEnabled &&  s_clipboard.Length > 0 && m_selectedCurves.Length > 0)
            {
                
                var sorted = s_clipboard.OrderBy(item => item.X).ToArray();
                // convert sorted points to local space.
                float fclipPtx = sorted[0].X;
                float fclipPty = sorted[0].Y;               
                sorted.ForEach( item => 
                    {
                        item.X -= fclipPtx;
                        item.Y -= fclipPty;
                    });


                float range = (sorted.Last().X - sorted.First().X) + CurveUtils.Epsilone;

                ICurve[] selectedCurves = new ICurve[m_selectedCurves.Length];
                m_selectedCurves.CopyTo(selectedCurves, 0);
                ClearSelection();
               
                m_selection.BeginUpdate();
                m_transactionContext.DoTransaction(delegate
                {
                    foreach (ICurve curve in selectedCurves)
                    {
                        if (curve.ControlPoints.Count == 0) continue;
                        
                        // relax the curve limits to allow offset.
                        curve.MinX = float.MinValue;
                        curve.MaxX = float.MaxValue;
                        curve.MinY = float.MaxValue;
                        curve.MaxY = float.MaxValue;
                        var insertAt = new PointF(curve.ControlPoints[0].X, curve.ControlPoints[0].Y);
                        CurveUtils.OffsetCurve(curve, range, 0);

                        int index = 0;
                        foreach (IControlPoint clipCpt in sorted)
                        {
                            IControlPoint cpt = clipCpt.Clone();
                            cpt.X += insertAt.X;
                            cpt.Y += insertAt.Y;
                            curve.InsertControlPoint(index++, cpt);
                            m_selection.Add(cpt);
                            cpt.EditorData.SelectedRegion = PointSelectionRegions.Point;
                        }
                        CurveUtils.ComputeTangent(curve);
                        UpdateCurveLimits();
                    }

                }, "Paste".Localize());
                m_selection.EndUpdate();
                UpdateCurveLimits();
                Invalidate();
            }
        }

        /// <summary>
        /// Deletes selected control points</summary>
        public void Delete()
        {
            if (m_selection.Count > 0)
            {
                m_transactionContext.DoTransaction(delegate
                {
                    foreach (IControlPoint cp in m_selection)
                    {
                        ICurve curve = cp.Parent;
                        curve.RemoveControlPoint(cp);
                    }
                    foreach (ICurve curve in m_selectedCurves)
                        CurveUtils.ComputeTangent(curve);
                }, "Delete".Localize());
                ClearSelection();
                UpdateCurveLimits();
                Invalidate();                               
            }
        }

        /// <summary>
        /// Applies CurveTangentTypes to all selected tangents for all selected control points</summary>        
        public void SetTangent(TangentSelection selectedTan, CurveTangentTypes tanType)
        {
            if (m_selection.Count == 0 || selectedTan == TangentSelection.None)
                return;
            if (selectedTan == TangentSelection.TangentIn)
            {
                if (tanType != CurveTangentTypes.Stepped && tanType != CurveTangentTypes.SteppedNext)
                {
                    m_transactionContext.DoTransaction(delegate
                    {
                        foreach (IControlPoint cpt in m_selection)
                        {
                            if (cpt.Parent.CurveInterpolation == InterpolationTypes.Linear)
                                continue;
                            if (cpt.EditorData.SelectedRegion == PointSelectionRegions.Point
                                || cpt.EditorData.SelectedRegion == PointSelectionRegions.TangentIn)
                                cpt.TangentInType = tanType;
                        }
                        // recompute tangents for the selected curves
                        ReComputeTangents();
                    }, "Edit Tangent".Localize()
                    );
                }
            }
            else if (selectedTan == TangentSelection.TangentOut)
            {
                m_transactionContext.DoTransaction(delegate
                {
                    foreach (IControlPoint cpt in m_selection)
                    {
                        if (cpt.Parent.CurveInterpolation == InterpolationTypes.Linear)
                            continue;
                        if (cpt.EditorData.SelectedRegion == PointSelectionRegions.Point
                            || cpt.EditorData.SelectedRegion == PointSelectionRegions.TangentOut)
                            cpt.TangentOutType = tanType;
                    }
                    // recompute tangents for the selected curves
                    ReComputeTangents();

                },"Edit Tangent".Localize()
                );
            }
            else if (selectedTan == TangentSelection.TangentInOut)
            {
                m_transactionContext.DoTransaction(delegate
                {
                    foreach (IControlPoint cpt in m_selection)
                    {
                        if (cpt.Parent.CurveInterpolation == InterpolationTypes.Linear)
                            continue;

                        if (cpt.EditorData.SelectedRegion == PointSelectionRegions.Point)
                        {
                            if (tanType != CurveTangentTypes.Stepped && tanType != CurveTangentTypes.SteppedNext)
                                cpt.TangentInType = tanType;
                            cpt.TangentOutType = tanType;
                        }
                        else if (cpt.EditorData.SelectedRegion == PointSelectionRegions.TangentIn)
                        {
                            if (tanType != CurveTangentTypes.Stepped && tanType != CurveTangentTypes.SteppedNext)
                                cpt.TangentInType = tanType;
                        }
                        else if (cpt.EditorData.SelectedRegion == PointSelectionRegions.TangentOut)
                        {
                            cpt.TangentOutType = tanType;
                        }
                    }
                    // recompute tangents for the selected curves
                    ReComputeTangents();

                }, "Edit Tangent".Localize()
                );
            }            
            Invalidate();
        }

        /// <summary>
        /// Breaks or unifies tangents for the selected points</summary>
        /// <param name="breaktan">True to break tangents, false to unify tangents</param>
        public void BreakTangents(bool breaktan)
        {
            if (m_selection.Count == 0)
                return;
            string cmdName = (breaktan) ? "Break Tangents".Localize() : "Unify Tangents".Localize();
            m_transactionContext.DoTransaction(delegate
            {
                foreach (IControlPoint cpt in m_selection)
                {
                    cpt.BrokenTangents = breaktan;
                }
            }, cmdName
                );
            Invalidate();

        }
        /// <summary>
        /// Sets pre-infinity to the given CurveLoopTypes for the selected curves</summary>
        /// <param name="infType">CurveLoopTypes to be set</param>
        public void SetPreInfinity(CurveLoopTypes infType)
        {
            if (m_selectedCurves.Length == 0)
                return;

            m_transactionContext.DoTransaction(delegate
            {
                foreach (ICurve curve in m_selectedCurves)
                    curve.PreInfinity = infType;
            }, "Edit Pre-Infinity".Localize());
            Invalidate();
        }

        /// <summary>
        /// Sets post-infinity to the given CurveLoopTypes for the selected curves</summary>
        /// <param name="infType">CurveLoopTypes to be set</param>
        public void SetPostInfinity(CurveLoopTypes infType)
        {
            if (m_selectedCurves.Length == 0)
                return;

            m_transactionContext.DoTransaction(delegate
            {
                foreach (ICurve curve in m_selectedCurves)
                    curve.PostInfinity = infType;
            }, "Edit Post-Infinity".Localize());
            Invalidate();

        }


        /// <summary>
        /// Frames selected points so all selected points fit in the window</summary>
        public void FitSelection()
        {            
            RectangleF bound = RectangleF.Empty;
            if (m_selection.Count == 0)
                return;

            List<IControlPoint> selectedPoints = new List<IControlPoint>(m_selection.Count);
            selectedPoints.AddRange(m_selection.GetSnapshot());

            // if only one point is selected, then try to fit at least two points.
            // framing single points is not practical
            if (selectedPoints.Count == 1)
            {
                ICurve curve = selectedPoints[0].Parent;
                ReadOnlyCollection<IControlPoint> curvePoints = curve.ControlPoints;

                int index = curve.ControlPoints.IndexOf(selectedPoints[0]);
                int lastIndex = curvePoints.Count - 1;
                if (index < lastIndex)
                {
                    selectedPoints.Add(curvePoints[index + 1]);
                }
                if (index > 0)
                {
                    selectedPoints.Add(curvePoints[index - 1]);
                }
            }
            bound = ComputeBound(selectedPoints.AsReadOnly());
            float padding = 0.1f; // in percent.
            if (bound != RectangleF.Empty)
            {
                bound.Inflate(bound.Width * padding, bound.Height * padding);
                Frame(bound);
                Invalidate();
            }
        }
      
        /// <summary>
        /// Frames selected control points or all visible curves if nothing is selected
        /// so that all points or curves fit in the window</summary>
        public void Fit()
        {
            if (m_selection.Count > 0)
            {
                FitSelection();
            }
            else
            {
                FitAll();
            }
        }

        /// <summary>
        /// Frames all curves so that all curves fit in the window</summary>
        public void FitAll()
        {
            if (m_curves.Count > 0)
            {
                RectangleF bound = RectangleF.Empty;
                foreach (ICurve curve in m_curves)
                {
                    if (!curve.Visible)
                        continue;
                    RectangleF curveBound = ComputeBound(curve.ControlPoints);
                    if (curveBound != RectangleF.Empty)
                        bound = RectangleF.Union(bound, curveBound);
                }

                float padding = 0.1f; // in percent.
                if (bound != RectangleF.Empty)
                {
                    bound.Inflate(bound.Width * padding, bound.Height * padding);
                    Frame(bound);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Snaps selected control points to their nearest major tick</summary>
        public void SnapToMajorTick()
        {
            if (m_selection.Count == 0)
                return;
            float majorTick = MajorTickX;
            m_transactionContext.DoTransaction(delegate
            {
                foreach (IControlPoint cpt in m_selection)
                {
                    SnapToX(cpt, majorTick);

                }
            }, "Snap".Localize());
            UpdateCurveLimits();
            Invalidate();
        }

        /// <summary>
        /// Snaps selected control points to their nearest minor tick</summary>
        public void SnapToMinorTick()
        {
            if (m_selection.Count == 0)
                return;
            float tick = MajorTickX / NumOfMinorTicks;
            m_transactionContext.DoTransaction(delegate
            {
                foreach (IControlPoint cpt in m_selection)
                {
                    SnapToX(cpt, tick);
                }
            }, "Snap".Localize());
            UpdateCurveLimits();
            Invalidate();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets whether the control editing curve properties</summary>
        public bool Editing
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether to auto compute curve limits
        /// The default is true.</summary>
        public bool AutoComputeCurveLimitsEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets enabling restricted control-point translation.
        /// If enabled, the user can only translate a control point between its 
        /// previous and next control points.</summary>
        public bool RestrictedTranslationEnabled
        {
            get;
            set;
        }
        /// <summary>
        /// Gets ITransactionContext</summary>
        public ITransactionContext TransactionContext
        {
            get { return m_transactionContext; }
        }

        /// <summary>
        /// Gets IHistoryContext</summary>
        public IHistoryContext HistoryContext
        {
            get { return m_historyContext; }
        }

        /// <summary>
        /// Gets and sets editing context</summary>
        public object Context
        {
            get { return m_context; }
            set
            {
                if (value == m_context)
                    return;
                m_context = value;
                m_historyContext = m_context.As<IHistoryContext>();
                if (m_historyContext == null)
                    m_historyContext = new DefaultHistoryContext();

                m_transactionContext = m_context.As<ITransactionContext>();
                if (m_transactionContext == null)
                    m_transactionContext = new DefaultTransactionContext();

                m_curves = s_emptyCurves;
                ClearSelection();
                Invalidate();
            }
        }
        /// <summary>
        /// Gets or sets enabling auto snap drag point to other curve points</summary>
        public bool AutoPointSnap
        {
            get { return m_autoPointSnap; }
            set { m_autoPointSnap = value; }
        }

        /// <summary>
        /// Gets or sets enabling auto snap drag points to a curve</summary>
        public bool AutoCurveSnap
        {
            get { return m_autoCurveSnap; }
            set { m_autoCurveSnap = value; }
        }

        /// <summary>
        /// Gets context menu</summary>
        public new ContextMenuStrip ContextMenuStrip
        {
            get { return m_contextMenu; }
        }
        
        /// <summary>
        /// Gets whether can paste</summary>
        public bool CanPaste
        {
            get
            {
                return s_clipboard.Length > 0 && m_selection.Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets enabling auto snap to major tick along x axis</summary>
        public bool AutoSnapToX
        {
            get { return m_autoSnapToX; }
            set { m_autoSnapToX = value; }
        }

        /// <summary>
        /// Gets or sets enabling auto snap to major tick along y axis</summary>
        public bool AutoSnapToY
        {
            get { return m_autoSnapToY; }
            set { m_autoSnapToY = value; }
        }

        /// <summary>
        /// Gets read only collection of selected points</summary>
        public ReadOnlyCollection<IControlPoint> Selection
        {
            get { return m_readonlySelection; }
        }


        /// <summary>
        /// Gets all the selected curves</summary>        
        public ICurve[] SelectedCurves
        {
            get { return m_selectedCurves; }
        }

        /// <summary>
        /// Gets or sets edit mode</summary>
        public EditModes EditMode
        {
            get { return m_editMode; }
            set
            {
                if (m_editMode == value)
                    return;
                m_editMode = value;
                SetCursor(m_editMode);
                EditModeChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets input mode</summary>
        public InputModes InputMode
        {
            get { return m_inputMode; }
            set
            {
                m_inputMode = value;
                if (m_inputMode == InputModes.Basic)
                {
                    m_activeCursor = m_cursors[CursorType.Selection];
                    Cursor = m_activeCursor;
                }
                else if (m_inputMode == InputModes.Advanced)
                {
                    SetCursor(m_editMode);
                }
            }
        }


        /// <summary>
        /// Gets or sets read only collection of all curves for editing</summary>
        public ReadOnlyCollection<ICurve> Curves
        {
            get { return m_curves; }
            set
            {
                if (m_curves == value)
                    return;
                m_curves = (value == null) ? s_emptyCurves : value;
                ClearSelection();
                UpdateCurveLimits();
                PanToOrigin(false);
                CurvesChanged(this, EventArgs.Empty);
                Invalidate();
            }
        }

        #endregion

        #region base overrides


        /// <summary>
        /// Processes dialog key</summary>
        /// <param name="keyData">Key to process</param>
        /// <returns>True iff dialog key processed</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {            
            if (keyData == ( Keys.Menu | Keys.Alt ))
                return true; // disable menu activvation via alt key.

            return base.ProcessDialogKey(keyData);                            
        }

        /// <summary>
        /// Calling the base ProcessCmdKey allows this key press to be consumed by owning
        /// controls like PropertyView and PropertyGridView and be seen by ControlHostService.
        /// Returning false allows the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// Returning true means that this key press has been consumed by this method and this
        /// event is not passed on to any other methods or controls.</summary>
        /// <param name="msg">Window message to process</param>
        /// <param name="keyData">Key data</param>
        /// <returns>False to allow the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// True to consume this key press, so this
        /// event is not passed on to any other methods or controls.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool consumed = true;

            // extract keycode from Keys
            Keys kc = keyData & Keys.KeyCode;
            bool CtrlPressed = (keyData & Keys.Control) == Keys.Control;

            if (keyData == ShortcutKeys.Undo)
            {
                Undo();
            }
            else if (keyData == ShortcutKeys.Redo)
            {
                Redo();
            }
            else if (keyData == ShortcutKeys.Cut)
            {
                Cut();
            }
            else if (keyData == ShortcutKeys.Copy)
            {
                Copy();
            }
            else if (keyData == ShortcutKeys.Paste)
            {
                Paste();
            }
            else if (keyData == ShortcutKeys.Fit)
            {
                Fit();

            }
            else if (keyData == ShortcutKeys.FitAll)
            {
                FitAll();
            }
            else if (keyData == ShortcutKeys.PanToOrigin)
            {
                PanToOrigin();
            }
            else if (keyData == ShortcutKeys.Scale)
            {
                EditMode = EditModes.Scale;
            }
            else if (keyData == ShortcutKeys.Delete)
            {
                Delete();
            }
            else if (keyData == ShortcutKeys.Move)
            {
                EditMode = EditModes.Move;
            }
            else if (keyData == ShortcutKeys.Deselect)
            {
                ClearSelection();
            }
            else if (CtrlPressed && kc == Keys.Y)
            {

            }
            else
            {
                consumed = false;
            }
            if (!consumed)
            {
                consumed = base.ProcessCmdKey(ref msg, keyData);
            }
            else
            {
                Invalidate();
            }
            return consumed;
        }

        /// <summary>
        /// Performs custom actions on MouseDown events. Handles selection and picking.</summary>        
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {            
            Parent.Focus();
            base.OnMouseDown(e);
            m_mouseDownAction = MouseDownAction.None;
            MouseEditAction editAction = MouseEditAction.None;
            m_moveAxis = MoveAxis.None;
            m_selectionClickPoint = ClickPoint;
            
            m_originalValues = null;
            m_scalePivot = ClickGraphPoint;
            m_limitHit = null;
            m_zoomCenterStart = new PointD(
                  (ClickPoint.X - Pan_d.X) / Zoom_d.X,
                  (ClickPoint.Y - Pan_d.Y) / Zoom_d.Y);
            m_updateCurveLimits = false;
            m_visibleCurves.Clear();
            foreach (ICurve curve in m_curves)
            {
                if (curve.Visible)
                    m_visibleCurves.Add(curve);
            }

            if (m_autoSnapToX)
            {
                m_scalePivot.X = CurveUtils.SnapTo(m_scalePivot.X, MajorTickX);
            }
            if (m_autoSnapToY)
            {
                m_scalePivot.Y = CurveUtils.SnapTo(m_scalePivot.Y, MajorTickY);
            }
            if (m_inputMode == InputModes.Basic)
            {
                editAction = BasicOnMouseDown(e);
            }
            else if (m_inputMode == InputModes.Advanced)
            {
                editAction = AdvancedOnMouseDown(e);
            }
           
            PerformAction(editAction);
            Invalidate();
        }

        /// <summary>
        /// Performs custom actions on MouseMove events. Handles moving control points and tangents.</summary>        
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (DraggingOverThreshold)
            {

                MouseEditAction editAction = MouseEditAction.None;
                if (m_mouseDownAction == MouseDownAction.Pan)
                {
                    editAction = MouseEditAction.Panning;
                }
                else if (m_mouseDownAction == MouseDownAction.Zoom)
                {
                    editAction = MouseEditAction.Zooming;
                }
                else if (m_mouseDownAction == MouseDownAction.FreeMove)
                {
                    editAction = MouseEditAction.MoveSelection;
                }
                else if (m_mouseDownAction == MouseDownAction.CurveLimitResize)
                {
                    editAction = MouseEditAction.CurveLimitResize;
                }
                else if (m_inputMode == InputModes.Basic)
                {
                    editAction = BasicOnMouseMove(e);
                }
                else if (m_inputMode == InputModes.Advanced)
                {
                    editAction = AdvancedOnMouseMove(e);
                }
               
                // start transaction for move and scale
                if (!m_transactionContext.InTransaction)
                {
                    if (m_mouseDownAction == MouseDownAction.FreeMove
                        || m_mouseDownAction == MouseDownAction.ConstrainedMove)
                    {
                        m_transactionContext.Begin("Move".Localize());
                    }
                    else if (m_mouseDownAction == MouseDownAction.FreeScale
                        || m_mouseDownAction == MouseDownAction.ConstrainedScale)
                    {
                        m_transactionContext.Begin("Scale".Localize());
                    }
                    else if (m_mouseDownAction == MouseDownAction.CurveLimitResize)
                    {
                        m_transactionContext.Begin("Resize Curve Limit".Localize("could be phrased as 'resize the boundary of the curve'"));
                    }
                }

                PerformAction(editAction);
                if (editAction != MouseEditAction.None || m_drawInsertLine)
                    Invalidate();
            }
            else if (e.Button == MouseButtons.None && m_curves.Count > 0)
            {
                if (PickCurveLimits(out m_limitSide) != null)
                {
                    this.Cursor = (m_limitSide == CurveLimitSides.Left ||
                      m_limitSide == CurveLimitSides.Right) ? m_cursors[CursorType.MoveHz]
                          : m_cursors[CursorType.MoveVert];
                }
                else
                {

                    if (Cursor != m_activeCursor)
                        Cursor = m_activeCursor;
                }
            }
        }

        /// <summary>
        /// Performs custom actions on MouseUp events. Handles selection and applying changes.</summary>        
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // if there is ongoing transaction end it
            if (m_transactionContext.InTransaction)
                m_transactionContext.End();

            // action selection 
            MouseEditAction editAction = MouseEditAction.None;
            if (m_inputMode == InputModes.Basic)
            {
                editAction = BasicOnMouseUp(e);
            }
            else if (m_inputMode == InputModes.Advanced)
            {
                editAction = AdvancedOnMouseUp(e);
            }

            if (e.Button == MouseButtons.Right)
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    m_contextMenu.Show(this, e.X, e.Y);
                }
            }
            PerformAction(editAction);
            Cursor = m_activeCursor;
            if (m_updateCurveLimits)
                UpdateCurveLimits();
            m_updateCurveLimits = false;
            m_drawInsertLine = false;
            m_drawScalePivot = false;
            m_mouseDownAction = MouseDownAction.None;
            base.OnMouseUp(e);
            Invalidate();
        }

        /// <summary>
        /// Performs custom actions on Paint events. Draws grid and curve.</summary>        
        /// <param name="e">Paint event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {        
            DrawCartesianGrid(e.Graphics);

            if (m_curves.Count > 0)
            {
                float w = ClientSize.Width;
                float h = ClientSize.Height;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Pen pen = new Pen(Color.Black, 2);

                // draw axis labels
                foreach (ICurve curve in m_curves)
                    DrawXYLabel(e.Graphics, curve.XLabel, curve.YLabel);

                // draw each curve.
                foreach (ICurve curve in m_curves)
                {
                    if (!curve.Visible)
                        continue;
                    m_renderer.DrawCurve(curve, e.Graphics);
                }

                // draw control points.
                foreach (ICurve curve in m_curves)
                {
                    if (!curve.Visible)
                        continue;
                    m_renderer.DrawControlPoints(curve, e.Graphics);
                }

                // draw curve limits for selected curves.
                foreach (ICurve curve in m_selectedCurves)
                {
                    if (!curve.Visible)
                        continue;
                    Vec2F min = new Vec2F(curve.MinX, curve.MinY);
                    Vec2F max = new Vec2F(curve.MaxX, curve.MaxY);
                    min = GraphToClient(min);
                    max = GraphToClient(max);
                    float x1 = min.X;
                    float x2 = max.X;
                    float y1 = min.Y;
                    float y2 = max.Y;

                    // to avoind gdi+ overflow and drawing limit where the width or height 
                    // is less than one pixel. 
                    // can't use  e.Graphics.DrawRectangle(pen, rect); 

                    // cull, clip and draw curve limit guide lines.
                    pen.Color = curve.CurveColor;
                    bool cull = (x1 < 0 && x2 < 0) || (x1 > w && x2 > w);
                    if (!cull)
                    {
                        // clip
                        float lx1 = Math.Max(0, x1);
                        float lx2 = Math.Min(w, x2);

                        // draw top line
                        if (y2 > 0 && y2 < h)
                        {
                            e.Graphics.DrawLine(pen, lx1, y2, lx2, y2);
                        }
                        // draw bottom line 
                        if (y1 > 0 && y1 < h)
                        {
                            e.Graphics.DrawLine(pen, lx1, y1, lx2, y1);
                        }
                    }
                    cull = (y1 < 0 && y2 < 0) || (y1 > h && y2 > h);
                    if (!cull)
                    {
                        // clip
                        float ly1 = Math.Min(h, y1);
                        float ly2 = Math.Max(0, y2);
                        // draw left line
                        if (x1 > 0 && x1 < w)
                        {
                            e.Graphics.DrawLine(pen, x1, ly2, x1, ly1);
                        }
                        // draw right line
                        if (x2 > 0 && x2 < w)
                        {
                            e.Graphics.DrawLine(pen, x2, ly2, x2, ly1);
                        }
                    }
                }

                if (m_drawScalePivot)
                {
                    Vec2F scalePivot = GraphToClient(m_scalePivot);
                    Rectangle pvRect = new Rectangle((int)scalePivot.X - 4, (int)scalePivot.Y - 4, 8, 8);
                    e.Graphics.DrawLine(Pens.Black, 0, scalePivot.Y, Width, scalePivot.Y);
                    e.Graphics.DrawLine(Pens.Black, scalePivot.X, 0, scalePivot.X, Height);
                    e.Graphics.DrawRectangle(Pens.Black, pvRect);
                }
                else if (m_drawInsertLine)
                {
                    Pen insertLinePen = new Pen(m_insertLineColor);
                    e.Graphics.DrawLine(insertLinePen, CurrentPoint.X, 0, CurrentPoint.X, Height);
                    foreach (ICurve curve in m_selectedCurves)
                    {

                        ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
                                                
                        float y = cv.Evaluate(CurrentGraphPoint.X);
                        Vec2F pt = new Vec2F(CurrentGraphPoint.X, y);
                        pt = GraphToClient(pt);
                        RectangleF ptRect = m_mouseRect;
                        ptRect.X = pt.X - ptRect.Width / 2;
                        ptRect.Y = pt.Y - ptRect.Height / 2;
                        int index = CurveUtils.GetValidInsertionIndex(curve, CurrentGraphPoint.X);
                        if (index >= 0)
                        {
                            e.Graphics.DrawRectangle(insertLinePen, Rectangle.Truncate(ptRect));
                        }
                        else
                        {
                            pt.X = pt.X - s_noAction.Width / 2;
                            pt.Y = pt.Y - s_noAction.Height / 2;
                            e.Graphics.DrawImage(s_noAction, pt);
                        }
                    }
                    insertLinePen.Dispose();
                }

                pen.Dispose();
            }

            //draw selection rect.
            if (SelectionRect != RectangleF.Empty)
            {
                e.Graphics.DrawRectangle(s_marqueePen, Rectangle.Truncate(SelectionRect));
            }
        }
        #endregion

        #region private helper methods


        private MouseEditAction AdvancedOnMouseDown(MouseEventArgs e)
        {
            MouseEditAction editAction = MouseEditAction.None;
            bool saveSelection = false;

            if (e.Button == MouseButtons.Left)
            {
                m_limitHit = PickCurveLimits(out m_limitSide);                     
                m_mouseDownAction = (m_limitHit == null) ? MouseDownAction.SelectionRectangle
                    : MouseDownAction.CurveLimitResize;
                if (m_limitSide != CurveLimitSides.None)
                {
                    Cursor = (m_limitSide == CurveLimitSides.Left ||
                        m_limitSide == CurveLimitSides.Right) ? m_cursors[CursorType.MoveHz]
                        : m_cursors[CursorType.MoveVert];
                }
            }
            else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None)
                   || (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Control))
            {
                if (m_selection.Count > 0 || m_visibleCurves.Count == 1)
                {
                    if (m_editMode == EditModes.AddPoint)
                    {
                        editAction = MouseEditAction.AddControlPoint;
                    }
                    else if (m_editMode == EditModes.InsertPoint)
                    {
                        m_drawInsertLine = true;
                    }
                    else if (m_selection.Count > 0)
                    {
                        if (m_editMode == EditModes.Move)
                        {
                            Cursor = m_cursors[CursorType.Move];
                            m_mouseDownAction = MouseDownAction.FreeMove;
                            saveSelection = true;
                        }
                        else if (m_editMode == EditModes.Scale)
                        {
                            Cursor = m_cursors[CursorType.Move];
                            m_mouseDownAction = MouseDownAction.FreeScale;
                            m_drawScalePivot = true;
                            saveSelection = true;
                        }
                    }
                }
                else if (m_curves.Count > 0 && m_editMode == EditModes.AddPoint)
                {// special case, add new point to empty curve.

                    editAction = MouseEditAction.AddControlPointToEmptyCurve;
                }
            }
            else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.Shift)
                   || (e.Button == MouseButtons.Right && Control.ModifierKeys == (Keys.Control | Keys.Shift)))
            {
                if (m_selection.Count > 0)
                {
                    if (m_editMode == EditModes.Move)
                    {
                        Cursor = m_cursors[CursorType.QuestionMark];
                        m_mouseDownAction = MouseDownAction.ConstrainedMove;
                        saveSelection = true;
                    }
                    else if (m_editMode == EditModes.Scale)
                    {
                        Cursor = m_cursors[CursorType.QuestionMark];
                        m_mouseDownAction = MouseDownAction.ConstrainedScale;
                        m_drawScalePivot = true;
                        saveSelection = true;
                    }
                }
            }
            else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.Alt)
                 || (e.Button == MouseButtons.Right && Control.ModifierKeys == (Keys.Control | Keys.Alt)))
            {
                m_mouseDownAction = MouseDownAction.Pan;
                Cursor = m_cursors[CursorType.Pan];
            }
            else if (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Alt)
            {
                m_mouseDownAction = MouseDownAction.Zoom;
            }

            if (saveSelection)
            {
                m_originalValues = new PointF[m_selection.Count];
                for (int i = 0; i < m_selection.Count; i++)
                {
                    IControlPoint cp = m_selection[i];
                    m_originalValues[i] = new PointF(cp.X, cp.Y);
                }
            }
            return editAction;

        }
        private MouseEditAction AdvancedOnMouseMove(MouseEventArgs e)
        {
            MouseEditAction editAction = MouseEditAction.None;
            if (m_mouseDownAction == MouseDownAction.SelectionRectangle)
            {
                if (Control.ModifierKeys == Keys.Alt)
                {
                    m_selectionClickPoint.X += (CurrentPoint.X - PreviousPoint.X);
                    m_selectionClickPoint.Y += (CurrentPoint.Y - PreviousPoint.Y);
                }
                SelectionRect = MakeRect(m_selectionClickPoint, CurrentPoint);
                Invalidate();
            }
            else if (m_mouseDownAction == MouseDownAction.ConstrainedMove)
            {
                if (m_moveAxis == MoveAxis.None)
                {
                    // compute dx and dy then choose move axis.
                    float dx = CurrentPoint.X - ClickPoint.X;
                    float dy = CurrentPoint.Y - ClickPoint.Y;
                    m_moveAxis = (Math.Abs(dx) > Math.Abs(dy)) ? MoveAxis.X : MoveAxis.Y;
                    if (m_moveAxis == MoveAxis.X)
                        Cursor = m_cursors[CursorType.MoveHz];
                    else
                        Cursor = m_cursors[CursorType.MoveVert];
                }
                editAction = (m_moveAxis == MoveAxis.X) ? MouseEditAction.MoveSelectionAlongX : MouseEditAction.MoveSelectionAlongY;
            }
            else if (m_mouseDownAction == MouseDownAction.FreeScale)
            {
                editAction = MouseEditAction.PivotScale;
            }
            else if (m_mouseDownAction == MouseDownAction.ConstrainedScale)
            {
                if (m_moveAxis == MoveAxis.None)
                {
                    // compute dx and dy then choose move axis.                        
                    float dx = CurrentPoint.X - ClickPoint.X;
                    float dy = CurrentPoint.Y - ClickPoint.Y;
                    m_moveAxis = (Math.Abs(dx) > Math.Abs(dy)) ? MoveAxis.X : MoveAxis.Y;
                    if (m_moveAxis == MoveAxis.X)
                        Cursor = m_cursors[CursorType.MoveHz];
                    else
                        Cursor = m_cursors[CursorType.MoveVert];
                }
                editAction = (m_moveAxis == MoveAxis.X) ? MouseEditAction.PivotScaleAlongX : MouseEditAction.PivotScaleAlongY;
            }

            return editAction;
        }
        private MouseEditAction AdvancedOnMouseUp(MouseEventArgs e)
        {
            // action selection 
            MouseEditAction editAction = MouseEditAction.None;
            if (e.Button == MouseButtons.Left && m_limitHit == null)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    editAction = MouseEditAction.RemoveFromSelection;
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    editAction = MouseEditAction.ToggleSelection;
                }
                else if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    editAction = MouseEditAction.AddToSelection;
                }
                else if (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Alt)
                {
                    editAction = MouseEditAction.Select;
                }
            }
            else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None)
                || (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Control))
            {
                if ((m_selection.Count > 0 || m_visibleCurves.Count == 1) && EditMode == EditModes.InsertPoint)
                {
                    editAction = MouseEditAction.InsertPoint;
                }
            }
            return editAction;
        }

        private MouseEditAction BasicOnMouseDown(MouseEventArgs e)
        {
            // action selection             
            MouseEditAction editAction = MouseEditAction.None;
            bool saveSelection = false;

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
            {
                if (Control.ModifierKeys == Keys.None && e.Button == MouseButtons.Left)
                {
                    m_limitHit = PickCurveLimits(out m_limitSide);                    
                    if (m_limitHit != null)
                    {
                        m_mouseDownAction = MouseDownAction.CurveLimitResize;
                        Cursor = (m_limitSide == CurveLimitSides.Left ||
                           m_limitSide == CurveLimitSides.Right) ? m_cursors[CursorType.MoveHz]
                           : m_cursors[CursorType.MoveVert];
                    }
                    else
                    {
                        List<IControlPoint> points = new List<IControlPoint>();
                        List<PointSelectionRegions> regions = new List<PointSelectionRegions>();
                        RectangleF pickRect = m_mouseRect;
                        pickRect.X = CurrentPoint.X - m_mouseRect.Width / 2;
                        pickRect.Y = CurrentPoint.Y - m_mouseRect.Height / 2;
                        m_renderer.PickPoints(m_curves, pickRect, points, regions);
                        IControlPoint pickedpt = (points.Count > 0) ? points[0] : null;
                        PointSelectionRegions pickedRegion =
                            (regions.Count > 0) ? regions[0] : PointSelectionRegions.None;
                        if (pickedpt != null)
                        {
                            if (!m_selection.Contains(pickedpt))
                            {
                                SetSelection(points, regions);

                            }
                            else if (pickedpt.EditorData.SelectedRegion != pickedRegion)
                            {
                                SetSelection(points, regions);
                            }
                            //else if (points.Count == pickedpt.Parent.ControlPoints.Count)
                            //{
                            //    SetSelection(points, regions);
                            //}
                            m_mouseDownAction = MouseDownAction.FreeMove;
                            saveSelection = true;
                        }
                        else
                        {
                            m_mouseDownAction = MouseDownAction.SelectionRectangle;
                        }
                    }
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    if (m_selection.Count > 0 || m_visibleCurves.Count == 1)
                        editAction = MouseEditAction.AddControlPoint;
                    else if (m_curves.Count > 0)
                        editAction = MouseEditAction.AddControlPointToEmptyCurve;


                }
                else if (Control.ModifierKeys == Keys.Alt)
                {
                    m_mouseDownAction = MouseDownAction.Pan;
                    Cursor = m_cursors[CursorType.Pan];

                }
            }
            else if (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Alt)
            {
                m_mouseDownAction = MouseDownAction.Zoom;
            }

            if (saveSelection)
            {
                m_originalValues = new PointF[m_selection.Count];
                for (int i = 0; i < m_selection.Count; i++)
                {
                    IControlPoint cp = m_selection[i];
                    m_originalValues[i] = new PointF(cp.X, cp.Y);
                }
            }
            return editAction;


        }
        private MouseEditAction BasicOnMouseMove(MouseEventArgs e)
        {
            // action selection 
            MouseEditAction editAction = MouseEditAction.None;
            if (m_mouseDownAction == MouseDownAction.SelectionRectangle)
            {
                SelectionRect = MakeRect(m_selectionClickPoint, CurrentPoint);
                Invalidate();
            }
            return editAction;
        }
        private MouseEditAction BasicOnMouseUp(MouseEventArgs e)
        {
            // action selection 
            MouseEditAction editAction = MouseEditAction.None;
            if (e.Button == MouseButtons.Left && m_limitHit == null)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    editAction = MouseEditAction.ToggleSelection;
                }
                else if (m_mouseDownAction == MouseDownAction.SelectionRectangle)
                {
                    editAction = MouseEditAction.Select;
                }
            }
            return editAction;
        }


        private void SetCursor(EditModes mode)
        {
            switch (mode)
            {
                case EditModes.Move:
                case EditModes.Scale:
                    m_activeCursor = m_cursors[CursorType.Selection];
                    break;
                case EditModes.AddPoint:
                case EditModes.InsertPoint:
                    m_activeCursor = m_cursors[CursorType.AddPoint];
                    break;
                default:
                    m_activeCursor = m_cursors[CursorType.Default];
                    break;
            }
            Cursor = m_activeCursor;
        }

        private void menustrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasSelection = m_selection.Count > 0;
            ContextMenuStrip menustrip = sender as ContextMenuStrip;
            foreach (ToolStripItem item in menustrip.Items)
            {
                item.Enabled = hasSelection;
            }

            //m_cutMenuItem.Visible = false;
            //m_copyMenuItem.Visible = false;
            //m_pasteMenuItem.Visible = false;


            m_undoMenuItem.Enabled = m_historyContext.CanUndo;
            m_undoMenuItem.Text = "Undo".Localize() + " " + m_historyContext.UndoDescription;
            m_redoMenuItem.Enabled = m_historyContext.CanRedo;
            m_redoMenuItem.Text = "Redo".Localize() + " " + m_historyContext.RedoDescription;
            //m_pasteMenuItem.Enabled = hasSelection && s_clipboard.Length > 0;

        }

        /// <summary>
        /// Computes bound for points</summary>
        private RectangleF ComputeBound(ReadOnlyCollection<IControlPoint> points)
        {
            RectangleF bound = RectangleF.Empty;
            if (points.Count > 0)
            {
                IControlPoint cpt = points[0];
                bound = new RectangleF(cpt.X, cpt.Y, 0, 0);
                for (int i = 1; i < points.Count; i++)
                {
                    cpt = points[i];
                    RectangleF boundCpt = new RectangleF(cpt.X, cpt.Y, 0, 0);
                    bound = RectangleF.Union(bound, boundCpt);
                }
                if (bound.Width == 0) bound.Width = float.Epsilon;
                if (bound.Height == 0) bound.Height = float.Epsilon;
            }
            return bound;
        }


        /// <summary>
        /// Performs given action.
        /// The source of action could be mouse, keyboard, or GUI button.</summary>        
        private void PerformAction(MouseEditAction action)
        {
            float dx = CurrentPoint.X - ClickPoint.X;
            float dy = CurrentPoint.Y - ClickPoint.Y;
            bool hasSelection = m_selection.Count > 0;
            List<IControlPoint> points = new List<IControlPoint>();
            List<PointSelectionRegions> regions = new List<PointSelectionRegions>();
            RectangleF pickRect = RectangleF.Empty;
            if (DraggingOverThreshold)
            {
                pickRect = SelectionRect;
            }
            else
            {
                pickRect = m_mouseRect;
                pickRect.X = CurrentPoint.X - m_mouseRect.Width / 2;
                pickRect.Y = CurrentPoint.Y - m_mouseRect.Height / 2;
            }

            switch (action)
            {
                case MouseEditAction.Panning:
                    {
                        Pan_d = new PointD(ClickPan_d.X + dx, ClickPan_d.Y + dy);
                    }
                    break;
                case MouseEditAction.Zooming:
                    {
                        float xScale = 1 + 4.0f * dx / (float)Width;
                        float yScale = 1 + 4.0f * -dy / (float)Height;
                        Zoom_d = new PointD(ClickZoom_d.X * xScale, ClickZoom_d.Y * yScale);
                        Pan_d = new PointD((ClickPoint.X - m_zoomCenterStart.X * Zoom_d.X),
                            (ClickPoint.Y - m_zoomCenterStart.Y * Zoom_d.Y));                       
                    }
                    break;
                case MouseEditAction.Select:
                    {
                        m_renderer.Pick(m_curves, pickRect, points, regions);
                        SetSelection(points, regions);
                    }
                    break;
                case MouseEditAction.AddToSelection:
                    {
                        m_renderer.Pick(m_curves, pickRect, points, regions);
                        AddToSelection(points, regions);
                    }
                    break;
                case MouseEditAction.RemoveFromSelection:
                    {
                        m_renderer.Pick(m_curves, pickRect, points, regions);
                        RemoveFromSelection(points, regions);
                    }
                    break;
                case MouseEditAction.ToggleSelection:
                    {
                        m_renderer.Pick(m_curves, pickRect, points, regions);
                        ToggleSelection(points, regions);
                    }
                    break;

                case MouseEditAction.MoveSelectionAlongX:
                case MouseEditAction.MoveSelectionAlongY:
                case MouseEditAction.MoveSelection:
                    {
                        m_updateCurveLimits = true;
                        float gdx = 0;
                        float gdy = 0;
                        if (action == MouseEditAction.MoveSelectionAlongX)
                        {
                            gdx = CurrentGraphPoint.X - ClickGraphPoint.X;
                        }
                        else if (action == MouseEditAction.MoveSelectionAlongY)
                        {
                            gdy = CurrentGraphPoint.Y - ClickGraphPoint.Y;
                        }
                        else
                        {
                            gdx = CurrentGraphPoint.X - ClickGraphPoint.X;
                            gdy = CurrentGraphPoint.Y - ClickGraphPoint.Y;
                        }
                        Translate(gdx, gdy);                       
                    }
                    break;

                case MouseEditAction.PivotScaleAlongX:
                case MouseEditAction.PivotScaleAlongY:
                case MouseEditAction.PivotScale:
                    {
                        m_updateCurveLimits = true;
                        float scale = 10.0f;
                        float xScale = 0;
                        float yScale = 0;
                        if (action == MouseEditAction.PivotScaleAlongX)
                        {
                            xScale = 1 + scale * dx / (float)Width;
                        }
                        else if (action == MouseEditAction.PivotScaleAlongY)
                        {
                            yScale = 1 + scale * -dy / (float)Height;
                        }
                        else
                        {
                            xScale = 1 + scale * dx / (float)Width;
                            yScale = 1 + scale * -dy / (float)Height;
                        }
                        ScalePoints(xScale, yScale);
                    }
                    break;
                case MouseEditAction.AddControlPoint:
                    {
                        m_updateCurveLimits = true;
                        // create control point and add it the selected curves.
                        m_transactionContext.DoTransaction(delegate
                        {
                            if (m_selectedCurves.Length > 0)
                            {
                                foreach (ICurve curve in m_selectedCurves)
                                {
                                    CurveUtils.AddControlPoint(curve, ClickGraphPoint, false);
                                }
                            }
                            else if (m_visibleCurves.Count == 1)
                            {
                                CurveUtils.AddControlPoint(m_visibleCurves[0], ClickGraphPoint, false);
                            }

                        }, "Add Control Point".Localize());                        
                    }
                    break;

                case MouseEditAction.AddControlPointToEmptyCurve:
                    {
                        m_updateCurveLimits = true;
                        if (m_curves.Count >0)
                        {
                            ICurve emptyCurve = null;
                            foreach (ICurve curve in m_curves)
                            {
                                if (curve.Visible && curve.ControlPoints.Count == 0)
                                {
                                    emptyCurve = curve;
                                    break;
                                }
                            }
                            if (emptyCurve != null)
                            {
                                // create control point and add it the emptyCurve curve.                        
                                m_transactionContext.DoTransaction(delegate
                                {
                                    CurveUtils.AddControlPoint(emptyCurve, ClickGraphPoint, false);
                                }, "Add Control Point".Localize());

                                regions.Add(PointSelectionRegions.Point);
                                points.Add(emptyCurve.ControlPoints[0]);
                                AddToSelection(points, regions);
                            }
                        }
                    }
                    break;
                case MouseEditAction.InsertPoint:
                    {
                        m_updateCurveLimits = true;
                        // create control point and add it the active curve.                        
                        m_transactionContext.DoTransaction(delegate
                        {
                            if (m_selectedCurves.Length > 0)
                            {
                                foreach (ICurve curve in m_selectedCurves)
                                {
                                    CurveUtils.AddControlPoint(curve, CurrentGraphPoint, true);
                                }
                            }
                            else if (m_visibleCurves.Count == 1)
                            {
                                CurveUtils.AddControlPoint(m_visibleCurves[0], CurrentGraphPoint, true);
                            }

                        },"Insert Control Point".Localize());
                    }
                    break;
                case MouseEditAction.CurveLimitResize:
                    {
                        try
                        {
                            Editing = true;
                            if (m_limitHit != null)
                            {
                                if (m_limitSide == CurveLimitSides.Left)
                                    m_limitHit.MinX = CurrentGraphPoint.X;
                                else if (m_limitSide == CurveLimitSides.Right)
                                    m_limitHit.MaxX = CurrentGraphPoint.X;
                                else if (m_limitSide == CurveLimitSides.Top)
                                    m_limitHit.MaxY = CurrentGraphPoint.Y;
                                else if (m_limitSide == CurveLimitSides.Bottom)
                                    m_limitHit.MinY = CurrentGraphPoint.Y;

                                ValidateCurveLimits(m_limitHit, m_limitSide);
                            }
                        }
                        catch (InvalidTransactionException ex)
                        {
                            if (m_transactionContext.InTransaction)
                                m_transactionContext.Cancel();

                            if (ex.ReportError)
                                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                        }
                        finally
                        {
                            Editing = false;                            
                        }
                    }
                    break;
                default:
                    break;
            }
            m_lastEditAction = action;
        }

        
        /// <summary>
        /// Updates limits for all curves</summary>
        public void UpdateCurveLimits()
        {
            if (m_curves.Count == 0 || !AutoComputeCurveLimitsEnabled)
                return;
            bool oldval = Editing;
            try
            {
               
                Editing = true;
                foreach (ICurve curve in m_curves)
                {
                    RectangleF bound = ComputeBound(curve.ControlPoints);
                    if (bound != RectangleF.Empty)
                    {
                        // update curve limits.
                        curve.MinX = bound.Left;
                        curve.MaxX = bound.Right;
                        curve.MinY = bound.Top;
                        curve.MaxY = bound.Bottom;
                    }
                }
            }
            finally
            {
                Editing = oldval;
            }
        }

        private ICurve PickCurveLimits(out CurveLimitSides side)
        {
            side = CurveLimitSides.None;
            if (AutoComputeCurveLimitsEnabled)
                return null;

            ICurve hit = null;
            const float pickTol = 4;
            RectangleF rect = RectangleF.Empty;

            for (int i = SelectedCurves.Length - 1; i >= 0; i--)
            {
                ICurve curve = SelectedCurves[i];
                Vec2F min = new Vec2F(curve.MinX, curve.MinY);
                Vec2F max = new Vec2F(curve.MaxX, curve.MaxY);
                min = GraphToClient(min);
                max = GraphToClient(max);
                RectangleF limitRect = MakeRect(min, max);
                RectangleF outerRect = RectangleF.Inflate(limitRect, pickTol, pickTol);
                if (!curve.Visible || !outerRect.Contains(CurrentPoint))
                    continue;
                RectangleF innerRect = RectangleF.Inflate(limitRect, -pickTol, -pickTol);
                if (!innerRect.Contains(CurrentPoint))
                {
                    m_limitHits.Clear();
                    m_limitHits[Math.Abs(min.X - CurrentPoint.X)] = CurveLimitSides.Left;
                    m_limitHits[Math.Abs(max.X - CurrentPoint.X)] = CurveLimitSides.Right;
                    m_limitHits[Math.Abs(min.Y - CurrentPoint.Y)] = CurveLimitSides.Bottom;
                    m_limitHits[Math.Abs(max.Y - CurrentPoint.Y)] = CurveLimitSides.Top;
                    foreach (KeyValuePair<float, CurveLimitSides> kv in m_limitHits)
                    {
                        if (kv.Key <= pickTol)
                        {
                            side = kv.Value;
                            hit = curve;
                            break;
                        }
                    }
                    break;
                }
            }
            return hit;
        }


        private void ValidateCurveLimits(ICurve curve, CurveLimitSides side)
        {            
            switch (side)
            {
                case CurveLimitSides.Left:
                    {
                        IControlPoint firstPt = curve.ControlPoints.Count > 0 ? curve.ControlPoints[0] : null;
                        float left = firstPt != null ? firstPt.X : curve.MaxX - CurveUtils.Epsilone;
                        if (curve.MinX > left)
                            curve.MinX = left;
                    }
                    break;
                case CurveLimitSides.Right:
                    {
                        IControlPoint lastPt = curve.ControlPoints.Count > 0 ? curve.ControlPoints[curve.ControlPoints.Count-1] : null;
                        float right = lastPt != null ? lastPt.X : curve.MinX + CurveUtils.Epsilone;
                        if (curve.MaxX < right)
                            curve.MaxX = right;
                    }
                    break;
                case CurveLimitSides.Top:
                    {
                        float top = curve.MinY + CurveUtils.Epsilone;
                        foreach (var cpt in curve.ControlPoints)
                            if (cpt.Y > top) top = cpt.Y;

                        if (curve.MaxY < top)
                            curve.MaxY = top;
                    }
                    break;
                case CurveLimitSides.Bottom:
                    {
                        float bottom = curve.MaxY - CurveUtils.Epsilone;
                        foreach (var cpt in curve.ControlPoints)
                            if (cpt.Y < bottom) bottom = cpt.Y;
                        if (curve.MinY > bottom)
                            curve.MinY = bottom;
                    }
                    break;
            }
        }

        /// <summary>
        /// Snaps p1 to v2</summary>                
        private void SnapPoint(IControlPoint p1, Vec2F v2, float threshold)
        {
            Vec2F v1 = new Vec2F(p1.X, p1.Y);
            Vec2F v3 = v1 - v2;
            v3 = GraphToClientTangent(v3);
            float dist = v3.Length;
            if (dist > threshold)
                return;

            p1.Y = v2.Y;
            float s = v2.X;

            ICurve curve = p1.Parent;
            int index = curve.ControlPoints.IndexOf(p1);
            if (index == -1)
                throw new ArgumentException("p1");

            IControlPoint neighbor = null;
            if (p1.X > s) // snap left.
            {
                neighbor = (index != 0) ? curve.ControlPoints[index - 1] : null;
                if (neighbor != null && Math.Abs(neighbor.X - s) <= CurveUtils.Epsilone)
                {
                    p1.X = neighbor.X + CurveUtils.Epsilone;
                }
                else
                {
                    p1.X = s;
                }
            }
            else if (p1.X < s)
            {
                neighbor = ((index + 1) < curve.ControlPoints.Count) ? curve.ControlPoints[index + 1] : null;

                if (neighbor != null && Math.Abs(neighbor.X - s) <= CurveUtils.Epsilone)
                {
                    p1.X = neighbor.X - CurveUtils.Epsilone;
                }
                else
                {
                    p1.X = s;
                }
            }
        }

        /// <summary>
        /// Snaps control point along y axis to snapValue</summary>        
        private void SnapToY(IControlPoint cpt, float snapValue)
        {
            float s = CurveUtils.SnapTo(cpt.Y, snapValue);
            cpt.Y = s;
        }

        /// <summary>
        /// Snaps control point along x axis to snapvalue</summary>        
        private void SnapToX(IControlPoint cpt, float snapValue)
        {
            ICurve curve = cpt.Parent;
            int index = curve.ControlPoints.IndexOf(cpt);
            if (index == -1)
                throw new ArgumentException("cpt");
            float s = CurveUtils.SnapTo(cpt.X, snapValue);
            IControlPoint neighbor = null;

            if (cpt.X > s) // snap left.
            {
                neighbor = (index != 0) ? curve.ControlPoints[index - 1] : null;
                if (neighbor != null && Math.Abs(neighbor.X - s) <= CurveUtils.Epsilone)
                {
                    cpt.X = neighbor.X + CurveUtils.Epsilone;
                }
                else
                {
                    cpt.X = s;
                }
            }
            else if (cpt.X < s)
            {
                neighbor = ((index + 1) < curve.ControlPoints.Count) ? curve.ControlPoints[index + 1] : null;

                if (neighbor != null && Math.Abs(neighbor.X - s) <= CurveUtils.Epsilone)
                {
                    cpt.X = neighbor.X - CurveUtils.Epsilone;
                }
                else
                {
                    cpt.X = s;
                }
            }
        }

        private void ScalePoints(float xScale, float yScale)
        {
            if (m_selection.Count == 0)
                return;

            try
            {
                m_curveSet.Clear();
                for (int i = 0; i < m_selection.Count; i++)
                {

                    IControlPoint cp = m_selection[i];
                    if (cp.EditorData.SelectedRegion != PointSelectionRegions.Point)
                        continue;

                    if (yScale != 0)
                    {
                        float y = m_originalValues[i].Y;
                        // transform to pivot space then scale then back to graph space.
                        y -= m_scalePivot.Y;
                        y *= yScale;
                        y += m_scalePivot.Y;
                        cp.Y = y;
                    }
                    if (xScale != 0)
                    {
                        float x = m_originalValues[i].X;
                        // xform x to pivot space.
                        x -= m_scalePivot.X;
                        // scale x 
                        x *= xScale;
                        // xform x back to graph space.
                        x += ClickGraphPoint.X;
                        cp.X = x;                       
                    }
                }

                for (int i = 0; i < m_selection.Count; i++)
                {
                    IControlPoint cp = m_selection[i];
                    if (!CurveUtils.IsSorted(cp))
                        m_curveSet.Add(cp.Parent);
                }

                if (RestrictedTranslationEnabled)
                {
                    if (m_curveSet.Count > 0)
                    {
                        for (int i = 0; i < m_selection.Count; i++)
                        {
                            var cp = m_selection[i];
                            var orgCp = m_originalValues[i];
                            cp.X = orgCp.X;
                            cp.Y = orgCp.Y;                            
                        }
                    }
                }
                else
                {
                    // sort curves
                    foreach (ICurve curve in m_curveSet)
                    {
                        CurveUtils.Sort(curve);
                    }
                }


                foreach (ICurve curve in m_selectedCurves)
                {
                    CurveUtils.ForceMinDistance(curve, CurveUtils.Epsilone);
                    CurveUtils.ComputeTangent(curve);
                }
                
            }
            catch (InvalidTransactionException ex)
            {
                if (m_transactionContext.InTransaction)
                    m_transactionContext.Cancel();

                if (ex.ReportError)
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }

        }


        private void Translate(float dx, float dy)
        {
            if (m_selection.Count == 0)
                return;

            try
            {
                Predicate<float> RangeFailed = (angle) => angle < MinAngle || angle > MaxAngle;
                m_curveSet.Clear();
                float epsilon = CurveUtils.Epsilone;
                // move drag points by delta
                for (int i = 0; i < m_selection.Count; i++)
                {
                    IControlPoint cp = m_selection[i];
                    PointSelectionRegions region = cp.EditorData.SelectedRegion;
                    if (region == PointSelectionRegions.TangentIn
                        || region == PointSelectionRegions.TangentOut)
                    {

                        Vec2F tanIn = cp.TangentIn;
                        Vec2F tanOut = cp.TangentOut;
                        float curTanInAngle = (float)Math.Atan2(tanIn.Y, tanIn.X);
                        float curTanOutAngle = (float)Math.Atan2(tanOut.Y, tanOut.X);
                        float newTanInAngle = 0.0f;
                        float newTanOutAngle = 0.0f;

                        if (region == PointSelectionRegions.TangentIn)
                        {
                            float prevAngle = (float)Math.Atan2(PreviousGraphPoint.Y - cp.Y, cp.X - PreviousGraphPoint.X);
                            float curAngle = (float)Math.Atan2(CurrentGraphPoint.Y - cp.Y, cp.X - CurrentGraphPoint.X);
                            float deltaAngle = prevAngle - curAngle;
                            
                            newTanInAngle = curTanInAngle + deltaAngle;
                            newTanOutAngle = curTanOutAngle + (newTanInAngle - curTanInAngle);

                            if (RangeFailed(newTanInAngle) || RangeFailed(newTanOutAngle)) continue;

                            cp.TangentIn = new Vec2F((float)Math.Cos(newTanInAngle), (float)Math.Sin(newTanInAngle));
                            cp.TangentInType = CurveTangentTypes.Fixed;
                            if (!cp.BrokenTangents
                                && cp.TangentOutType != CurveTangentTypes.Stepped
                                && cp.TangentOutType != CurveTangentTypes.SteppedNext)
                            {
                                cp.TangentOut = new Vec2F((float)Math.Cos(newTanOutAngle), (float)Math.Sin(newTanOutAngle));
                                cp.TangentOutType = CurveTangentTypes.Fixed;
                            }
                        }
                        else
                        {
                            float prevAngle = (float)Math.Atan2(PreviousGraphPoint.Y - cp.Y, PreviousGraphPoint.X - cp.X);
                            float curAngle = (float)Math.Atan2(CurrentGraphPoint.Y - cp.Y, CurrentGraphPoint.X - cp.X);
                            float deltaAngle = curAngle - prevAngle;
                            
                            newTanOutAngle = curTanOutAngle + deltaAngle;
                            newTanInAngle =  curTanInAngle + (newTanOutAngle - curTanOutAngle);
                            if (RangeFailed(newTanInAngle) || RangeFailed(newTanOutAngle)) continue;

                            cp.TangentOut = new Vec2F((float)Math.Cos(newTanOutAngle), (float)Math.Sin(newTanOutAngle));
                            cp.TangentOutType = CurveTangentTypes.Fixed;
                            if (!cp.BrokenTangents
                               && cp.TangentInType != CurveTangentTypes.Stepped
                               && cp.TangentInType != CurveTangentTypes.SteppedNext)
                            {
                                cp.TangentIn = new Vec2F((float)Math.Cos(newTanInAngle), (float)Math.Sin(newTanInAngle));
                                cp.TangentInType = CurveTangentTypes.Fixed;
                            }
                        }
                    }
                    else if (region == PointSelectionRegions.Point)
                    {
                        PointF dragPos = m_originalValues[i];
                        cp.X = dragPos.X + dx;
                        cp.Y = dragPos.Y + dy;                        
                    }

                }

                for (int i = 0; i < m_selection.Count; i++)
                {
                    IControlPoint cp = m_selection[i];
                    if (!CurveUtils.IsSorted(cp))
                        m_curveSet.Add(cp.Parent);
                }

                if (RestrictedTranslationEnabled)
                {
                    if (m_curveSet.Count > 0)
                    {
                        for (int i = 0; i < m_selection.Count; i++)
                        {
                            var cp = m_selection[i];
                            var orgCp = m_originalValues[i];
                            cp.X = orgCp.X;
                            cp.Y = orgCp.Y;
                        }
                    }
                }
                else
                {
                    // sort curves
                    foreach (ICurve curve in m_curveSet)
                    {
                        CurveUtils.Sort(curve);
                    }

                    // snap.
                    float majorTickX = MajorTickX;
                    float majorTickY = MajorTickY;
                    foreach (IControlPoint cpt in m_selection)
                    {
                        float xval = cpt.X;
                        float yval = cpt.Y;

                        if (m_autoPointSnap)
                        {
                            foreach (ICurve curve in m_curves)
                            {
                                if (cpt.Parent == curve)
                                    continue;

                                foreach (IControlPoint innercpt in curve.ControlPoints)
                                {
                                    if (innercpt.EditorData.SelectedRegion != PointSelectionRegions.None)
                                        continue;
                                    SnapPoint(cpt, new Vec2F(innercpt.X, innercpt.Y), SnapThreshold);
                                }
                            }
                        }
                        if (m_autoCurveSnap)
                        {
                            foreach (ICurve curve in m_curves)
                            {
                                if (cpt.Parent == curve)
                                    continue;
                                ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
                                float y = cv.Evaluate(cpt.X);
                                SnapPoint(cpt, new Vec2F(cpt.X, y), SnapThreshold);
                            }
                        }

                        if (m_autoSnapToX && xval == cpt.X)
                            SnapToX(cpt, majorTickX);
                        if (m_autoSnapToY && yval == cpt.Y)
                            SnapToY(cpt, MajorTickY);
                    }

                    foreach (ICurve curve in m_selectedCurves)
                    {
                        CurveUtils.ForceMinDistance(curve, CurveUtils.Epsilone);                     
                    }
                }
                foreach (ICurve curve in m_selectedCurves)
                {                 
                    CurveUtils.ComputeTangent(curve);
                }
            }
            catch (InvalidTransactionException ex)
            {
                if (m_transactionContext.InTransaction)
                    m_transactionContext.Cancel();

                if (ex.ReportError)
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
            }
        }


        private void SetSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
        {
            m_selection.BeginUpdate();
            ClearSelection();
            for (int i = 0; i < points.Count; i++)
            {
                IControlPoint cpt = points[i];
                cpt.EditorData.SelectedRegion = regions[i];
                m_selection.Add(cpt);
            }
            m_selection.EndUpdate();

        }

        private void AddToSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
        {
            m_selection.BeginUpdate();
            for (int i = 0; i < points.Count; i++)
            {
                IControlPoint cpt = points[i];
                cpt.EditorData.SelectedRegion = regions[i];
                m_selection.Add(cpt);
            }
            m_selection.EndUpdate();
        }

        private void RemoveFromSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
        {
            m_selection.BeginUpdate();
            for (int i = 0; i < points.Count; i++)
            {
                IControlPoint cpt = points[i];
                cpt.EditorData.SelectedRegion = PointSelectionRegions.None;
                m_selection.Remove(cpt);
            }
            m_selection.EndUpdate();
        }

        private void ToggleSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
        {
            m_selection.BeginUpdate();
            for (int i = 0; i < points.Count; i++)
            {
                IControlPoint cpt = points[i];
                PointSelectionRegions region = regions[i];
                if (m_selection.Contains(cpt))
                {
                    if (cpt.EditorData.SelectedRegion == region)
                    {
                        m_selection.Remove(cpt);
                        cpt.EditorData.SelectedRegion = PointSelectionRegions.None;
                    }
                    else
                    {
                        cpt.EditorData.SelectedRegion = region;
                    }
                }
                else
                {
                    cpt.EditorData.SelectedRegion = region;
                    m_selection.Add(cpt);
                }
            }
            m_selection.EndUpdate();

        }

        private void m_selection_SelectionChanged(object sender, EventArgs e)
        {
            Dictionary<ICurve, object> curves = new Dictionary<ICurve, object>();
            foreach (IControlPoint cpt in m_selection)
            {
                if (cpt.EditorData.SelectedRegion == PointSelectionRegions.None)
                    cpt.EditorData.SelectedRegion = PointSelectionRegions.Point;
                curves[cpt.Parent] = null;
            }
            m_selectedCurves = new ICurve[curves.Count];
            curves.Keys.CopyTo(m_selectedCurves, 0);

            SelectionChanged(this, EventArgs.Empty);
        }

        private void ClearSelection()
        {
            foreach (IControlPoint cpt in m_selection)
                cpt.EditorData.SelectedRegion = PointSelectionRegions.None;
            m_selection.Clear();
        }
        
        private void ReComputeTangents()
        {
            foreach (ICurve curve in m_selectedCurves)
            {
                if (curve.CurveInterpolation == InterpolationTypes.Linear)
                    continue;
                CurveUtils.ComputeTangent(curve);
            }
        }

        #endregion

        #region private fields

        
        private PointD m_zoomCenterStart;
        private readonly SortedDictionary<float, CurveLimitSides> m_limitHits = new SortedDictionary<float, CurveLimitSides>();
        private ICurve m_limitHit;
        private CurveLimitSides m_limitSide;

        private const float SnapThreshold = 16.0f; // in pixel
       
        // context strip items.
        private object m_context;
        private ITransactionContext m_transactionContext = new DefaultTransactionContext();
        private IHistoryContext m_historyContext = new DefaultHistoryContext();
        private readonly ContextMenuStrip m_contextMenu;
        private readonly ToolStripMenuItem m_undoMenuItem;
        private readonly ToolStripMenuItem m_redoMenuItem;
        //private readonly ToolStripMenuItem m_cutMenuItem;
        //private readonly ToolStripMenuItem m_copyMenuItem;
        //private readonly ToolStripMenuItem m_pasteMenuItem;
        private readonly ToolStripMenuItem m_deleteMenuItem;
        private static IControlPoint[] s_clipboard = new IControlPoint[0];
        private readonly List<ICurve> m_visibleCurves
            = new List<ICurve>();
        private Vec2F m_scalePivot;
        private bool m_updateCurveLimits;
        private bool m_autoPointSnap;
        private bool m_autoCurveSnap;
        private bool m_autoSnapToX; // auto snap on horizontal major tick
        private bool m_autoSnapToY; // auto snap on vertical major tick
        private readonly HashSet<ICurve> m_curveSet = new HashSet<ICurve>(); // used for scaling and translation.
        private PointF[] m_originalValues; // used for scaling.
        private MoveAxis m_moveAxis = MoveAxis.None;
        private PointF m_selectionClickPoint;
        private MouseDownAction m_mouseDownAction = MouseDownAction.None;
        private readonly Color m_insertLineColor = Color.FromArgb(240, 240, 83);
        private bool m_drawScalePivot;
        private bool m_drawInsertLine;
        private RectangleF m_mouseRect = new RectangleF(0, 0, 10, 10);
        private Cursor m_activeCursor;
        private readonly Dictionary<CursorType, Cursor> m_cursors = new Dictionary<CursorType, Cursor>();
        private EditModes m_editMode = EditModes.Move;
        private InputModes m_inputMode = InputModes.Basic;
        private readonly Selection<IControlPoint> m_selection = new Selection<IControlPoint>();
        private readonly ReadOnlyCollection<IControlPoint> m_readonlySelection;
        private ICurve[] m_selectedCurves = new ICurve[0];
        private ReadOnlyCollection<ICurve> m_curves = s_emptyCurves;
        private static readonly ReadOnlyCollection<ICurve> s_emptyCurves = (new List<ICurve>()).AsReadOnly();
        private readonly CurveRenderer m_renderer;
        private MouseEditAction m_lastEditAction = MouseEditAction.None;

        private const float MaxAngle = 1.5706217938697f; // = 89.99 degree
        private const float MinAngle = -1.5706217938697f; // =-89.99 degree        
        
        private static readonly Bitmap s_noAction;
        private static readonly Pen s_marqueePen;
        #endregion

        #region  enums and classes

        private class DefaultTransactionContext : ITransactionContext
        {
            #region ITransactionContext Members

            public void Begin(string transactionName)
            {                
            }

            public bool InTransaction
            {
                get { return false; }
            }

            public void Cancel()
            {                
            }

            public void End()
            {                
            }

            #endregion
        }

        private class DefaultHistoryContext : IHistoryContext
        {

            #region IHistoryContext Members

            public bool CanUndo
            {
                get { return false; }
            }

            public bool CanRedo
            {
                get { return false; }
            }

            public string UndoDescription
            {
                get { return string.Empty; }
            }

            public string RedoDescription
            {
                get { return string.Empty; }
            }

            public void Undo()
            {
            }

            public void Redo()
            {
            }

            public bool Dirty
            {
                get
                {
                    return false;
                }
                set
                {
                    DirtyChanged.Raise(this, EventArgs.Empty);
                }
            }

            public event EventHandler DirtyChanged;

            #endregion
        }

        /// <summary>
        /// Default shortcut keys</summary>
        public static class ShortcutKeys
        {
            /// <summary>Undo</summary>            
            public static Keys Undo = Keys.Control | Keys.Z;

            /// <summary>Redo</summary>            
            public static Keys Redo = Keys.Control | Keys.Y;

            /// <summary>Cut</summary>            
            public static Keys Cut = Keys.Control | Keys.X;

            /// <summary>Copy</summary>            
            public static Keys Copy = Keys.Control | Keys.C;

            /// <summary>Paste</summary>            
            public static Keys Paste = Keys.Control | Keys.V;

            /// <summary>Delete</summary>            
            public static Keys Delete = Keys.Delete;

            /// <summary>Activate move tool</summary>
            public static Keys Move = Keys.W;

            /// <summary>Activate scale tool</summary>            
            public static Keys Scale = Keys.R;

            /// <summary>Fit selected points</summary>            
            public static Keys Fit = Keys.F;

            /// <summary>Fit all the visible curves</summary>            
            public static Keys FitAll = Keys.A;

            /// <summary>Pan to origin</summary>            
            public static Keys PanToOrigin = Keys.C;

            /// <summary>Clear selection</summary>
            public static Keys Deselect = Keys.Escape;
        }

        /// <summary>Mouse editing action enums</summary>
        private enum MouseEditAction
        {
            /// <summary>No action</summary>
            None,

            /// <summary>Pan</summary>
            Panning,

            /// <summary>Zoom</summary>
            Zooming,

            /// <summary>Select picked control points</summary>
            Select,

            /// <summary>Add picked control points to the current selection</summary>
            AddToSelection,

            /// <summary>Remove picked control points from selection</summary>
            RemoveFromSelection,

            /// <summary>Toggle picked control point from/to selection</summary>            
            ToggleSelection,

            /// <summary>Drag selected control control points and/or tangents along x axis</summary>           
            MoveSelectionAlongX,

            /// <summary>Drag selected control control points and/or tangents along y axis</summary>           
            MoveSelectionAlongY,

            /// <summary>Drag selected control control points and/or tangents</summary>           
            MoveSelection,

            /// <summary>Add control point to the selected curves</summary>
            AddControlPoint,

            /// <summary>
            /// Add control point to first empty curve.
            /// This action is performed when no curve is selected.
            /// In this case, a new control point is added to first empty curve.
            /// If no empty curve is found, no point is added.</summary>
            AddControlPointToEmptyCurve,

            /// <summary>Insert control point into the selected curves</summary>
            InsertPoint,

            /// <summary>Scale selected points around pivot</summary>            
            PivotScale,

            /// <summary>Scale selected points along x axis</summary>
            PivotScaleAlongX,

            /// <summary>Scale selected point along y axis</summary>            
            PivotScaleAlongY,

            /// <summary>Resize curve limit</summary>
            CurveLimitResize,
        }

        /// <summary>
        /// Curve limit sides</summary>
        [Flags]
        enum CurveLimitSides
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
        }

        /// <summary>
        /// Move axis</summary>
        [Flags]
        private enum MoveAxis
        {
            None,
            X,
            Y,
        }

        private enum MouseDownAction
        {
            None,
            SelectionRectangle,
            Pan,
            Zoom,
            FreeMove,
            ConstrainedMove,
            FreeScale,
            ConstrainedScale,
            CurveLimitResize
        }

        private enum CursorType
        {
            Default,
            QuestionMark,
            Selection,
            MoveHz,
            MoveVert,
            Move,
            CantDo,
            AddPoint,
            Pan,
        }

        #endregion
    }

    
    /// <summary>
    /// Edit modes</summary>
    public enum EditModes
    {
        /// <summary>No edit</summary>
        None,

        /// <summary>Add control point at the current mouse position</summary>
        AddPoint,

        /// <summary>Insert control point at mouse x and curve y</summary>
        InsertPoint,

        /// <summary>Translation</summary>
        Move,

        /// <summary>Scale</summary>
        Scale,
    }

    /// <summary>
    /// Select tangent. This enum is used by the SetTangent() method.</summary>    
    public enum TangentSelection
    {
        /// <summary>None</summary>
        None,

        /// <summary>Tangent in</summary>
        TangentIn,

        /// <summary>Tangent out</summary>
        TangentOut,

        /// <summary>Tangent in and out</summary>
        TangentInOut,
    }

    /// <summary>
    /// Input and mouse handling modes.
    /// Basic is simple and easy to learn.
    /// Advanced is compatible with Maya's graph editor.</summary>
    public enum InputModes
    {
        /// <summary>
        /// Basic mode</summary>
        Basic,

        /// <summary>
        /// Sdvanced mode, similiar to Maya graph editor</summary>
        Advanced,
    }
}
