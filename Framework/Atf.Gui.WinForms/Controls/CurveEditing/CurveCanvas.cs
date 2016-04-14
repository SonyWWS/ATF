//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;
using Sce.Atf.Controls.PropertyEditing;

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
            m_currentX = new CurrentX(this);
            m_pasteOptions = new PasteOptions(this);
            OnlyEditSelectedCurves = true;
            
            m_renderer = new CurveRenderer();
            m_renderer.SetCartesian2dCanvas(this);
            Dock = DockStyle.Fill;
            m_selection.Changed += m_selection_SelectionChanged;
            m_readonlySelection = new ReadOnlyCollection<IControlPoint>(m_selection);
                                    
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
            
            List<CommandMenuItem> menuItemList = new List<CommandMenuItem>();
            menuItemList.AddRange(new CommandMenuItem[] {null,null,null,null,null,null });

            menuItemList[UndoMenuItemIndex] = new CommandMenuItem(m_contextMenu, CommandInfo.EditUndo, Undo);            
            menuItemList[RedoMenuItemIndex] = new CommandMenuItem(m_contextMenu, CommandInfo.EditRedo, Redo);            
            m_contextMenu.Items.Add(new ToolStripSeparator());
            menuItemList[CutMenuItemIndex] = new CommandMenuItem(m_contextMenu,CommandInfo.EditCut, Cut);            
            menuItemList[CopyMenuItemIndex] = new CommandMenuItem(m_contextMenu, CommandInfo.EditCopy, Copy);
            menuItemList[PasteMenuItemIndex] = new CommandMenuItem(m_contextMenu, CommandInfo.EditPaste, Paste, ShowPasteOptionsForm);            
            menuItemList[DeleteMenuItemIndex] = new CommandMenuItem(m_contextMenu, CommandInfo.EditDelete, Delete);
            m_commandItems = menuItemList.AsReadOnly();
            SkinService.ApplyActiveSkin(m_contextMenu);
           
        }
       
        /// <summary>
        /// Initialize static members</summary>
        static CurveCanvas()
        {
            s_genBrush = new SolidBrush(Color.Black);
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
        /// Register settings.</summary>
        /// <param name="settingsService"></param>
        internal void RegisterSettings(ISettingsService settingsService)
        {
            if (settingsService == null || m_isSettingRegistered)
                return;
            m_isSettingRegistered = true;

            // register settings.
             settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(
                        this, () => LockOrigin,
                        "Lock origin".Localize("This is the name of a command. Lock is a verb. Origin is like the origin of a graph."), null, null));
           
            settingsService.RegisterSettings(this,
                   new BoundPropertyDescriptor(
                       this, () => DrawMinorTickEnabled, "Show subdivision lines".Localize(), null, null));



            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.LocationMode, "LocationMode", null, null));

            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.Start, "Start", null, null));

            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.XOffset, "XOffset", null, null));

            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.YOffset, "YOffset", null, null));

            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.Copies, "Copies", null, null));

            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.PasteMethod, "PasteMethod", null, null));

            settingsService.RegisterSettings(m_pasteOptions,
                   new BoundPropertyDescriptor(
                       m_pasteOptions, () => m_pasteOptions.Connect, "Connect", null, null));
        }
        private bool m_isSettingRegistered;
        
        public Color CurrentXColor
        {
            get { return m_currentX.Color; }
            set
            {
                m_currentX.Color = value;
                Invalidate();
            }
        }

        public Color CurrentXHoverColor
        {
            get { return m_currentX.HoverColor; }
            set
            {
                m_currentX.HoverColor = value;
                Invalidate();
            }
        }
            
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
                var clipboard = new IControlPoint[m_selection.Count];
                m_transactionContext.DoTransaction(delegate
                {
                    int i = 0;
                    foreach (IControlPoint cp in m_selection)
                    {
                        ICurve curve = cp.Parent;
                        curve.RemoveControlPoint(cp);
                        clipboard[i++] = cp;
                    }
                    s_clipboard = clipboard.OrderBy(item => item.X).ToArray();
                    foreach (ICurve curve in m_selectedCurves)
                        CurveUtils.ComputeTangent(curve);
                }, m_commandItems[CutMenuItemIndex].MenuItem.Text);
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
                var clipboard = new IControlPoint[m_selection.Count];
                int i = 0;
                foreach (IControlPoint pt in m_selection)
                {
                    var cpt = pt.Clone();
                    if (cpt.Parent != null) cpt.Parent.RemoveControlPoint(cpt);
                    clipboard[i++] = cpt;
                }
                s_clipboard = clipboard.OrderBy(item => item.X).ToArray();
                Invalidate();
            }
        }


        /// <summary>
        /// Pastes into selected curves from internal clipboard</summary>
        public void Paste()
        {
            if (!CanPaste) return;
            var curves = GetPasteTargetCurves();
            
           
            // control points in s_clipboard are already sorted along x-axis.
            var sortedPoints = s_clipboard.Select(cp => cp.Clone()).ToArray();
            


            float origX = sortedPoints[0].X;
            float origY = sortedPoints[0].Y;
            // transform all the points to local space
            // where the first point starts at (0,0)
            if (m_pasteOptions.Connect)
            {
                sortedPoints.ForEach(item =>
                {
                    item.X -= origX;
                    item.Y -= origY;
                });
                
            }
            else
            {
                sortedPoints.ForEach(item =>
                {
                    item.X -= origX;
                });
            }


            if (m_pasteOptions.Copies > 1 && sortedPoints.Length > 1)
            {
                var lastPt = sortedPoints[sortedPoints.Length - 1];
                float xoffset = lastPt.X;
                float yoffset = lastPt.Y;
                     
                int ncopies = (int)m_pasteOptions.Copies;
                int length = ncopies * (sortedPoints.Length - 1) + 1;
                List<IControlPoint> pointlist = new List<IControlPoint>(length);
                pointlist.AddRange(sortedPoints);
                pointlist.RemoveAt(pointlist.Count - 1);
                for (int c = 1; c < (ncopies); c++)
                {
                    float cx = xoffset * c;
                    float cy = yoffset * c;
                    int ptCount =  c == ncopies-1 ? sortedPoints.Length : sortedPoints.Length-1;
                    for (int i = 0; i < ptCount; i++)
                    {
                        var clone = sortedPoints[i].Clone();
                        clone.X += cx;
                        if (m_pasteOptions.Connect) clone.Y += cy;
                        pointlist.Add(clone);
                    }
                }

                sortedPoints = pointlist.ToArray();

            }

            

            float gx0 = m_pasteOptions.GetPasteAtX();
            float gx1 = gx0 + sortedPoints[sortedPoints.Length - 1].X;
            m_transactionContext.DoTransaction(delegate
                {
                    var deleteList = new List<IControlPoint>();

                    foreach (var curve in curves)
                    {

                        var cv = CurveUtils.CreateCurveEvaluator(curve);
                        float gy0 = cv.Evaluate(gx0) + m_pasteOptions.YOffset;


                        // find insert index.
                        int pasteIndex = curve.ControlPoints.Count;
                        for (int i = 0; i < curve.ControlPoints.Count; i++)
                        {
                            var cpt = curve.ControlPoints[i];
                            if (gx0 <= cpt.X)
                            {
                                pasteIndex = i;
                                break;
                            }
                        }

                        if (m_pasteOptions.PasteMethod == PasteOptions.PasteMethods.Insert)
                        {
                            float xoffset = (gx1 - gx0) + CurveUtils.Epsilone;
                            for (int i = pasteIndex; i < curve.ControlPoints.Count; i++)
                                curve.ControlPoints[i].X += xoffset;
                            
                        }
                        else if (m_pasteOptions.PasteMethod == PasteOptions.PasteMethods.Replace)
                        {
                            foreach (var cpt in curve.ControlPoints)
                            {                                
                                if (cpt.X >= gx0 && cpt.X <= gx1)
                                    deleteList.Add(cpt);
                            }
                            deleteList.ForEach(p => curve.RemoveControlPoint(p));
                            deleteList.Clear();
                        }
                        else if (m_pasteOptions.PasteMethod != PasteOptions.PasteMethods.Merge)
                        {
                            throw new InvalidTransactionException("Paste method "+m_pasteOptions.PasteMethod + " is not supported");
                        }

                        // paste new control points
                        foreach (var cp in sortedPoints)
                        {
                            var newCpt = cp.Clone();
                            newCpt.X += gx0;
                            if (m_pasteOptions.Connect) newCpt.Y += gy0;
                            curve.InsertControlPoint(pasteIndex++, newCpt);
                        }

                        
                        // remove any point outside limit.
                        foreach (var cpt in curve.ControlPoints)
                        {
                            if (cpt.X < curve.MinX || cpt.X > curve.MaxX)
                                deleteList.Add(cpt);

                        }
                        deleteList.ForEach(p => curve.RemoveControlPoint(p));
                        deleteList.Clear();


                        CurveUtils.Sort(curve);

                        // if merging then accept new point when it overlaps with existing one.
                        if (m_pasteOptions.PasteMethod == PasteOptions.PasteMethods.Merge)
                        {

                            var pastePointSet = new HashSet<IControlPoint>(sortedPoints);
                            var overlapSet = new HashSet<IControlPoint>();
                            for (int i = 0; i < (curve.ControlPoints.Count - 1); i++)
                            {
                                var cpt1 = curve.ControlPoints[i];
                                var cpt2 = curve.ControlPoints[i + 1];
                                if (Math.Abs(cpt2.X - cpt1.X) < CurveUtils.Epsilone)
                                {
                                    overlapSet.Add(cpt1);
                                    overlapSet.Add(cpt2);
                                }
                            }
                            // remove any point that is overlapping and it is not new.
                            var todelete = overlapSet.Except(pastePointSet);
                            todelete.ForEach(p => curve.RemoveControlPoint(p));
                        }
                        
                        CurveUtils.ForceMinDistance(curve);
                        CurveUtils.ComputeTangent(curve);  
                    }
                },
                m_commandItems[PasteMenuItemIndex].MenuItem.Text);                        
            Invalidate();           
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
                }, m_commandItems[DeleteMenuItemIndex].MenuItem.Text);
                ClearSelection();
                UpdateCurveLimits();
                Invalidate();                               
            }
        }

        /// <summary>
        /// Applies CurveTangentTypes to all selected tangents for all selected control points</summary>   
        /// <param name="selectedTan">Selected tangents</param>
        /// <param name="tanType">CurveTangentTypes type to apply to selected tangents</param>
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
            if (m_editSet.Count == 0 && m_selectedCurves.Length == 0)
                return;
            IEnumerable<ICurve> curves = m_editSet.Count > 0 ? (IEnumerable<ICurve>)m_editSet : m_selectedCurves;
            m_transactionContext.DoTransaction(delegate
            {
                foreach (ICurve curve in curves)
                    curve.PreInfinity = infType;
            }, "Edit Pre-Infinity".Localize());
            Invalidate();
        }

        /// <summary>
        /// Sets post-infinity to the given CurveLoopTypes for the selected curves</summary>
        /// <param name="infType">CurveLoopTypes to be set</param>
        public void SetPostInfinity(CurveLoopTypes infType)
        {
            if (m_editSet.Count == 0 && m_selectedCurves.Length == 0)
                return;
            IEnumerable<ICurve> curves = m_editSet.Count > 0 ? (IEnumerable<ICurve>)m_editSet : m_selectedCurves;
            m_transactionContext.DoTransaction(delegate
            {
                foreach (ICurve curve in curves)
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

        private bool m_fitAllRequest;
        /// <summary>
        /// Frames all curves so that all curves fit in the window</summary>
        public void FitAll()
        {
            if (!Visible)
            {
               // delay the FitAll action until it is visible.
                m_fitAllRequest = true;
                return;                
            }
            
            if (m_curves.Count > 0)
            {
                m_fitAllRequest = false;

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

        #region public and internal properties

       
        /// <summary>
        /// Gets or sets the renderer responsible for drawing the curves and handling hit testing</summary>
        public CurveRenderer Renderer
        {
            get { return m_renderer; }            
        }


        /// <summary>
        /// Gets whether the control editing curve properties</summary>
        public bool Editing
        {
            get;
            private set;
        }
       
        /// <summary>
        /// Gets or sets whether only selected curves 
        /// can be edited.
        /// Setting it to true will limit the editable curves to ones that are specified
        /// see <see cref="EditableCurves"/> property</summary>
        public bool OnlyEditSelectedCurves
        {
            get;
            set;
        }

        /// <summary>
        /// Sets indices of the curves that can be edited
        /// <see cref="OnlyEditSelectedCurves"/> must be set to true
        /// for this feature to work </summary>
        public IEnumerable<int> EditableCurves
        {
            set
            {
                m_editSet.Clear();

                // if OnlyEditSelectedCurves is false, then
                // this property has no function.
                if (!OnlyEditSelectedCurves) return;
                
                if (value != null)
                    value.ForEach(index => m_editSet.Add(m_curves[index]));

                // remove curve from selection if is not in the editable set.
                if (m_editSet.Count > 0)
                {
                    foreach (var curve in m_curves)
                        if (!m_editSet.Contains(curve))
                            RemoveCurveFromSelection(curve);
                }
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether user can resize curve limits
        /// The default is false</summary>
        public bool AllowResizeCurveLimits
        {
            get;
            set;
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
                EditableCurves = EmptyEnumerable<int>.Instance;
                Invalidate();
            }
        }


        /// <summary>
        /// Gets whether can paste</summary>
        public bool CanPaste
        {
            get
            {
                return GetPasteTargetCurves().Any();
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
                m_curves = value ?? s_emptyCurves;
                ClearSelection();
                UpdateCurveLimits();
                if(Visible) PanToOrigin(false);
                CurvesChanged(this, EventArgs.Empty);
                EditableCurves = EmptyEnumerable<int>.Instance;
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
            //Keys kc = keyData & Keys.KeyCode;
            //bool CtrlPressed = (keyData & Keys.Control) == Keys.Control;

            var cmdItem = GetCommandItem(keyData);
            if (cmdItem != null)
            {
                cmdItem.ClickAction();
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
            m_currentX.HitTest(ClickPoint.X);
            m_originalValues = null;
            m_scalePivot = ClickGraphPoint;
            m_limitHit = null;
            m_zoomCenterStart = new PointD(
                  (ClickPoint.X - Pan_d.X) / Zoom_d.X,
                  (ClickPoint.Y - Pan_d.Y) / Zoom_d.Y);
            m_updateCurveLimits = false;            
            m_visibleCurveCount = m_curves.Count(c => c.Visible);

            // create list of curves for picking.            
            //m_pickableCurves = m_curves.Where(c => c.Visible
            //    && (m_visibleCurveCount == 1 || !OnlyEditSelectedCurves || m_editSet.Count == 0 || m_editSet.Contains(c))).Reverse();
            
            m_pickableCurves = m_curves.Where(c => c.Visible
                && (!OnlyEditSelectedCurves ||  m_editSet.Contains(c))).Reverse();

            if (m_autoSnapToX)
                m_scalePivot.X = CurveUtils.SnapTo(m_scalePivot.X, MajorTickX);
           
            if (m_autoSnapToY)
                m_scalePivot.Y = CurveUtils.SnapTo(m_scalePivot.Y, MajorTickY);


            if (m_currentX.IsPicked)
            {
                m_mouseDownAction = MouseDownAction.PasteAtMove;
                Cursor = m_cursors[CursorType.MoveHz];
            }
            else if (m_inputMode == InputModes.Basic)
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
                if (m_mouseDownAction == MouseDownAction.PasteAtMove)
                {
                    editAction = MouseEditAction.PasteAtMove;
                }
                else if (m_mouseDownAction == MouseDownAction.Pan)
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
                if (m_currentX.HitTest(CurrentPoint.X))
                {
                    this.Cursor = m_cursors[CursorType.MoveHz];                    
                }
                else if (PickCurveLimits(out m_limitSide) != null)
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
                
                // draw axis labels for last selected and visible curve.               
                for (int index = m_selection.Count - 1; index >= 0; index--)
                {
                    ICurve curve = m_selection[index].Parent;
                    if (curve != null && curve.Visible)
                    {
                        DrawXYLabel(e.Graphics, curve.XLabel, curve.YLabel, curve.CurveColor);
                        break;
                    }
                }

                foreach (ICurve curve in m_curves)
                {                
                    if (!curve.Visible)
                        continue;
                    float thickness = m_editSet.Contains(curve) ? 2.6f : 1.0f;
                    m_renderer.DrawCurve(curve, e.Graphics, thickness);
                }

                e.Graphics.SmoothingMode = SmoothingMode.None;
                // draw control points.
                foreach (ICurve curve in m_curves)
                {
                    if (!curve.Visible)
                        continue;
                    m_renderer.DrawControlPoints(curve, e.Graphics);
                }

                Pen pen = new Pen(Color.Black, 2);
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
                    var insertLinePen = new Pen(m_insertLineColor);
                    var insertLineBrush = new SolidBrush(m_insertLineColor);
                    
                    e.Graphics.DrawLine(insertLinePen, CurrentPoint.X, 0, CurrentPoint.X, Height);
                    IEnumerable<ICurve> curves = m_editSet.Count > 0 ? (IEnumerable<ICurve>)m_editSet : m_selectedCurves;
                    foreach (ICurve curve in curves)
                    {
                        ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
                                                
                        float y = cv.Evaluate(CurrentGraphPoint.X);
                        Vec2F gpt = new Vec2F(CurrentGraphPoint.X, y);
                        Vec2F pt = GraphToClient(gpt);
                        RectangleF ptRect = m_mouseRect;                                                    
                        ptRect.X = pt.X - ptRect.Width / 2;
                        ptRect.Y = pt.Y - ptRect.Height / 2;

                        bool inCurveLimit
                            = AutoComputeCurveLimitsEnabled
                            || (gpt.X >= curve.MinX
                            && gpt.X <= curve.MaxX
                            && gpt.Y >= curve.MinY
                            && gpt.Y <= curve.MaxY);

                        int index = CurveUtils.GetValidInsertionIndex(curve, CurrentGraphPoint.X);
                        if (index >= 0 && inCurveLimit)
                        {
                            e.Graphics.FillRectangle(insertLineBrush, ptRect);
                        }
                        else
                        {
                            pt.X = pt.X - s_noAction.Width / 2;
                            pt.Y = pt.Y - s_noAction.Height / 2;
                            e.Graphics.DrawImage(s_noAction, pt);
                        }
                    }

                    insertLinePen.Dispose();
                    insertLineBrush.Dispose();
                }

                pen.Dispose();
                m_currentX.Draw(e.Graphics);
                DrawPasteAtIndicators(e.Graphics);
            }

            //draw selection rectangle
            if (SelectionRect != RectangleF.Empty)
            {
                e.Graphics.DrawRectangle(s_marqueePen, Rectangle.Truncate(SelectionRect));
            }
            if (m_fitAllRequest) FitAll();
        }
        #endregion

        #region private helper methods

        private void DrawPasteAtIndicators(Graphics g)
        {
            var targeCurves = GetPasteTargetCurves();
            if (!targeCurves.Any()) return;

            RectangleF ptRect = new RectangleF(0, 0, 9, 9);
            s_genBrush.Color = Color.Gold;

            float gx = m_pasteOptions.GetPasteAtX();
            if (m_pasteOptions.Connect)
            {

                foreach (var curve in targeCurves)
                {
                    ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
                    float gy = cv.Evaluate(gx) + m_pasteOptions.YOffset;

                    Vec2F gpt = new Vec2F(gx, gy);
                    Vec2F pt = GraphToClient(gpt);
                    ptRect.X = pt.X - ptRect.Width / 2;
                    ptRect.Y = pt.Y - ptRect.Height / 2;
                    g.FillEllipse(s_genBrush, ptRect);
                }
            }
            else
            {
                float gy = s_clipboard[0].Y + m_pasteOptions.YOffset;
                Vec2F gpt = new Vec2F(gx, gy);
                Vec2F pt = GraphToClient(gpt);
                ptRect.X = pt.X - ptRect.Width / 2;
                ptRect.Y = pt.Y - ptRect.Height / 2;
                g.FillEllipse(s_genBrush, ptRect);
            }
        }

        private PasteOptionsForm m_pasteOptionsForm;
        private Rectangle m_pasteOptionsFormBounds;
       
        private void ShowPasteOptionsForm()
        {
            if (m_pasteOptionsForm != null && !m_pasteOptionsForm.IsDisposed)
            {
                m_pasteOptionsForm.Activate();
            }
            else
            {

                m_pasteOptionsForm = new PasteOptionsForm(this);

                m_pasteOptionsForm.SizeChanged += delegate
                {
                    m_pasteOptionsFormBounds = m_pasteOptionsForm.Bounds;
                };
                m_pasteOptionsForm.LocationChanged += delegate
                {
                    m_pasteOptionsFormBounds = m_pasteOptionsForm.Bounds;
                };

                m_pasteOptionsForm.FormClosed += delegate
                {                    
                    m_pasteOptionsForm = null;
                };               
                m_pasteOptionsForm.Disposed += delegate
                {
                    m_pasteOptionsForm = null;
                };

                if (m_pasteOptionsFormBounds != Rectangle.Empty)
                {
                    m_pasteOptionsForm.Bounds = m_pasteOptionsFormBounds;
                }
                else
                {
                    m_pasteOptionsForm.Location = PointToScreen(Point.Empty);
                }
                var parentForm = FindForm();
                
                m_pasteOptionsForm.Show(parentForm);
            }
        }
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
                if (m_editSet.Count > 0 || m_selection.Count > 0 || m_visibleCurveCount == 1)
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
                if ((m_editSet.Count > 0 || m_selection.Count > 0 || m_visibleCurveCount == 1) && EditMode == EditModes.InsertPoint)
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
                        m_renderer.PickPoints(m_pickableCurves, pickRect, points, regions, true);
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
                    if (m_editSet.Count > 0 || m_selection.Count > 0 || m_visibleCurveCount == 1)
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
            
            foreach (var cmdMenu in m_commandItems)
            {
                cmdMenu.UpdateMenuItems();
                cmdMenu.MenuItem.Enabled = hasSelection;                
            }

            m_commandItems[UndoMenuItemIndex].MenuItem.Enabled = m_historyContext.CanUndo;
            m_commandItems[UndoMenuItemIndex].MenuItem.Text = "Undo".Localize() + " " + m_historyContext.UndoDescription;
            m_commandItems[RedoMenuItemIndex].MenuItem.Enabled = m_historyContext.CanRedo;
            m_commandItems[RedoMenuItemIndex].MenuItem.Text = "Redo".Localize() + " " + m_historyContext.RedoDescription;
            m_commandItems[PasteMenuItemIndex].MenuItem.Enabled = CanPaste;
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
            List<IControlPoint> points = new List<IControlPoint>();
            List<PointSelectionRegions> regions = new List<PointSelectionRegions>();
            RectangleF pickRect = RectangleF.Empty;

            bool singlePick = !DraggingOverThreshold;
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
                case MouseEditAction.PasteAtMove:
                    {
                        m_currentX.Position = CurrentGraphPoint.X;
                    }
                    break;
                case MouseEditAction.Panning:
                    {
                        Pan_d = new PointD(ClickPan_d.X + dx, ClickPan_d.Y + dy);
                    }
                    break;
                case MouseEditAction.Zooming:
                    {                        
                        float xScale = 1 + 4.0f * Math.Abs(dx / Width);
                        if (dx < 0) xScale = 1.0f / xScale;

                        float yScale = 1 + 4.0f * Math.Abs(dy / Height);
                        if (dy > 0) yScale = 1.0f / yScale;

                        Zoom_d = new PointD(ClickZoom_d.X * xScale, ClickZoom_d.Y * yScale);
                        Pan_d = new PointD((ClickPoint.X - m_zoomCenterStart.X * Zoom_d.X),
                            (ClickPoint.Y - m_zoomCenterStart.Y * Zoom_d.Y));                       
                    }
                    break;
                case MouseEditAction.Select:
                    {
                        m_renderer.Pick(m_pickableCurves, pickRect, points, regions, singlePick);
                        SetSelection(points, regions);
                    }
                    break;
                case MouseEditAction.AddToSelection:
                    {
                        m_renderer.Pick(m_pickableCurves, pickRect, points, regions, singlePick);
                        AddToSelection(points, regions);
                    }
                    break;
                case MouseEditAction.RemoveFromSelection:
                    {
                        m_renderer.Pick(m_pickableCurves, pickRect, points, regions, singlePick);
                        RemoveFromSelection(points, regions);
                    }
                    break;
                case MouseEditAction.ToggleSelection:
                    {
                        m_renderer.Pick(m_pickableCurves, pickRect, points, regions, singlePick);
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

                case MouseEditAction.InsertPoint:
                case MouseEditAction.AddControlPoint:
                    {
                        m_updateCurveLimits = true;
                        
                        // collect curves 
                        IEnumerable<ICurve> curves = null;
                        if (m_visibleCurveCount == 1)
                        {
                            var curve = m_curves.First(c => c.Visible);
                            curves = new[]{curve};
                        }
                        else
                        {
                            curves = m_editSet.Count > 0 ? (IEnumerable<ICurve>)m_editSet : m_selectedCurves;
                        }

                        bool insert = action == MouseEditAction.InsertPoint;
                        var transName = insert ? "Insert Control Point".Localize() : "Add Control Point".Localize();
                        Vec2F pt = insert ? CurrentGraphPoint : ClickGraphPoint;
                        m_transactionContext.DoTransaction(delegate
                        {
                            curves.ForEach(curve => CurveUtils.AddControlPoint(curve, pt, insert));
                        }, transName);                                                   
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
                                    regions.Add(PointSelectionRegions.Point);
                                    points.Add(emptyCurve.ControlPoints[0]);
                                    AddToSelection(points, regions);
                                }, "Add Control Point".Localize());

                                
                            }
                        }
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
            if (AutoComputeCurveLimitsEnabled || !AllowResizeCurveLimits)
                return null;

            ICurve hit = null;
            const float pickTol = 4;

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
                            SnapToX(cpt, MajorTickX);
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

            m_selectedCurves = curves.Keys.ToArray();           
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

        private CommandMenuItem GetCommandItem(Keys shortcut)
        {
            var atfKey = KeysInterop.ToAtf(shortcut);
            foreach (var cmdItem in m_commandItems)
            {
                if (cmdItem.CmdInfo.IsShortcut(atfKey))
                    return cmdItem;
            }
            return null;
        }

        private IEnumerable<ICurve> GetPasteTargetCurves()
        {
            if (s_clipboard.Length > 0)
            {
                if (OnlyEditSelectedCurves)
                    return m_editSet.Where(c => c.Visible);
                return SelectedCurves.Where(c => c.Visible);
            }
            return EmptyArray<ICurve>.Instance;
        }

        #endregion
      
        #region private fields

        private HashSet<ICurve> m_editSet = new HashSet<ICurve>();
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
        private readonly int UndoMenuItemIndex = 0;
        private readonly int RedoMenuItemIndex = 1;
        private readonly int CutMenuItemIndex = 2;
        private readonly int CopyMenuItemIndex = 3;
        private readonly int PasteMenuItemIndex = 4;
        private readonly int DeleteMenuItemIndex = 5;
        private readonly ReadOnlyCollection<CommandMenuItem> m_commandItems;        
        private static IControlPoint[] s_clipboard = new IControlPoint[0];
        
        private int m_visibleCurveCount; // number of visible curves. Cmputed on mouse down.
        private IEnumerable<ICurve> m_pickableCurves; // List of curves that can be picked. Computed on mouse down.
            
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
        private CurveRenderer m_renderer;
        private readonly CurrentX m_currentX;
        private readonly PasteOptions m_pasteOptions;
        private const float MaxAngle = 1.5706217938697f; // = 89.99 degree
        private const float MinAngle = -1.5706217938697f; // =-89.99 degree        
        
        private static readonly Bitmap s_noAction;
        private static readonly Pen s_marqueePen;
        private static readonly SolidBrush s_genBrush;
        #endregion

        #region private and internal classes
        /// <summary>
        /// This is similar to current time in Maya's graph editor</summary>
        private class CurrentX
        {
            private bool m_setInitialPosition = true;
            public CurrentX(CurveCanvas canvas)
            {
                m_canvas = canvas;
                Color = Color.Yellow;
                HoverColor = Color.FromArgb(255, 255, 128);
            }

            public Color Color { get; set; }
            public Color HoverColor { get; set; }
          
            public bool IsPicked
            {
                get;
                set;
            }
            public bool HitTest(float cx)
            {
                bool oldIsPick = IsPicked;
                const float pickTolerance = 3; // pixels

                // perform hit testing in screen space.
                float cPos = m_canvas.GraphToClient(Position);
                IsPicked = Visible && Math.Abs(cPos - cx) < pickTolerance;
                if (oldIsPick != IsPicked) m_canvas.Invalidate();
                return IsPicked;
            }
            public bool Visible
            {
                get;
                private set;
            }
           
            public void Draw(Graphics g)
            {                
                var curves = m_canvas.GetPasteTargetCurves();
                Visible = curves.Any();
                if (!Visible) return;
              
                if (m_setInitialPosition)
                {// one time, set initial position.
                    m_setInitialPosition = false;
                    float xs = m_canvas.ClientRectangle.Width / 2;
                    Position = m_canvas.ClientToGraph(xs);
                }

                RectangleF crect = m_canvas.ClientRectangle;
                float cPos = m_canvas.GraphToClient(Position);
                float leftEdge = crect.X + 3;
                float rightEdge = crect.Right - 3;

                if (cPos < leftEdge || cPos > rightEdge)
                {
                    cPos = MathUtil.Clamp(cPos, leftEdge, rightEdge);
                    Position = m_canvas.ClientToGraph(cPos);
                }

                s_pen.Color = IsPicked ? HoverColor : Color;
                s_brush.Color = IsPicked ? HoverColor : Color;
                g.DrawLine(s_pen, cPos, crect.Y, cPos, crect.Bottom);
                            
                float strValY = m_canvas.Bottom - 2f * m_canvas.ScaleTextFont.Height - GridTextMargin - 8.0f;
                string strVal = Math.Round(Position, 4).ToString();
                g.DrawString(strVal, m_canvas.ScaleTextFont, s_brush, new PointF(cPos + 4, strValY));
            }
          
            static CurrentX()
            {
                s_pen = new Pen(Color.Yellow);
                s_pen.Width = 1.0f;
                s_brush = new SolidBrush(Color.Yellow);
            }

            /// <summary>
            /// Position of x-coordinate in graph space.</summary>
            public float Position;
            private static Pen s_pen;
            private static SolidBrush s_brush;
            private readonly CurveCanvas m_canvas;
        }

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

        private class CustomToolStripMenuItem : ToolStripMenuItem
        {
            public EventHandler ButtonClick = delegate { };
            public CustomToolStripMenuItem(){ }

            public CustomToolStripMenuItem(string text)
                : base(text, null, (EventHandler)null)
            {

            }

            protected override void OnClick(EventArgs e)
            {
                if (m_mouseIsOnButton)
                {
                    var handler = ButtonClick;
                    handler(this, EventArgs.Empty);
                    return;
                }                
                base.OnClick(e);
            }

            private const int ButtonMargin = 2;
            private Rectangle GetButtonRect()
            {
                
                var buttonRect = ContentRectangle;
                buttonRect.Height -= (2 * ButtonMargin + 1);
                buttonRect.Width = buttonRect.Height;
                buttonRect.Y += ButtonMargin;
                buttonRect.X = ContentRectangle.Width - buttonRect.Width - ButtonMargin;
                return buttonRect;
            }
            private Rectangle m_buttonRect;

            private Color GetBkgColor()
            {
                if (Owner == null) return BackColor;
                if (BackColor != Owner.BackColor) return BackColor;
                var proRenderer = Owner.Renderer as ToolStripProfessionalRenderer;
                if (proRenderer != null)
                    return proRenderer.ColorTable.ToolStripDropDownBackground;              
                return (!ToolStripManager.VisualStylesEnabled) ? BackColor : SystemColors.Menu; 
            }

            protected override void OnPaint(PaintEventArgs e)
            {                
                if (Owner == null)
                {
                    base.OnPaint(e);
                    return;
                }
                base.OnPaint(e);
                Color bkgColor = GetBkgColor();
                m_buttonRect = GetButtonRect();                               
                Color borderColor = Enabled ? ForeColor : Color.Gray;
               
                if (!m_mouseIsOnButton && Enabled && Selected)
                {                    
                    using (Brush b = new SolidBrush(bkgColor))
                    {
                        var fillRect = ContentRectangle;
                        fillRect.X = m_buttonRect.X - ButtonMargin;
                        fillRect.Width = ContentRectangle.Right - fillRect.X + 1;
                        fillRect.Y -= 1;
                        fillRect.Height += 2;

                        e.Graphics.FillRectangle(b, fillRect);                       
                    }
                }

                using (Pen p = new Pen(borderColor))
                {
                    e.Graphics.DrawRectangle(p, m_buttonRect);
                    var dotRect = m_buttonRect;
                    dotRect.Size = new Size(1, 1);
                    dotRect.Y = m_buttonRect.Bottom - 2;
                    dotRect.X += (m_buttonRect.Width - 5) / 2;
                    Brush b = new SolidBrush(borderColor);

                    e.Graphics.FillRectangle(b, dotRect);
                    dotRect.X += 2;
                    e.Graphics.FillRectangle(b, dotRect);
                    dotRect.X += 2;
                    e.Graphics.FillRectangle(b, dotRect);
                }
               
            }

            private bool m_mouseIsOnButton;
            protected override void OnMouseMove(MouseEventArgs mea)
            {
                base.OnMouseMove(mea);                
                m_mouseIsOnButton = Enabled && mea.X > m_buttonRect.X;
                Invalidate();
            }
            
            protected override void OnMouseLeave(EventArgs e)
            {
                m_mouseIsOnButton = false;
                base.OnMouseLeave(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {                
                Invalidate();
                base.OnMouseDown(e);
            }
        }

        // Pairs CommandInfo with tool strip menu item.
        private class CommandMenuItem
        {
            public CommandMenuItem(ContextMenuStrip contextMenu, CommandInfo cmd, Action click, Action buttonClick = null)
            {
                ClickAction = click;
                CmdInfo = cmd;

                if (buttonClick != null)
                {
                    var customItem = new CustomToolStripMenuItem(cmd.MenuText);
                    customItem.ButtonClick += (s,e) => buttonClick();
                    MenuItem = customItem;
                }
                else
                {                                       
                    MenuItem = new ToolStripMenuItem(cmd.MenuText);
                }
                                
                MenuItem.Click += (s, e) => ClickAction();
                contextMenu.Items.Add(MenuItem);
            }

            public void UpdateMenuItems()
            {
                MenuItem.Text = CmdInfo.MenuText;
                MenuItem.ShortcutKeyDisplayString = CmdInfo.ShortcutKeyDisplayString;
            }
            public readonly Action ClickAction;
            public readonly CommandInfo CmdInfo;
            public readonly ToolStripMenuItem MenuItem;
        }

        /// <summary>
        /// Default shortcut keys</summary>
        internal static class ShortcutKeys
        {
            /// <summary>Undo</summary>            
            public static Keys Undo = Keys.Control | Keys.Z;

            /// <summary>Redo</summary>            
            public static Keys Redo = Keys.Control | Keys.Y;

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

        /// <summary>
        /// Holds all the paste related options</summary>
        private class PasteOptions
        {
            private CurveCanvas m_canvas;
            public PasteOptions(CurveCanvas canvas)
            {
                m_canvas = canvas;
                LocationMode = XLocationMode.Current;
                Connect = true;
                Copies = 1;
                PasteMethod = PasteMethods.Replace;
            }


            /// <summary>
            /// Computes X position of paste target</summary>
            public float GetPasteAtX()
            {
                float posX = XOffset;
                if (LocationMode == XLocationMode.Current)
                    posX += m_canvas.m_currentX.Position;
                else if (LocationMode == XLocationMode.Start)
                    posX += Start;
                else if (LocationMode == XLocationMode.Clipboard)
                    posX += CurveCanvas.s_clipboard.Length > 0 ? CurveCanvas.s_clipboard[0].X : 0;

                return posX;                
            }

            /// <summary>
            /// Gets or sets whether to paste keys at target Y 
            /// or at the Y of the first point of the pasted curve
            /// segment.
            /// if true it will paste at the Y coordinate of the first
            /// point of the pasted curve segment.
            /// </summary>
            [DefaultValue(true)]
            public bool Connect
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the starting value of paste operation.
            /// <note> this property is used only when
            /// the value of LocationMode property is XLocationMode.Start</note>
            /// </summary>
            public float Start
            {
                get;
                set;
            }

            

            /// <summary>
            /// Gets or sets X offset</summary>
            public float XOffset
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets Y offset.</summary>
            public float YOffset
            {
                get;
                set;
            }


            private uint m_numCopies;
            /// <summary>
            /// Gets and sets number of time the paste operation is
            /// performed.             
            /// </summary>
            public uint Copies
            {
                get { return m_numCopies; }
                set { m_numCopies = MathUtil.Clamp<uint>(value, 1, 100); }
            }

            /// <summary>
            /// Gets or sets the mode that controls how the location of paste is computed</summary>
            public XLocationMode LocationMode
            {
                get;
                set;
            }


            /// <summary>
            /// Gets or sets the value that controls paste method.</summary>
            public PasteMethods PasteMethod
            {
                get;
                set;
            }

            /// <summary>
            /// Paste methods</summary>
            public enum PasteMethods
            {
                /// <summary>
                /// Insert, shift all the keys to right
                /// and insert the new keys starting at the specified location.                
                /// </summary>
                Insert,

                /// <summary>
                /// Paste at the specified locaiton and replace any key that
                /// overlapse with the keys.</summary>
                Replace,

                /// <summary>
                /// Paste at the specified location with out removing or modifying existing keys.
                /// But if the new and existing keys have same value then new exiting key will be 
                /// replace with the new key.
                /// </summary>
                Merge,
            }

            /// <summary>
            /// paste location modes.</summary>
            public enum XLocationMode
            {
                /// <summary>
                /// Paste starting at the current location indicated 
                /// by current x line</summary>
                Current,

                /// <summary>
                /// Paste starting from the value of Start property</summary>
                Start,

                /// <summary>
                /// Paste at the original value.</summary>
                Clipboard
            }
        }
        private class PasteOptionsForm : Form
        {
            // paste at controls.                        
            private GroupBox m_pasteAtgrp;
            private Label m_xlocLbl;            
            private RadioButton m_startRdo;
            private RadioButton m_currentRdo;
            private RadioButton m_clipboardRdo;
            private InputField<float> m_startField;
            private InputField<float> m_xOffsetField;
            private InputField<float> m_yOffsetField;
            private InputField<uint>  m_copiesField;


            private GroupBox m_pasteMethodGrp;
            private Label m_pasteMethodLbl;
            private RadioButton m_insertRdo;
            private RadioButton m_replaceRdo;
            private RadioButton m_mergeRdo;
            private CheckBox m_connectChk;

            // buttons
            private Control m_buttonsPanel;
            private Button m_okBtn;
            private Button m_closeBtn;
            private Button m_applyBtn;


            private CurveCanvas m_canvas;
            
            public PasteOptionsForm(CurveCanvas canvas)
            {
                m_canvas = canvas;
                
                // paste at group.
                m_pasteAtgrp = new GroupBox();
                
                m_xlocLbl = new Label();
                m_xlocLbl.AutoSize = true;
                m_xlocLbl.Text = "X Location".Localize();
                
                // radio buttons.
                m_startRdo = new RadioButton();
                m_startRdo.AutoSize = true;
                m_startRdo.Text = "Start".Localize();
                m_startRdo.Checked = true;
               
                
                m_currentRdo = new RadioButton();
                m_currentRdo.AutoSize = true;
                m_currentRdo.Text = "Current".Localize();
                
                m_clipboardRdo = new RadioButton();
                m_clipboardRdo.Text = "Clipboard".Localize();
                m_clipboardRdo.AutoSize = true;
                                
                // input fields.
                m_startField = new InputField<float>("Start".Localize(), 150);                
                m_xOffsetField = new InputField<float>("X Offset".Localize(), 150);                
                m_yOffsetField = new InputField<float>("Y Offset".Localize(), 150);                
                m_copiesField = new InputField<uint>("Copies".Localize(), 100);
                
                m_pasteAtgrp.Controls.AddRange(new Control[] {
                    m_xlocLbl,
                    m_startRdo,
                    m_currentRdo,
                    m_clipboardRdo,
                    m_startField,
                    m_xOffsetField,
                    m_yOffsetField,
                    m_copiesField,
                });

                
                // paste method group.
                m_pasteMethodGrp = new GroupBox();
                
                m_pasteMethodLbl = new Label();
                m_pasteMethodLbl.Text = "Paste Method".Localize();
                m_pasteMethodLbl.AutoSize = true;
                
                m_insertRdo = new RadioButton();
                m_insertRdo.Text = "Insert".Localize();
                m_insertRdo.AutoSize = true;
                m_insertRdo.Checked = true;

                m_replaceRdo = new RadioButton();
                m_replaceRdo.Text = "Replace".Localize();
                m_replaceRdo.AutoSize = true;
                

                m_mergeRdo = new RadioButton();
                m_mergeRdo.Text = "Merge".Localize();
                m_mergeRdo.AutoSize = true;
                

                m_connectChk = new CheckBox();
                m_connectChk.Text = "Connect".Localize();
                m_connectChk.AutoSize = true;
                
                m_pasteMethodGrp.Controls.AddRange(new Control[]
                {
                    m_pasteMethodLbl,
                    m_insertRdo,
                    m_replaceRdo,
                    m_mergeRdo,
                    m_connectChk
                });


                // buttons panel.
                m_buttonsPanel = new Panel();
                
                m_okBtn = new Button
                {
                    Text = "Ok".Localize(),                                        
                };
                m_okBtn.Click += (s, e) => ApplyAndClose();
              
                
                m_applyBtn = new Button
                {
                    Text = "Apply".Localize(),
                    
                };
                m_applyBtn.Click += (s, e) => Apply();


                m_closeBtn = new Button
                {
                    Text = "Close".Localize(),                    
                };
                m_closeBtn.Click += (s, e) => Close();

                m_buttonsPanel.Controls.AddRange(new[]
                    {
                        m_okBtn,                        
                        m_applyBtn,
                        m_closeBtn,
                    });
                                
                SuspendLayout();

                Controls.AddRange(new[]
                {
                    m_buttonsPanel,
                    m_pasteMethodGrp,
                    m_pasteAtgrp,                                       
                });
                                
                AutoScaleMode = AutoScaleMode.None;                                              
                FormBorderStyle = FormBorderStyle.SizableToolWindow;                
                Name = "PasteOptionsForm";
                ShowIcon = false;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                Text = "Curve Editor - Paste options".Localize();
                
                SizeChanged += (s, e) => DoLayout();
                m_startField.SizeChanged += (s, e) => DoLayout();
                ClientSize = new Size(530, 410);
                ResumeLayout(false);
                PerformLayout();

                // set tab orders
                m_pasteAtgrp.TabStop = false;
                m_pasteAtgrp.TabIndex = 0;

                m_startRdo.TabStop = true;
                m_startRdo.TabIndex = 0;
                

                m_currentRdo.TabStop = true;
                m_currentRdo.TabIndex = 1;

                m_clipboardRdo.TabStop = true;
                m_clipboardRdo.TabIndex = 2;

                m_startField.TabIndex = 3;
                m_xOffsetField.TabIndex = 4;
                m_yOffsetField.TabIndex = 5;
                m_copiesField.TabIndex = 6;


                m_pasteMethodGrp.TabStop = false;
                m_pasteMethodGrp.TabIndex = 1;

                m_insertRdo.TabStop = true;
                m_insertRdo.TabIndex = 0;

                m_replaceRdo.TabStop = true;
                m_replaceRdo.TabIndex = 1;
                    
                m_mergeRdo.TabStop = true;
                m_mergeRdo.TabIndex = 2;

                m_connectChk.TabStop = true;
                m_connectChk.TabIndex = 3;

                m_buttonsPanel.TabStop = false;
                m_buttonsPanel.TabIndex = 2;
                m_okBtn.TabIndex = 0;
                m_applyBtn.TabIndex = 1;
                m_closeBtn.TabIndex = 2;

                Shown += (s, e) =>
                    {
                        DoLayout();
                        ModelToView();
                    };
                    
                
                Deactivate += (s, e) => ViewToModel();
                
                
                m_startRdo.CheckedChanged += RdoCheckedChanged;
                m_currentRdo.CheckedChanged += RdoCheckedChanged;
                m_clipboardRdo.CheckedChanged += RdoCheckedChanged;
                m_insertRdo.CheckedChanged += RdoCheckedChanged;
                m_replaceRdo.CheckedChanged += RdoCheckedChanged;
                m_mergeRdo.CheckedChanged += RdoCheckedChanged;

                m_startField.ValueChanged += (s, e) => ViewToModel();
                m_xOffsetField.ValueChanged += (s, e) => ViewToModel();
                m_yOffsetField.ValueChanged += (s, e) => ViewToModel();
                m_copiesField.ValueChanged += (s, e) => ViewToModel();
                m_connectChk.CheckedChanged += (s, e) => ViewToModel();
            }

            
                       
            private void ApplyAndClose()
            {
                Apply();
                Close();
            }
            private void Apply()
            {
                ViewToModel();
                m_canvas.Paste();                
            }

            private bool m_updatingView;
            private void ModelToView()
            {
                try
                {
                    m_updatingView = true;
                    var po = m_canvas.m_pasteOptions;
                    m_startRdo.Checked = po.LocationMode == PasteOptions.XLocationMode.Start;
                    m_currentRdo.Checked = po.LocationMode == PasteOptions.XLocationMode.Current;
                    m_clipboardRdo.Checked = po.LocationMode == PasteOptions.XLocationMode.Clipboard;

                    m_startField.Value = po.Start;
                    m_startField.Enabled = m_startRdo.Checked;

                    m_xOffsetField.Value = po.XOffset;
                    m_yOffsetField.Value = po.YOffset;

                    m_copiesField.Value = po.Copies;

                    m_insertRdo.Checked = po.PasteMethod == PasteOptions.PasteMethods.Insert;
                    m_replaceRdo.Checked = po.PasteMethod == PasteOptions.PasteMethods.Replace;
                    m_mergeRdo.Checked = po.PasteMethod == PasteOptions.PasteMethods.Merge;
                    m_connectChk.Checked = po.Connect;
                }
                finally
                {
                    m_updatingView = false;
                }
            }

            private void RdoCheckedChanged(object sender, EventArgs e)
            {
                var rdo = sender as RadioButton;
                rdo.TabStop = true;

                if (m_updatingView) return;                
                if(rdo == m_startRdo) m_startField.Enabled = m_startRdo.Checked;
                if (rdo.Checked) ViewToModel();                    
            }

            private void ViewToModel()
            {
                if (m_updatingView) return;

                var po = m_canvas.m_pasteOptions;
                if (m_startRdo.Checked) po.LocationMode = PasteOptions.XLocationMode.Start;
                else if (m_currentRdo.Checked) po.LocationMode = PasteOptions.XLocationMode.Current;                
                else if (m_clipboardRdo.Checked) po.LocationMode = PasteOptions.XLocationMode.Clipboard;

                po.Start = m_startField.Value;
                po.XOffset = m_xOffsetField.Value;
                po.YOffset = m_yOffsetField.Value;
                po.Copies = m_copiesField.Value;

                if(m_insertRdo.Checked)  po.PasteMethod = PasteOptions.PasteMethods.Insert;
                else if (m_replaceRdo.Checked) po.PasteMethod = PasteOptions.PasteMethods.Replace;
                else if (m_mergeRdo.Checked )  po.PasteMethod = PasteOptions.PasteMethods.Merge;

                po.Connect = m_connectChk.Checked;

                if (po.Copies != m_copiesField.Value)
                    m_copiesField.Value = po.Copies;
                m_canvas.Invalidate();

            }

            private void DoLayout()
            {                
                int outerMargin = 4;
                int grpLeftRightMargin = 16;
                int leftPadding = 30;
                int vSpacing = 8; // vertical spacing between fields.
                int vSpacingGrp = 16;
                int hSpacing = 4; // horizontal spacing between fields.

                int pad = 16;
                int cw = ClientSize.Width - 2 * grpLeftRightMargin;
                m_pasteAtgrp.Location = new Point(grpLeftRightMargin, outerMargin);
                m_pasteAtgrp.Height = m_currentRdo.Height + 2 * pad + 3 * vSpacingGrp + vSpacing + 4 * m_startField.Height;

                int txtLeft = m_xlocLbl.Width + 6;
                int rdoX = leftPadding + txtLeft;

                m_xlocLbl.Location = new Point(leftPadding, pad + (m_startRdo.Height - m_xlocLbl.Height) / 2);

                m_startRdo.Location = new Point(rdoX, pad);
                
                rdoX += (m_startRdo.Width + hSpacing);
                m_currentRdo.Location = new Point(rdoX, pad);

                rdoX += (m_currentRdo.Width + hSpacing);
                m_clipboardRdo.Location = new Point(rdoX, pad);

                int fieldY = m_clipboardRdo.Bottom + vSpacingGrp;
                

                m_startField.Location = new Point(leftPadding + (txtLeft - m_startField.TextBoxLocation.X), fieldY);
                fieldY += (m_startField.Height + vSpacingGrp);

                m_xOffsetField.Location = new Point(leftPadding + (txtLeft- m_xOffsetField.TextBoxLocation.X),fieldY);
                fieldY += (m_startField.Height + vSpacing);

                m_yOffsetField.Location = new Point(leftPadding + (txtLeft - m_yOffsetField.TextBoxLocation.X), fieldY);
                fieldY += (m_startField.Height + vSpacingGrp);
                
                m_copiesField.Location = new Point(leftPadding + (txtLeft - m_copiesField.TextBoxLocation.X), fieldY);

                m_pasteAtgrp.Width = Math.Max(m_clipboardRdo.Right, cw); ;

                m_pasteMethodGrp.Width = m_pasteAtgrp.Width;
                m_pasteMethodGrp.Location = new Point(grpLeftRightMargin, m_pasteAtgrp.Bottom + vSpacingGrp);
                m_pasteMethodGrp.Height = 2 * pad + m_insertRdo.Height + vSpacing + m_connectChk.Height;

                m_pasteMethodLbl.Location = new Point(leftPadding, pad);
                m_insertRdo.Location = new Point(m_pasteMethodLbl.Right + hSpacing, pad);
                m_replaceRdo.Location = new Point(m_insertRdo.Right + hSpacing, pad);
                m_mergeRdo.Location = new Point(m_replaceRdo.Right + hSpacing, pad);
                m_connectChk.Location = new Point(m_insertRdo.Left, m_mergeRdo.Bottom + vSpacing);


                m_buttonsPanel.Height = m_applyBtn.Height + vSpacing + 1;
                m_buttonsPanel.Width = cw;               
                m_buttonsPanel.Location = new Point(grpLeftRightMargin, ClientSize.Height - m_buttonsPanel.Height);

                int closeBtnW = TextRenderer.MeasureText(m_closeBtn.Text, m_closeBtn.Font).Width;
                int applyBtnW = TextRenderer.MeasureText(m_applyBtn.Text, m_applyBtn.Font).Width;
                int okBtnW = TextRenderer.MeasureText(m_okBtn.Text, m_okBtn.Font).Width;
                int btnW = Math.Max(closeBtnW, applyBtnW);
                btnW = Math.Max(btnW, okBtnW) * 2;
                
                m_closeBtn.Width = btnW;
                m_applyBtn.Width = btnW;
                m_okBtn.Width = btnW;
                              
                m_closeBtn.Location = new Point(m_buttonsPanel.ClientSize.Width - m_closeBtn.Width , 1);
                m_applyBtn.Location = new Point(m_closeBtn.Left - m_closeBtn.Width - hSpacing, 1);
                m_okBtn.Location = new Point(m_applyBtn.Left - m_okBtn.Width - hSpacing, 1);
            }

            private class InputField<T> : Control where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
            {

                public event EventHandler ValueChanged = delegate { };


                private readonly NumberTextBox<T> m_txtBox;
                private readonly Label m_label;
                private int m_txtWidth;
                
                public InputField(string label, int length)
                {
                    m_txtWidth = length;
                    if (m_txtWidth < 20) m_txtWidth = 20;

                    
                    m_label = new Label();
                    m_label.AutoSize = true;
                    m_label.Location = new Point(1, 1);
                    m_label.Name = "m_label";                  
                    m_label.Text = label;


                    m_txtBox = new NumberTextBox<T>();
                    m_txtBox.Location = new Point(26, 1);
                    m_txtBox.Name = "m_txtBox";
                    m_txtBox.Size = new Size(m_txtWidth, 30);
                    m_txtBox.TabIndex = 0;
                    m_txtBox.TabStop = true;

                    m_txtBox.ValueChanged += (s, e) => m_dirty = true;
                    m_txtBox.Validated += (s, e) => RaiseValueChanged();
                    m_txtBox.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.Enter)
                        {
                            RaiseValueChanged();
                        }
                    };

                                        
                    Controls.Add(m_label);
                    Controls.Add(m_txtBox);

                    m_txtBox.SizeChanged += (ts, te) => DoLayout();
                    m_label.SizeChanged += (ls, le) => DoLayout();
                    SizeChanged += (s, e) => DoLayout();
                    Size = new Size(200, 30);
                    TabStop = false;
                }

                private bool m_dirty;
                private void RaiseValueChanged()
                {
                    if (m_dirty)
                    {
                        m_dirty = false;
                        var h = ValueChanged;
                        h(this, EventArgs.Empty);
                    }
                }
            
                //private PropertyInfo m_boundProperty;
                //private object m_source;
                //public void SetBinding(object source, Expression<Func<T>> expr)
                //{
                //    m_boundProperty = null;
                //    m_source = source;                    
                //    if (source == null) return;                                     
                //    var memexpr = expr.Body as MemberExpression;
                //    PropertyInfo propInfo = memexpr != null ? memexpr.Member as PropertyInfo : null;
                //    if (propInfo == null) throw new ArgumentException("property not found");
                //    if (propInfo.CanRead && propInfo.CanWrite) m_boundProperty = propInfo;
                //}


                public Point TextBoxLocation
                {
                    get { return m_txtBox.Location; }
                }

                public T Value
                {
                    get { return m_txtBox.Value; }
                    set { m_txtBox.Value = value; }
                }
               
                private void DoLayout()
                {
                    Height = Math.Max(m_label.Height, m_txtBox.Height);
                    Width = m_label.Width + m_txtBox.Width + 3;                                                               
                    m_label.Location = new Point(1, (Height - m_label.Height) / 2);
                    m_txtBox.Location = new Point(m_label.Bounds.Right, (Height - m_txtBox.Height) / 2);
                }
            }
            /// <summary>
            /// Numeric text box</summary>    
            class NumberTextBox<T> : TextBox where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
            {
                /// <summary>
                /// this event is raised when Value changed. </summary>
                public event EventHandler ValueChanged = delegate { };

                /// <summary>
                /// Constructs and verifies that T is one of the numeric data type.</summary>
                public NumberTextBox()
                {
                    ttype = typeof(T);
                    if (ttype == typeof(sbyte) || ttype == typeof(short)
                        || ttype == typeof(int) || ttype == typeof(long))
                    {
                        IsInteger = true;
                        IsSigned = true;
                    }

                    else if (ttype == typeof(byte) || ttype == typeof(ushort)
                        || ttype == typeof(uint) || ttype == typeof(ulong))
                    {
                        IsInteger = true;
                        IsSigned = false;
                    }
                    else if (ttype == typeof(float) || ttype == typeof(double))
                    {
                        IsInteger = false;
                        IsSigned = true;
                    }
                    else
                    {
                        throw new Exception(ttype.FullName + " is not supported"); ;
                    }

                    Value = default(T);

                    TextChanged += NumberTextBox_TextChanged;
                    KeyPress += NumberTextBox_KeyPress;
                    LostFocus += NumberTextBox_LostFocus;
                }

                private void NumberTextBox_LostFocus(object sender, EventArgs e)
                {
                    T val;
                    string str = Text;
                    if (!TryParseT(str, out val))
                    {
                        Value = default(T);
                    }

                }

                private void NumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
                {
                    // drop invalid key presses.
                    if (!char.IsControl(e.KeyChar))
                    {
                        T val;
                        string str = Text.Insert(SelectionStart, e.KeyChar.ToString());
                        if (!IsSigned || str != "-")
                            e.Handled = !TryParseT(str, out val);
                    }
                }

                private void NumberTextBox_TextChanged(object sender, EventArgs e)
                {
                    string str = Text;
                    if (IsSigned && str == "-") return;
                    if (!IsInteger && str == ".") return;

                    T val = default(T);

                    if (!string.IsNullOrWhiteSpace(str) && !TryParseT(str, out val))
                    {
                        int pos = SelectionStart;
                        Text = ValueToString(m_val);
                        SelectionStart = pos > 0 ? pos - 1 : pos;
                    }
                    else if (!((IEquatable<T>)val).Equals(m_val))
                    {
                        m_val = val;
                        OnValueChanged(EventArgs.Empty);
                    }
                }

                /// <summary>
                /// Gets and sets the value of this numeric text box.</summary>
                public T Value
                {
                    get { return m_val; }
                    set
                    {
                        string strval = ValueToString(value);
                        if (Text != strval) Text = strval;
                        if (!((IEquatable<T>)m_val).Equals(value))
                        {
                            m_val = value;
                            OnValueChanged(EventArgs.Empty);
                        }
                    }
                }

                protected virtual void OnValueChanged(EventArgs e)
                {
                    var h = ValueChanged;
                    h(this, EventArgs.Empty);
                }

                #region mouse handling
                private bool m_selectAll;
                
                protected override void OnEnter(EventArgs e)
                {
                    base.OnEnter(e);
                    m_selectAll = MouseButtons == MouseButtons.Left;
                    if (MouseButtons == MouseButtons.None)
                    {// select all, when tabbing into this control.
                        SelectAll();
                    }
                }
                
                protected override void OnLeave(EventArgs e)
                {
                    base.OnLeave(e);
                    m_selectAll = false;
                }
                
                protected override void OnMouseUp(MouseEventArgs mevent)
                {
                    base.OnMouseUp(mevent);
                    if (m_selectAll && SelectionLength == 0)
                    {
                        m_selectAll = false;
                        SelectAll();
                        Focus();
                    }

                }                
                protected override void OnMouseDown(MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Left &&
                        e.Clicks == 2)
                    {
                        SelectAll();
                        return;
                    }
                    base.OnMouseDown(e);
                }

                #endregion

                #region private methods.

                private bool TryParseT(string s, out T value)
                {
                    bool valid;

                    // var converter = TypeDescriptor.GetConverter(ttype);
                    // val = (T)converter.ConvertFrom(s);

                    if (ttype == typeof(sbyte))
                    {
                        sbyte val;
                        valid = sbyte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(byte))
                    {
                        byte val;
                        valid = byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(short))
                    {
                        short val;
                        valid = short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(ushort))
                    {
                        ushort val;
                        valid = ushort.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(int))
                    {
                        int val;
                        valid = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(uint))
                    {
                        uint val;
                        valid = uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(long))
                    {
                        long val;
                        valid = long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(ulong))
                    {
                        ulong val;
                        valid = ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(float))
                    {
                        float val;
                        valid = float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else if (ttype == typeof(double))
                    {
                        double val;
                        valid = double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
                        value = (T)(object)val;
                    }
                    else
                    {
                        value = default(T);
                        valid = false;
                    }
                    return valid;
                }

                private string ValueToString(T val)
                {
                    if (IsInteger) return ((IFormattable)val).ToString(null, CultureInfo.InvariantCulture);
                    return ((IFormattable)val).ToString("R", CultureInfo.InvariantCulture);
                }
                #endregion
                private T m_val;
                private readonly Type ttype;
                private readonly bool IsInteger;
                private readonly bool IsSigned;
            }

        }
        #endregion

        #region  private enums
        
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

            /// <summary> Move paste at line</summary>
            PasteAtMove,
        }

        /// <summary>
        /// Curve limit sides</summary>
        [Flags]
        private enum CurveLimitSides
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
            CurveLimitResize,            
            PasteAtMove
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
        /// Advanced mode, similar to Maya graph editor</summary>
        Advanced,
    }    
}
