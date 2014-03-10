//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Container control for CurveCanvas Control</summary>
    public class CurveEditingControl : Control
    {
        /// <summary>
        /// Default constructor</summary>
        public CurveEditingControl()
        {
            Init(new CurveCanvas());
        }

        /// <summary>
        /// Constructor with CurveCanvas</summary>
        public CurveEditingControl(CurveCanvas curveCanvas)
        {
            Init(curveCanvas);
        }

        #region public events

        public event EventHandler CurvesChanged
        {
            add { m_curveControl.CurvesChanged += value; }
            remove { m_curveControl.CurvesChanged -= value; }
        }

        #endregion

        
        #region public properties and methods

        /// <summary>
        /// Gets or sets whether to auto compute curve limits
        /// The default is true.</summary>
        public bool AutoComputeCurveLimitsEnabled
        {
            get { return m_curveControl.AutoComputeCurveLimitsEnabled; }
            set { m_curveControl.AutoComputeCurveLimitsEnabled = value; }
        }

        /// <summary>
        /// Gets or sets enabling restricted control-point translation.
        /// If enabled, the user can only translate a control point between its 
        /// previous and next control points.</summary>
        public bool RestrictedTranslationEnabled
        {
            get { return m_curveControl.RestrictedTranslationEnabled; }
            set { m_curveControl.RestrictedTranslationEnabled = value; }
        }

        /// <summary>
        /// Sets editing context</summary>
        public object Context
        {
            set { m_curveControl.Context = value; }
        }
       
        /// <summary>
        /// Sets curves</summary>
        public virtual ReadOnlyCollection<ICurve> Curves
        {
            set
            {                
                SetUI(value != null);
                m_curveControl.Curves = value;
                PopulateListView(value);                
                UpdateCurveTypeSelector();
            }
        }

        
        /// <summary>
        /// Frames all the curves</summary>
        public void FitAll()
        {
            m_curveControl.FitAll();
        }

        /// <summary>
        /// Gets or sets origin lock mode</summary>
        public OriginLockMode LockOrigin
        {
            get { return m_curveControl.LockOrigin; }
            set { m_curveControl.LockOrigin = value; }
        }

        /// <summary>
        /// Gets or sets input mode</summary>
        public InputModes InputMode
        {
            get
            {
                return m_curveControl.InputMode;
            }
            set
            {
                m_curveControl.InputMode = value;
                if (value == InputModes.Basic)
                {
                    foreach (ToolStripButton btn in m_editModeButtons)
                    {
                        btn.Visible = false;
                    }
                }
                else if (value == InputModes.Advanced)
                {
                    foreach (ToolStripButton btn in m_editModeButtons)
                    {
                        btn.Visible = true;
                    }
                }
                else
                    throw new ArgumentOutOfRangeException();

                m_advancedInputMenuItem.Checked = value == InputModes.Advanced;
                m_basicMenuItem.Checked = value == InputModes.Basic;
            }
        }

        /// <summary>
        /// Refreshes control</summary>
        public override void Refresh()
        {
            if (CurveCanvas.Editing) return;
            base.Refresh();
            UpdateCurveTypeSelector();            
        }
        
        #endregion

        /// <summary>
        /// Gets or sets whether to show or hide tangent editing related menu and toolbar items</summary>
        protected bool ShowTangentEditing
        {
            get { return m_showTangentEditing; }
            set
            {
                m_showTangentEditing = value;
                foreach (ToolStripButton btn in m_tangentBtns)
                    btn.Visible = value;
                m_breakTangent.Visible = value;
                m_unifyTangent.Visible = value;
                m_tangentsMenuItem.Visible = value;
                m_tanSeparator1.Visible = value;
                m_tanSeparator2.Visible = value;


            }
        }

        /// <summary>
        /// Gets or sets flip y</summary>
        public bool FlipY
        {
            get { return m_curveControl.FlipY; }
            set 
            {
                m_curveControl.FlipY = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets CurveCanvas control</summary>
        protected CurveCanvas CurveCanvas
        {
            get { return m_curveControl; }
        }

        /// <summary>
        /// Gets curve editor main menu</summary>
        protected MenuStrip MainMenu
        {
            get { return m_menu; }
        }

        /// <summary>
        /// Gets curve editor tool bar</summary>
        protected ToolStrip ToolBar
        {
            get { return m_topStrip; }
        }

        /// <summary>
        /// Performs custom actions on Paint events. Performs some initialization on first paint</summary>
        /// <param name="e">Paint event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_firstPaint)
            {
                PopulateListView(m_curveControl.Curves);
                m_curveControl.PanToOrigin();
                m_firstPaint = false;
            }
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
            if (keyData == Keys.Delete)
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region private methods
        private void Init(CurveCanvas curveCanvas)
        {
            var lockorgValues = (int[])Enum.GetValues(typeof(OriginLockMode));
            var tanTypeValues = (int[])Enum.GetValues(typeof(CurveTangentTypes));
            var loopValues = (int[])Enum.GetValues(typeof(CurveLoopTypes));
            splitContainer1 = new SplitContainer();
            m_curvesListView = new ListView();

            m_curveControl = curveCanvas;

            m_menu = new MenuStrip();
            m_tangentsMenuItem = new ToolStripMenuItem();
            m_InTangentMenuItem = new ToolStripMenuItem();
            m_outTangentMenuItem = new ToolStripMenuItem();
            m_topStrip = new ToolStrip();
            m_PointLabel = new ToolStripLabel();
            m_xTxtBox = new ToolStripTextBox();
            m_yTxtBox = new ToolStripTextBox();
            m_bottomStrip = new ToolStrip();
            m_MousePos = new ToolStripLabel();
            m_helpForm = new HelpForm();
            m_TangentsSep1 = new ToolStripSeparator();
            m_editModeButtons = new ToolStripButton[4];
            for (int i = 0; i < m_editModeButtons.Length; i++)
            {
                m_editModeButtons[i] = new ToolStripButton();
            }

            m_tangentBtns = new ToolStripButton[5];
            for (int i = 0; i < m_tangentBtns.Length; i++)
            {
                m_tangentBtns[i] = new ToolStripButton();
            }

            m_infinityBtns = new ToolStripButton[4];
            for (int i = 0; i < m_infinityBtns.Length; i++)
            {
                m_infinityBtns[i] = new ToolStripButton();
            }

            // help menu items
            m_helpMenuItem = new ToolStripMenuItem();
            var quickHelpMenuItem = new ToolStripMenuItem();


            m_fitBtn = new ToolStripButton();
            m_breakTangent = new ToolStripButton();
            m_unifyTangent = new ToolStripButton();
            var snapToX = new ToolStripButton();
            var snapToY = new ToolStripButton();
            var snapToPoint = new ToolStripButton();
            var snapToCurve = new ToolStripButton();

            m_undoBtn = new ToolStripButton();
            m_redoBtn = new ToolStripButton();
            //m_cutBtn = new ToolStripButton();
            //m_copyBtn = new ToolStripButton();
            //m_pasteBtn = new ToolStripButton();
            m_delBtn = new ToolStripButton();

            // suspendlayouts            
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();


            m_preInfinityMenuItem = new ToolStripMenuItem();
            m_preInfinityMenuItem.Name = "PreInfinity";
            m_preInfinityMenuItem.Text = "Pre-Infinity".Localize();
            foreach (int val in loopValues)
            {
                string name = Enum.GetName(typeof(CurveLoopTypes), val);
                var item = new ToolStripMenuItem();
                item.Name = "PreInfinity" + name;
                item.Text = name;
                item.Tag = (CurveLoopTypes)val;
                item.Click += curveLoopMenu_Click;
                m_preInfinityMenuItem.DropDownItems.Add(item);
            }

            m_postInfinityMenuItem = new ToolStripMenuItem();
            m_postInfinityMenuItem.Name = "PostInfinity";
            m_postInfinityMenuItem.Text = "Post-Infinity".Localize();
            foreach (int val in loopValues)
            {
                string name = Enum.GetName(typeof(CurveLoopTypes), val);
                var item = new ToolStripMenuItem();
                item.Name = "PostInfinity" + name;
                item.Text = name;
                item.Tag = (CurveLoopTypes)val;
                item.Click += curveLoopMenu_Click;
                m_postInfinityMenuItem.DropDownItems.Add(item);
            }

            m_curveMenuItem = new ToolStripMenuItem();
            m_curveMenuItem.DropDownItems.AddRange(new ToolStripItem[] { m_preInfinityMenuItem, m_postInfinityMenuItem });
            m_curveMenuItem.Name = "Curve";
            m_curveMenuItem.Text = "Curve".Localize();

            var editMenuItem = new ToolStripMenuItem("Edit".Localize());
            editMenuItem.DropDown = m_curveControl.ContextMenuStrip;
           
            m_menu.Location = new Point(0, 0);
            m_menu.Name = "m_menu";
            m_menu.RenderMode = ToolStripRenderMode.System;
            m_menu.Size = new Size(898, 31);
            m_menu.TabIndex = 0;
            m_menu.Text = "menuStrip1";
            m_menu.Renderer = new CustomToolStripRenderer();

            foreach (int val in tanTypeValues)
            {
                var tanType = (CurveTangentTypes)val;
                if (!IsImplemented(tanType))
                    continue;
                string name = Enum.GetName(typeof(CurveTangentTypes), val);
                var item = new ToolStripMenuItem();
                item.Name = name;
                item.Text = name;
                item.Tag = tanType;
                item.Click += TanMenuItem_Click;
                m_tangentsMenuItem.DropDownItems.Add(item);
            }

            m_tangentsMenuItem.DropDownItems.AddRange(new ToolStripItem[] {                        
            m_TangentsSep1,
            m_InTangentMenuItem,
            m_outTangentMenuItem});
            m_tangentsMenuItem.Name = "m_tangentsMenuItem";
            m_tangentsMenuItem.Size = new Size(100, 27);
            m_tangentsMenuItem.Text = "Tangents".Localize();

            foreach (int val in tanTypeValues)
            {
                var tanType = (CurveTangentTypes)val;
                if (!IsImplemented(tanType) || tanType == CurveTangentTypes.Stepped
                    || tanType == CurveTangentTypes.SteppedNext)
                    continue;

                string name = Enum.GetName(typeof(CurveTangentTypes), val);
                var item = new ToolStripMenuItem();
                item.Name = "InTan" + name;
                item.Text = name;
                item.Tag = tanType;
                item.Click += TanMenuItem_Click;
                m_InTangentMenuItem.DropDownItems.Add(item);
            }
            m_InTangentMenuItem.Name = "m_InTangentMenuItem";
            m_InTangentMenuItem.Size = new Size(205, 28);
            m_InTangentMenuItem.Text = "In Tangent".Localize();

            foreach (int val in tanTypeValues)
            {
                var tanType = (CurveTangentTypes)val;
                if (!IsImplemented(tanType) || tanType == CurveTangentTypes.Stepped
                    || tanType == CurveTangentTypes.SteppedNext)
                    continue;

                string name = Enum.GetName(typeof(CurveTangentTypes), val);
                var item = new ToolStripMenuItem();
                item.Name = "OutTan" + name;
                item.Text = name;
                item.Tag = tanType;
                item.Click += TanMenuItem_Click;
                m_outTangentMenuItem.DropDownItems.Add(item);
            }
            m_outTangentMenuItem.Name = "m_outTangentMenuItem";
            m_outTangentMenuItem.Size = new Size(205, 28);
            m_outTangentMenuItem.Text = "Out Tangent".Localize();

            m_helpMenuItem.Name = "helpMenuItem";
            m_helpMenuItem.Text = "Help".Localize();
            m_helpMenuItem.DropDownItems.Add(quickHelpMenuItem);

            quickHelpMenuItem.Name = "quickHelpMenuItem";
            quickHelpMenuItem.Text = "Quick Help...".Localize();
            quickHelpMenuItem.Click += delegate
            {
                if (m_helpForm.Visible)
                {
                    m_helpForm.Activate();
                    return;
                }
                m_helpForm.Show(this);
            };

            m_optionsMenu = new ToolStripMenuItem("Options".Localize());
            var inputmodeMenu = new ToolStripMenuItem("Input Mode".Localize());
            m_basicMenuItem = new ToolStripMenuItem("Basic".Localize());
            m_basicMenuItem.Name = "basic";
            m_basicMenuItem.Click += delegate
            {
                InputMode = InputModes.Basic;

            };
            m_advancedInputMenuItem = new ToolStripMenuItem("Advanced".Localize());
            m_advancedInputMenuItem.Click += delegate
            {
                InputMode = InputModes.Advanced;
            };
            InputMode = m_curveControl.InputMode;

            m_flipYMenuItem = new ToolStripMenuItem("Flip Y-Axis".Localize());
            m_flipYMenuItem.Click += delegate
            {
                FlipY = !FlipY;
            };

            m_optionsMenu.DropDownOpening += delegate { m_flipYMenuItem.Checked = FlipY; };
                       
            inputmodeMenu.DropDownItems.Add(m_basicMenuItem);
            inputmodeMenu.DropDownItems.Add(m_advancedInputMenuItem);

            var lockmenu = new ToolStripMenuItem("Lock Origin".Localize(
                "This is the name of a command. Lock is a verb. Origin is like the origin of a graph."));
            foreach (int val in lockorgValues)
            {
                string name = Enum.GetName(typeof(OriginLockMode), val);
                var item = new ToolStripMenuItem();
                item.Name = name;
                item.Text = name;
                item.Tag = (OriginLockMode)val;
                item.Click += delegate(object sender, EventArgs e)
                {
                    var menuItem = (ToolStripMenuItem)sender;
                    m_curveControl.LockOrigin = (OriginLockMode)menuItem.Tag;
                };
                lockmenu.DropDownItems.Add(item);
            }
            lockmenu.DropDownOpening += delegate
            {
                foreach (ToolStripMenuItem mitem in lockmenu.DropDownItems)
                {
                    mitem.Checked = m_curveControl.LockOrigin == (OriginLockMode)mitem.Tag;
                }
            };

            m_optionsMenu.DropDownItems.Add(inputmodeMenu);
            m_optionsMenu.DropDownItems.Add(lockmenu);
            m_optionsMenu.DropDownItems.Add(m_flipYMenuItem);
                        
            // Initialize CurveTypeSelector (with items and labels)
            m_curveTypeLabel = new ToolStripLabel();
            m_curveTypeLabel.Name = "CurveTypeLabel";
            m_curveTypeLabel.AutoSize = true;
            m_curveTypeLabel.Text = "Type".Localize("curve types");
            m_curveTypeSelector = new ToolStripDropDownButton();
            m_curveTypeSelector.Name = "CurveTypeSelector";
            m_curveTypeSelector.AutoSize = false;
            m_curveTypeSelector.Width = 70;
            m_curveTypeSelector.ToolTipText = "Type of Selected Curve(s)".Localize();
            m_curveTypeSelector.DisplayStyle = ToolStripItemDisplayStyle.Text;
            var linearItem = new ToolStripMenuItem("Linear".Localize());
            linearItem.Tag = InterpolationTypes.Linear;
            linearItem.Name = linearItem.Text;
            var smoothItem = new ToolStripMenuItem("Smooth".Localize());
            smoothItem.Tag = InterpolationTypes.Hermite;
            smoothItem.Name = smoothItem.Text;
            smoothItem.Checked = true;
            m_curveTypeSelector.DropDownItems.Add(linearItem);
            m_curveTypeSelector.DropDownItems.Add(smoothItem);
            m_curveTypeSelector.Text = smoothItem.Text;
            m_curveTypeSelector.DropDownItemClicked += curveTypeSelector_DropDownItemClicked;

            m_menu.Items.AddRange(new ToolStripItem[] {
                editMenuItem,
                m_curveMenuItem,
            m_tangentsMenuItem,m_optionsMenu,m_helpMenuItem});

            m_topStrip.Items.AddRange(m_editModeButtons);
            m_topStrip.Items.Add(new ToolStripSeparator());
            m_topStrip.Items.AddRange(new ToolStripItem[] {                
            m_PointLabel,
            m_xTxtBox,
            m_yTxtBox,
            m_fitBtn,            
            });
            m_tanSeparator1 = new ToolStripSeparator();
            m_tanSeparator2 = new ToolStripSeparator();
            m_topStrip.Items.Add(m_tanSeparator1);
            m_topStrip.Items.Add(m_curveTypeLabel);
            m_topStrip.Items.Add(m_curveTypeSelector);
            m_topStrip.Items.AddRange(m_tangentBtns);
            m_topStrip.Items.Add(m_tanSeparator2);
            m_topStrip.Items.Add(m_breakTangent);
            m_topStrip.Items.Add(m_unifyTangent);
            m_topStrip.Items.Add(new ToolStripSeparator());
            m_topStrip.Items.Add(snapToX);
            m_topStrip.Items.Add(snapToY);
            m_topStrip.Items.Add(snapToPoint);
            m_topStrip.Items.Add(snapToCurve);
            m_topStrip.Items.Add(new ToolStripSeparator());
            m_topStrip.Items.AddRange(m_infinityBtns);
            m_topStrip.Items.Add(new ToolStripSeparator());
            m_topStrip.Items.Add(m_undoBtn);
            m_topStrip.Items.Add(m_redoBtn);
            //m_topStrip.Items.Add(m_cutBtn);
            //m_topStrip.Items.Add(m_copyBtn);
            //m_topStrip.Items.Add(m_pasteBtn);
            m_topStrip.Items.Add(m_delBtn);
            m_topStrip.Items.Add(new ToolStripSeparator());
            m_topStrip.Location = new Point(0, 31);
            m_topStrip.Name = "m_topStrip";
            m_topStrip.RenderMode = ToolStripRenderMode.System;
            m_topStrip.Size = new Size(898, 32);
            m_topStrip.TabIndex = 1;
            m_topStrip.Stretch = true;
            m_topStrip.Text = "topstrip";
            m_topStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_topStrip.MinimumSize = new Size(32, 32);
            m_topStrip.CausesValidation = true;


            for (int i = 0; i < m_editModeButtons.Length; i++)
            {
                m_editModeButtons[i].DisplayStyle = ToolStripItemDisplayStyle.Image;
                m_editModeButtons[i].Click += EditModeClick;
                m_editModeButtons[i].Alignment = ToolStripItemAlignment.Left;
                m_editModeButtons[i].ImageScaling = ToolStripItemImageScaling.None;
            }

            m_editModeButtons[0].Name = "ScalePoint";
            m_editModeButtons[0].Tag = EditModes.Scale;
            m_editModeButtons[0].Image = new Bitmap(typeof(CurveUtils), "Resources.ScaleKeysTool.png");
            m_editModeButtons[0].ToolTipText = "Scale selected control points   " + KeysUtil.KeysToString(CurveCanvas.ShortcutKeys.Scale, true);

            m_editModeButtons[1].Checked = true;
            m_editModeButtons[1].Name = "MovePoint";
            m_editModeButtons[1].Tag = EditModes.Move;
            m_editModeButtons[1].Image = new Bitmap(typeof(CurveUtils), "Resources.MoveKeysTool.png");
            m_editModeButtons[1].ToolTipText = "Move selected control points   " + KeysUtil.KeysToString(CurveCanvas.ShortcutKeys.Move, true);

            m_editModeButtons[2].Name = "InsertPoint";
            m_editModeButtons[2].Tag = EditModes.InsertPoint;
            m_editModeButtons[2].Image = new Bitmap(typeof(CurveUtils), "Resources.InsertKeysTool.png");
            m_editModeButtons[2].ToolTipText = "Insert control point";

            m_editModeButtons[3].Name = "AddPoint";
            m_editModeButtons[3].Tag = EditModes.AddPoint;
            m_editModeButtons[3].Image = new Bitmap(typeof(CurveUtils), "Resources.AddKeysTool.png");
            m_editModeButtons[3].ToolTipText = "Add control point";


            m_PointLabel.Name = "m_PointLabel";
            m_PointLabel.AutoSize = true;
            m_PointLabel.Text = "Stats".Localize();

            m_xTxtBox.Name = "m_XtxtBox";
            m_xTxtBox.Size = new Size(100, 30);
            m_xTxtBox.Validating += InputBoxValidating;
            m_xTxtBox.KeyUp += m_TxtBox_KeyUp;
            m_xTxtBox.ReadOnly = true;


            m_yTxtBox.Name = "m_yTxtBox";
            m_yTxtBox.Size = new Size(100, 30);
            m_yTxtBox.Validating += InputBoxValidating;
            m_yTxtBox.KeyUp += m_TxtBox_KeyUp;
            m_yTxtBox.ReadOnly = true;


            // fit all
            m_fitBtn.Name = "m_fitBtn";
            m_fitBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_fitBtn.Alignment = ToolStripItemAlignment.Left;
            m_fitBtn.Tag = null;
            m_fitBtn.Image = new Bitmap(typeof(CurveUtils), "Resources.FrameAll.png");
            m_fitBtn.ToolTipText = "Fit " + KeysUtil.KeysToString(CurveCanvas.ShortcutKeys.Fit, true);
            m_fitBtn.Click += delegate { m_curveControl.Fit(); };
            m_fitBtn.ImageScaling = ToolStripItemImageScaling.None;
            



            // tangent buttons
            for (int i = 0; i < m_tangentBtns.Length; i++)
            {
                m_tangentBtns[i].DisplayStyle = ToolStripItemDisplayStyle.Image;
                m_tangentBtns[i].Alignment = ToolStripItemAlignment.Left;
                m_tangentBtns[i].Name = "m_tangentBtns" + i;
                m_tangentBtns[i].ImageScaling = ToolStripItemImageScaling.None;
                m_tangentBtns[i].Click += delegate(object sender, EventArgs e)
                {
                    var btn = sender as ToolStripButton;
                    m_curveControl.SetTangent(TangentSelection.TangentInOut, (CurveTangentTypes)btn.Tag);
                };
            }
            m_tangentBtns[0].Tag = CurveTangentTypes.Spline;
            m_tangentBtns[0].Image = new Bitmap(typeof(CurveUtils), "Resources.SplineTangents.png");
            m_tangentBtns[0].ToolTipText = "Spline";

            m_tangentBtns[1].Tag = CurveTangentTypes.Clamped;
            m_tangentBtns[1].Image = new Bitmap(typeof(CurveUtils), "Resources.ClampedTangents.png");
            m_tangentBtns[1].ToolTipText = "Clamped";

            m_tangentBtns[2].Tag = CurveTangentTypes.Linear;
            m_tangentBtns[2].Image = new Bitmap(typeof(CurveUtils), "Resources.LinearTangents.png");
            m_tangentBtns[2].ToolTipText = "Linear";

            m_tangentBtns[3].Tag = CurveTangentTypes.Flat;
            m_tangentBtns[3].Image = new Bitmap(typeof(CurveUtils), "Resources.FlatTangents.png");
            m_tangentBtns[3].ToolTipText = "Flat";

            m_tangentBtns[4].Tag = CurveTangentTypes.Stepped;
            m_tangentBtns[4].Image = new Bitmap(typeof(CurveUtils), "Resources.StepTangents.png");
            m_tangentBtns[4].ToolTipText = "Step";


            // break tangents
            m_breakTangent.Name = "m_breakTangent";
            m_breakTangent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_breakTangent.Alignment = ToolStripItemAlignment.Left;
            m_breakTangent.Image = new Bitmap(typeof(CurveUtils), "Resources.BreakTangents.png");
            m_breakTangent.ImageScaling = ToolStripItemImageScaling.None;
            m_breakTangent.ToolTipText = "Break Tangents";
            m_breakTangent.Click += delegate { m_curveControl.BreakTangents(true); };

            m_unifyTangent.Name = "m_unifyTangent";
            m_unifyTangent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_unifyTangent.Alignment = ToolStripItemAlignment.Left;
            m_unifyTangent.Image = new Bitmap(typeof(CurveUtils), "Resources.UnifyTangents.png");
            m_unifyTangent.ImageScaling = ToolStripItemImageScaling.None;
            m_unifyTangent.ToolTipText = "Unify Tangents";
            m_unifyTangent.Click += delegate { m_curveControl.BreakTangents(false); };


            snapToX.Checked = m_curveControl.AutoSnapToX;
            snapToX.Name = "snapToX";
            snapToX.DisplayStyle = ToolStripItemDisplayStyle.Image;
            snapToX.Alignment = ToolStripItemAlignment.Left;
            snapToX.Image = new Bitmap(typeof(CurveUtils), "Resources.TimeSnap.png");
            snapToX.ImageScaling = ToolStripItemImageScaling.None;
            snapToX.ToolTipText = "Auto snap to major X tick";
            snapToX.Click += delegate
            {
                snapToX.Checked = !snapToX.Checked;
                m_curveControl.AutoSnapToX = snapToX.Checked;
            };


            snapToY.Checked = m_curveControl.AutoSnapToY;
            snapToY.Name = "snapToY";
            snapToY.DisplayStyle = ToolStripItemDisplayStyle.Image;
            snapToY.Alignment = ToolStripItemAlignment.Left;
            snapToY.Image = new Bitmap(typeof(CurveUtils), "Resources.ValueSnap.png");
            snapToY.ImageScaling = ToolStripItemImageScaling.None;
            snapToY.ToolTipText = "Auto snap to major Y tick";
            snapToY.Click += delegate
            {
                snapToY.Checked = !snapToY.Checked;
                m_curveControl.AutoSnapToY = snapToY.Checked;
            };

            snapToPoint.Checked = m_curveControl.AutoPointSnap;
            snapToPoint.Name = "snapToPoint";
            snapToPoint.DisplayStyle = ToolStripItemDisplayStyle.Image;
            snapToPoint.Alignment = ToolStripItemAlignment.Left;
            snapToPoint.Image = new Bitmap(typeof(CurveUtils), "Resources.PointSnap.png");
            snapToPoint.ImageScaling = ToolStripItemImageScaling.None;
            snapToPoint.ToolTipText = "Auto snap to point";
            snapToPoint.Click += delegate
            {
                snapToPoint.Checked = !snapToPoint.Checked;
                m_curveControl.AutoPointSnap = snapToPoint.Checked;
            };

            snapToCurve.Checked = m_curveControl.AutoCurveSnap;
            snapToCurve.Name = "snapToCurve";
            snapToCurve.DisplayStyle = ToolStripItemDisplayStyle.Image;
            snapToCurve.Alignment = ToolStripItemAlignment.Left;
            snapToCurve.Image = new Bitmap(typeof(CurveUtils), "Resources.CurveSnap.png");
            snapToCurve.ImageScaling = ToolStripItemImageScaling.None;
            snapToCurve.ToolTipText = "Auto snap to curve";
            snapToCurve.Click += delegate
            {
                snapToCurve.Checked = !snapToCurve.Checked;
                m_curveControl.AutoCurveSnap = snapToCurve.Checked;
            };

            for (int i = 0; i <= 1; i++)
            {
                m_infinityBtns[i].DisplayStyle = ToolStripItemDisplayStyle.Image;
                m_infinityBtns[i].Alignment = ToolStripItemAlignment.Left;
                m_infinityBtns[i].Name = "m_infinityBtns" + i;
                m_infinityBtns[i].ImageScaling = ToolStripItemImageScaling.None;
                m_infinityBtns[i].Click += delegate(object sender, EventArgs e)
                {
                    var btn = sender as ToolStripButton;
                    m_curveControl.SetPreInfinity((CurveLoopTypes)btn.Tag);
                };
            }

            for (int i = 2; i <= 3; i++)
            {
                m_infinityBtns[i].DisplayStyle = ToolStripItemDisplayStyle.Image;
                m_infinityBtns[i].Alignment = ToolStripItemAlignment.Left;
                m_infinityBtns[i].Name = "m_infinityBtns" + i;
                m_infinityBtns[i].ImageScaling = ToolStripItemImageScaling.None;
                m_infinityBtns[i].Click += delegate(object sender, EventArgs e)
                {
                    var btn = sender as ToolStripButton;
                    m_curveControl.SetPostInfinity((CurveLoopTypes)btn.Tag);
                };
            }


            m_infinityBtns[0].Tag = CurveLoopTypes.Cycle;
            m_infinityBtns[0].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleBefore.png");
            m_infinityBtns[0].ToolTipText = "Cycle Before";

            m_infinityBtns[1].Tag = CurveLoopTypes.CycleWithOffset;
            m_infinityBtns[1].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleBeforewithOffset.png");
            m_infinityBtns[1].ToolTipText = "Cycle Before with Offset";

            m_infinityBtns[2].Tag = CurveLoopTypes.Cycle;
            m_infinityBtns[2].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleAfter.png");
            m_infinityBtns[2].ToolTipText = "Cycle After";

            m_infinityBtns[3].Tag = CurveLoopTypes.CycleWithOffset;
            m_infinityBtns[3].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleAfterwithOffset.png");
            m_infinityBtns[3].ToolTipText = "Cycle After with Offset";

            // udo/redo/cut/copy/paste/delete  buttons            
            m_undoBtn.Name = "m_undoBtn";
            m_undoBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_undoBtn.Alignment = ToolStripItemAlignment.Left;
            m_undoBtn.Image = ResourceUtil.GetImage24(Resources.UndoImage);
            m_undoBtn.ImageScaling = ToolStripItemImageScaling.None;
            m_undoBtn.ToolTipText = "Undo";
            m_undoBtn.Click += delegate { m_curveControl.Undo(); };

            m_redoBtn.Name = "m_redoBtn";
            m_redoBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_redoBtn.Alignment = ToolStripItemAlignment.Left;
            m_redoBtn.Image = ResourceUtil.GetImage24(Resources.RedoImage);
            m_redoBtn.ImageScaling = ToolStripItemImageScaling.None;
            m_redoBtn.ToolTipText = "Redo";
            m_redoBtn.Click += delegate { m_curveControl.Redo(); };

            //m_cutBtn.Name = "cutBtn";
            //m_cutBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //m_cutBtn.Alignment = ToolStripItemAlignment.Left;
            //m_cutBtn.Image = ResourceUtil.GetImage24(Resources.CutImage);
            //m_cutBtn.ImageScaling = ToolStripItemImageScaling.None;
            //m_cutBtn.ToolTipText = "Cut selected points";
            //m_cutBtn.Click += delegate { m_curveControl.Cut(); };

            //m_copyBtn.Name = "copyBtn";
            //m_copyBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //m_copyBtn.Alignment = ToolStripItemAlignment.Left;
            //m_copyBtn.Image = ResourceUtil.GetImage24(Resources.CopyImage);
            //m_copyBtn.ImageScaling = ToolStripItemImageScaling.None;
            //m_copyBtn.ToolTipText = "Copy selected points";
            //m_copyBtn.Click += delegate { m_curveControl.Copy(); };

            //m_pasteBtn.Name = "pasteBtn";
            //m_pasteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //m_pasteBtn.Alignment = ToolStripItemAlignment.Left;
            //m_pasteBtn.Image = ResourceUtil.GetImage24(Resources.PasteImage);
            //m_pasteBtn.ImageScaling = ToolStripItemImageScaling.None;
            //m_pasteBtn.ToolTipText = "Paste selected points";
            //m_pasteBtn.Click += delegate { m_curveControl.Paste(); };

            m_delBtn.Name = "delBtn";
            m_delBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_delBtn.Alignment = ToolStripItemAlignment.Left;
            m_delBtn.Image = ResourceUtil.GetImage24(Resources.DeleteImage);
            m_delBtn.ImageScaling = ToolStripItemImageScaling.None;
            m_delBtn.ToolTipText = "Delete selected points";
            m_delBtn.Click += delegate { m_curveControl.Delete(); };

            m_bottomStrip.Dock = DockStyle.Bottom;
            m_bottomStrip.Items.AddRange(new ToolStripItem[] { m_MousePos });
            m_bottomStrip.Location = new Point(0, 549);
            m_bottomStrip.Name = "m_bottomStrip";
            m_bottomStrip.RenderMode = ToolStripRenderMode.System;
            m_bottomStrip.Size = new Size(898, 26);
            m_bottomStrip.TabIndex = 2;
            m_bottomStrip.Text = "toolStrip2";
            m_bottomStrip.GripStyle = ToolStripGripStyle.Hidden;

            m_MousePos.Alignment = ToolStripItemAlignment.Left;
            m_MousePos.AutoSize = true;
            m_MousePos.Name = "m_MousePos";
            m_MousePos.Size = new Size(250, 27);
            m_MousePos.Text = "Mouse Position".Localize();

            m_curveControl.Dock = DockStyle.Fill;
            m_curveControl.Location = new Point(24, 61);
            m_curveControl.Name = "m_curveControl";
            m_curveControl.Size = new Size(900, 600);
            m_curveControl.TabIndex = 0;
            m_curveControl.TabStop = false;
            m_curveControl.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                PointD gp = m_curveControl.ClientToGraph_d(e.X, e.Y);
                m_MousePos.Text = string.Format("{0}, {1}", Math.Round(gp.X, 4), Math.Round(gp.Y, 4));
            };
            m_curveControl.MouseLeave += delegate { m_MousePos.Text = ""; };
            m_curveControl.MouseUp += delegate { UpdateInputBoxes(); };
            m_curveControl.EditMode = EditModes.Move;
            m_curveControl.EditModeChanged += delegate
            {
                foreach (ToolStripButton btn in m_editModeButtons)
                {
                    btn.Checked = (EditModes)btn.Tag == m_curveControl.EditMode;
                }
            };
            m_curveControl.SelectionChanged += SelectionChanged;

            m_TangentsSep1.Name = "m_TangentsSep1";
            m_TangentsSep1.Size = new Size(202, 6);

            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.ForeColor = SystemColors.Control;
            splitContainer1.Location = new Point(0, 48);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Panel1MinSize = 30;
            splitContainer1.Panel2MinSize = 30;
            splitContainer1.Panel1.Controls.Add(m_curvesListView);
            splitContainer1.Panel2.Controls.Add(m_curveControl);
            splitContainer1.Size = new Size(898, 520);
            splitContainer1.SplitterDistance = 180;
            splitContainer1.SplitterIncrement = 5;
            splitContainer1.TabIndex = 0;
            splitContainer1.TabStop = false;
            splitContainer1.Text = "splitContainer1";
            splitContainer1.BorderStyle = BorderStyle.None;
            splitContainer1.SplitterWidth = 4;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            splitContainer1.SplitterMoving += splitContainer1_SplitterMoving;

            // list view                  
            m_curvesListView.CheckBoxes = true;
            m_curvesListView.Dock = DockStyle.Fill;
            m_curvesListView.HideSelection = false;
            m_curvesListView.LabelEdit = false;
            m_curvesListView.Location = new Point(0, 0);
            m_curvesListView.Name = "m_curvesListView";
            m_curvesListView.Size = new Size(300, 300);
            m_curvesListView.TabIndex = 0;
            m_curvesListView.TabStop = false;
            m_curvesListView.TileSize = new Size(250, 24);
            m_curvesListView.UseCompatibleStateImageBehavior = false;
            m_curvesListView.View = View.Details;
            m_curvesListView.Sorting = SortOrder.Ascending;
            m_curvesListView.ItemChecked += m_curvesListView_ItemChecked;            
            m_curvesListView.Scrollable = true;
            m_curvesListView.Columns.Add("Curves", 250);
            m_curvesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            m_curvesListView.AllowColumnReorder = false;
            m_curvesListView.BackColor = m_curveControl.BackColor;

            var addMenuItem = new ToolStripMenuItem("Add Point".Localize());
            var listMenuStrip = new ContextMenuStrip();
            m_curvesListView.ContextMenuStrip = listMenuStrip;
            listMenuStrip.Opening += delegate
            {
                addMenuItem.Enabled = m_curvesListView.SelectedItems.Count > 0;
            };
            listMenuStrip.Items.Add(addMenuItem);
            addMenuItem.Click += delegate
            {
                if (m_curvesListView.SelectedItems.Count == 0)
                    return;
                var dlg = new AddPointDialog();
                dlg.Location = new Point(MousePosition.X, MousePosition.Y);
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                    return;

                PointF pt = dlg.PointPosition;
                m_curveControl.TransactionContext.DoTransaction(delegate
                {
                    foreach (ListViewItem item in m_curvesListView.SelectedItems)
                    {
                        var curve = (ICurve)item.Tag;
                        IControlPoint cpt = curve.CreateControlPoint();
                        float x = pt.X;

                        int index = CurveUtils.GetValidInsertionIndex(curve, x);
                        while (index == -1)
                        {
                            x += CurveUtils.Epsilone;
                            index = CurveUtils.GetValidInsertionIndex(curve, x);
                        }

                        cpt.X = x;
                        cpt.Y = pt.Y;
                        curve.InsertControlPoint(index, cpt);
                        CurveUtils.ComputeTangent(curve);
                    }
                }, "Add Point".Localize());

                m_curveControl.Invalidate();
            };

            ClientSize = new Size(898, 575);
            Dock = DockStyle.Fill;
            Controls.Add(splitContainer1);
            Controls.Add(m_bottomStrip);
            Controls.Add(m_topStrip);
            Controls.Add(m_menu);

            // resume layouts.                        
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
            Invalidated += CurveEditorControl_Invalidated;
            SetUI(false);
            Application.Idle += Application_Idle;
        }

        void curveTypeSelector_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var menuItem = e.ClickedItem as ToolStripMenuItem;
            if (menuItem.Checked)
                return;

            foreach (ToolStripMenuItem dropDownItem in m_curveTypeSelector.DropDownItems)
                dropDownItem.Checked = false;
            menuItem.Checked = true;

            m_curveTypeSelector.Text = menuItem.Text;

            foreach (ICurve curve in m_curveControl.Curves)
                curve.CurveInterpolation = (InterpolationTypes)menuItem.Tag;

            Invalidate();
        }
        
        private void Application_Idle(object sender, EventArgs e)
        {
            bool hasSelection = m_curveControl.Selection.Count > 0;
            
            m_undoBtn.Enabled = m_curveControl.HistoryContext.CanUndo;
            m_redoBtn.Enabled = m_curveControl.HistoryContext.CanRedo;
            //m_cutBtn.Enabled = hasSelection;
            //m_copyBtn.Enabled = hasSelection;
            //m_pasteBtn.Enabled = m_curveControl.CanPaste;
            m_delBtn.Enabled = hasSelection;            
        }

        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Control ct = sender as Control;
            ct.Cursor = Cursors.VSplit;
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Control ct = sender as Control;
            ct.Cursor = Cursors.Default;
        }

        private void EditModeClick(object sender, EventArgs e)
        {
            ToolStripButton clickedBtn = (ToolStripButton)sender;
            foreach (ToolStripButton btn in m_editModeButtons)
            {
                btn.Checked = btn == clickedBtn;
            }
            m_curveControl.EditMode = (EditModes)clickedBtn.Tag;
        }

        private void m_curvesListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ICurve curve = e.Item.Tag as ICurve;
            curve.Visible = e.Item.Checked;
            if (!curve.Visible)
                m_curveControl.RemoveCurveFromSelection(curve);
            Invalidate();
        }
        
        private void PopulateListView(ReadOnlyCollection<ICurve> curves)
        {
            m_curvesListView.Items.Clear();
            if (curves == null || curves.Count == 0)
            {
                return;
            }
            m_curvesListView.BeginUpdate();
            foreach (ICurve curve in curves)
            {
                string name = curve.Name;
                if (name.Length > 250)
                    name = name.Substring(0, 250);
                ListViewItem item = new ListViewItem(name);
                item.ForeColor = curve.CurveColor;
                item.Checked = curve.Visible;
                item.Tag = curve;
                m_curvesListView.Items.Add(item);
            }
            m_curvesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            m_curvesListView.EndUpdate();
        }

        private void SetUI(bool enable)
        {
            foreach (ToolStripItem item in m_topStrip.Items)
                item.Enabled = enable;
            foreach (ToolStripItem item in m_menu.Items)
                item.Enabled = enable;
            m_optionsMenu.Enabled = true;
            m_helpMenuItem.Enabled = true;            
        }

        private void CurveEditorControl_Invalidated(object sender, InvalidateEventArgs e)
        {
            m_curveControl.Invalidate();
        }


        private void TanMenuItem_Click(object sender, EventArgs e)
        {
            if (m_curveControl.Selection.Count == 0)
                return;

            var menuItem = sender as ToolStripMenuItem;
            if (menuItem.OwnerItem == null)
                throw new InvalidOperationException("Improper UI initialization");

            var tanSelection = TangentSelection.None;
            if (menuItem.OwnerItem == m_tangentsMenuItem)
            {
                tanSelection = TangentSelection.TangentInOut;
            }
            else if (menuItem.OwnerItem == m_InTangentMenuItem)
            {
                tanSelection = TangentSelection.TangentIn;
            }
            else if (menuItem.OwnerItem == m_outTangentMenuItem)
            {
                tanSelection = TangentSelection.TangentOut;
            }

            if (tanSelection != TangentSelection.None)
            {
                m_curveControl.SetTangent(tanSelection, (CurveTangentTypes)menuItem.Tag);
            }

        }

        private void curveLoopMenu_Click(object sender, EventArgs e)
        {

            var menuItem = sender as ToolStripMenuItem;
            if (menuItem.OwnerItem == null)
                throw new InvalidOperationException("Improper UI initialization");
            if (menuItem.OwnerItem == m_preInfinityMenuItem)
            {
                m_curveControl.SetPreInfinity((CurveLoopTypes)menuItem.Tag);
            }
            else if (menuItem.OwnerItem == m_postInfinityMenuItem)
            {
                m_curveControl.SetPostInfinity((CurveLoopTypes)menuItem.Tag);
            }
            else
            {
                throw new InvalidOperationException(menuItem.OwnerItem.Text + " is not valid parent for " + menuItem.Text);
            }
        }

        private void m_TxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                m_curveControl.Focus();
            }
        }

        private void InputBoxValidating(object sender, CancelEventArgs e)
        {
            var txtBox = sender as ToolStripTextBox;
            if (m_curveControl.Selection.Count == 0)
            {
                SetInputBoxes(m_xTxtBox, false);
                SetInputBoxes(m_yTxtBox, false);
                return;
            }

            try
            {
                string str = txtBox.Text.Trim();
                var op = MathOperation.Assign;
                if (str.StartsWith("+="))
                {
                    op = MathOperation.Add;
                }
                else if (str.StartsWith("-="))
                {
                    op = MathOperation.Subtract;

                }
                else if (str.StartsWith("*="))
                {
                    op = MathOperation.Multiply;
                }
                else if (str.StartsWith("/="))
                {
                    op = MathOperation.Divide;

                }
                if (op != MathOperation.Assign)
                {
                    str = str.Substring(2);
                }

                if (string.IsNullOrEmpty(str))
                    return;

                var curveset = new HashSet<ICurve>();
                var orglist = new List<Vec2F>();
                foreach (IControlPoint cpt in m_curveControl.Selection)
                {                    
                    orglist.Add(new Vec2F(cpt.X, cpt.Y));
                }

                float f = float.Parse(str);
                m_curveControl.TransactionContext.DoTransaction(delegate
                {
                    if (txtBox == m_xTxtBox)
                    {
                        bool canEdit = (bool)m_xTxtBox.Tag
                        || ((op != MathOperation.Assign) && (op != MathOperation.Multiply
                        || f != 0));
                        if (canEdit)
                        {
                            foreach (IControlPoint cpt in m_curveControl.Selection)
                            {
                                cpt.X = Operate(cpt.X, f, op);
                                if (!CurveUtils.IsSorted(cpt))
                                    curveset.Add(cpt.Parent);
                            }
                            if (m_curveControl.RestrictedTranslationEnabled && curveset.Count > 0)
                            {
                                for (int i = 0; i < m_curveControl.Selection.Count; i++)
                                {
                                    IControlPoint cpt = m_curveControl.Selection[i];
                                    var orgCpt = orglist[i];
                                    cpt.X = orgCpt.X;
                                    cpt.Y = orgCpt.Y;                                   
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (IControlPoint cpt in m_curveControl.Selection)
                        {
                            cpt.Y = Operate(cpt.Y, f, op);
                        }
                    }

                    foreach (ICurve curve in m_curveControl.SelectedCurves)
                    {
                        CurveUtils.Sort(curve);
                        CurveUtils.ForceMinDistance(curve);
                        CurveUtils.ComputeTangent(curve);
                    }
                    CurveCanvas.UpdateCurveLimits();
                },"Edit Point".Localize());
                m_curveControl.Invalidate();
            }
            catch (Exception ex)
            {
                string str = txtBox.Text;
                bool sameval = true;
                float val = 0.0f;
                if (txtBox == m_xTxtBox)
                {
                    val = m_curveControl.Selection[0].X;
                    foreach (IControlPoint cpt in m_curveControl.Selection)
                    {
                        if (cpt.X != val)
                        {
                            sameval = false;
                            break;
                        }
                    }
                }
                else
                {
                    val = m_curveControl.Selection[0].Y;
                    foreach (IControlPoint cpt in m_curveControl.Selection)
                    {
                        if (cpt.Y != val)
                        {
                            sameval = false;
                            break;
                        }
                    }
                }
                txtBox.Text = (sameval) ? val.ToString() : "";
                MessageBox.Show(this, str + " " + ex.Message, "CurveEditor");
            }
        }

        private float Operate(float val1, float val2, MathOperation operation)
        {
            float retval = 0.0f;
            if (operation == MathOperation.Add)
            {
                retval = val1 + val2;
            }
            else if (operation == MathOperation.Subtract)
            {
                retval = val1 - val2;
            }
            else if (operation == MathOperation.Multiply)
            {
                retval = val1 * val2;
            }
            else if (operation == MathOperation.Divide)
            {
                retval = val1 / val2; // ok to throw exception.
            }
            else if (operation == MathOperation.Assign)
            {
                retval = val2;
            }
            return retval;
        }


        private void SelectionChanged(object sender, EventArgs e)
        {
            UpdateInputBoxes();
        }

        private void UpdateInputBoxes()
        {

            if (m_curveControl.Selection.Count > 0)
            {
                SetInputBoxes(m_yTxtBox, true);
                SetInputBoxes(m_xTxtBox, true);

                bool sameval = true;
                float val = m_curveControl.Selection[0].Y;
                foreach (IControlPoint cpt in m_curveControl.Selection)
                {
                    if (cpt.Y != val)
                    {
                        sameval = false;
                        break;
                    }
                }
                m_yTxtBox.Text = (sameval) ? val.ToString() : "";
                m_yTxtBox.BackColor = (sameval) ? SystemColors.Window : m_multiPointColor;

                bool allowAssignment = true;
                Dictionary<ICurve, object> curves = new Dictionary<ICurve, object>();
                foreach (IControlPoint cpt in m_curveControl.Selection)
                {
                    if (cpt.Parent != null && curves.ContainsKey(cpt.Parent))
                    {
                        allowAssignment = false;
                        break;
                    }
                    curves.Add(cpt.Parent, null);
                }
                m_xTxtBox.Tag = allowAssignment;
                sameval = true;
                val = m_curveControl.Selection[0].X;
                foreach (IControlPoint cpt in m_curveControl.Selection)
                {
                    if (cpt.X != val)
                    {
                        sameval = false;
                        break;
                    }
                }
                m_xTxtBox.Text = (sameval) ? val.ToString() : "";
                m_xTxtBox.BackColor = (sameval) ? SystemColors.Window : m_multiPointColor;
            }
            else
            {
                SetInputBoxes(m_yTxtBox, false);
                SetInputBoxes(m_xTxtBox, false);
            }
        }


        private void SetInputBoxes(ToolStripTextBox txtBox, bool active)
        {
            txtBox.Text = "";
            txtBox.Tag = null;
            if (active)
            {
                txtBox.BackColor = SystemColors.Window;
                txtBox.ReadOnly = false;
            }
            else
            {
                txtBox.BackColor = SystemColors.Control;
                txtBox.ReadOnly = true;
            }
        }

        private void UpdateCurveTypeSelector()
        {
            bool linearFound = false;
            bool smoothFound = false;
            foreach (ICurve curve in m_curveControl.Curves)
            {
                if (curve.CurveInterpolation == InterpolationTypes.Linear)
                    linearFound = true;
                else if (curve.CurveInterpolation == InterpolationTypes.Hermite)
                    smoothFound = true;

                if (linearFound && smoothFound)
                    break;
            }

            InterpolationTypes interpol = InterpolationTypes.None; // mixed
            if (linearFound && !smoothFound) // all linear
                interpol = InterpolationTypes.Linear;
            else if (smoothFound && !linearFound) // all smooth
                interpol = InterpolationTypes.Hermite;
            SetCurveTypeSelector(interpol);
        }

        private void SetCurveTypeSelector(InterpolationTypes interpolationTypes)
        {
            if (interpolationTypes == InterpolationTypes.None)
            {
                foreach (ToolStripMenuItem item in m_curveTypeSelector.DropDownItems)
                    item.Checked = false;
                m_curveTypeSelector.Text = "(Multiple)".Localize("CurveTypeSelector");
            }
            else
            {
                foreach (ToolStripMenuItem item in m_curveTypeSelector.DropDownItems)
                {
                    if (((InterpolationTypes)item.Tag == interpolationTypes))
                    {
                        item.Checked = true;
                        m_curveTypeSelector.Text = item.Text;
                    }
                    else
                        item.Checked = false;
                }
            }
        }
        
        private bool IsImplemented(CurveTangentTypes tanType)
        {
            bool result = false;
            switch (tanType)
            {
                case CurveTangentTypes.Linear:
                case CurveTangentTypes.Spline:
                case CurveTangentTypes.Flat:
                case CurveTangentTypes.Clamped:
                case CurveTangentTypes.Stepped:
                    result = true;
                    break;
            }
            return result;
        }
        #endregion

        #region private fields
        private bool m_showTangentEditing = true;
        private ToolStripButton m_undoBtn;
        private ToolStripButton m_redoBtn;
        //private ToolStripButton m_cutBtn;
        //private ToolStripButton m_copyBtn;
        //private ToolStripButton m_pasteBtn;
        private ToolStripButton m_delBtn;
        private ToolStripSeparator m_tanSeparator1;
        private ToolStripSeparator m_tanSeparator2;
        private readonly Color m_multiPointColor = Color.FromArgb(186, 150, 190);
        private CurveCanvas m_curveControl;
        private MenuStrip m_menu;
        private ToolStrip m_topStrip;
        private ToolStrip m_bottomStrip;
        private ToolStripMenuItem m_helpMenuItem;
        private ToolStripMenuItem m_optionsMenu;
        private ToolStripMenuItem m_curveMenuItem;
        private ToolStripMenuItem m_preInfinityMenuItem;
        private ToolStripMenuItem m_postInfinityMenuItem;
        private ToolStripMenuItem m_tangentsMenuItem;
        private ToolStripMenuItem m_InTangentMenuItem;
        private ToolStripMenuItem m_outTangentMenuItem;
        private ToolStripLabel m_MousePos;
        private ToolStripTextBox m_xTxtBox;
        private ToolStripLabel m_PointLabel;
        private ToolStripTextBox m_yTxtBox;
        private ToolStripSeparator m_TangentsSep1;

        private SplitContainer splitContainer1;
        private ListView m_curvesListView;
        private HelpForm m_helpForm;
        private ToolStripButton[] m_editModeButtons;
        private ToolStripButton m_fitBtn;
        private ToolStripButton[] m_tangentBtns;
        private ToolStripButton m_breakTangent;
        private ToolStripButton m_unifyTangent;
        private ToolStripButton[] m_infinityBtns;
        private bool m_firstPaint = true;
        private ToolStripMenuItem m_basicMenuItem;
        private ToolStripMenuItem m_flipYMenuItem;
        private ToolStripMenuItem m_advancedInputMenuItem;        
        private ToolStripLabel m_curveTypeLabel;
        private ToolStripDropDownButton m_curveTypeSelector;

        #endregion

        #region private enums and classes

        private enum MathOperation
        {
            Assign,
            Add,
            Subtract,
            Multiply,
            Divide,
        }

        private class HelpForm : Form
        {
            public HelpForm()
            {
                Size = new Size(700, 450);
                Text = "Quick startup guide";
                Font = new Font(Font.Name, 12);
                m_textBox = new TextBox();
                m_textBox.ReadOnly = true;
                m_textBox.BackColor = SystemColors.Info;
                m_textBox.Multiline = true;
                m_textBox.WordWrap = true;
                m_textBox.Dock = DockStyle.Fill;
                m_textBox.ScrollBars = ScrollBars.Vertical;
                m_textBox.Font = new Font("Lucida Console", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

                Assembly assem = Assembly.GetAssembly(typeof(CurveUtils));
                Stream strm = assem.GetManifestResourceStream(typeof(CurveUtils), "Resources.QuickHelp.txt");
                var reader = new StreamReader(strm);
                m_textBox.Text = reader.ReadToEnd();
                reader.Close();
                strm.Close();

                m_textBox.Select(0, 0);
                Controls.Add(m_textBox);
                ShowInTaskbar = false;
                FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                e.Cancel = true;
                base.OnClosing(e);
                Hide();
            }
            private readonly TextBox m_textBox;

        }

        private class CustomToolStripRenderer : ToolStripSystemRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Enabled)
                    base.OnRenderMenuItemBackground(e);
            }
        }

        private class AddPointDialog : Form
        {
            public AddPointDialog()
            {
                label1 = new Label();
                label2 = new Label();
                txtBoxX = new TextBox();
                textBoxY = new TextBox();
                cancelBtn = new Button();
                OkBtn = new Button();
                SuspendLayout();
                // 
                // label1
                // 
                label1.AutoSize = true;
                label1.Location = new Point(12, 13);
                label1.Name = "label1";
                label1.Size = new Size(18, 18);
                label1.TabIndex = 0;
                label1.Text = "X";
                // 
                // label2
                // 
                label2.AutoSize = true;
                label2.Location = new Point(13, 43);
                label2.Name = "label2";
                label2.Size = new Size(17, 18);
                label2.TabIndex = 1;
                label2.Text = "Y";
                // 
                // txtBoxX
                // 
                txtBoxX.Location = new Point(36, 7);
                txtBoxX.Name = "txtBoxX";
                txtBoxX.Size = new Size(197, 24);
                txtBoxX.TabIndex = 2;
                txtBoxX.Text = "0";
                // 
                // textBoxY
                // 
                textBoxY.Location = new Point(36, 37);
                textBoxY.Name = "textBoxY";
                textBoxY.Size = new Size(197, 24);
                textBoxY.TabIndex = 3;
                textBoxY.Text = "0";
                // 
                // cancelBtn
                // 
                cancelBtn.DialogResult = DialogResult.Cancel;
                cancelBtn.Location = new Point(158, 77);
                cancelBtn.Name = "cancelBtn";
                cancelBtn.Size = new Size(75, 29);
                cancelBtn.TabIndex = 5;
                cancelBtn.Text = "&Cancel";
                cancelBtn.UseVisualStyleBackColor = true;
                // 
                // OkBtn
                //
                OkBtn.Location = new Point(77, 77);
                OkBtn.Name = "OkBtn";
                OkBtn.Size = new Size(75, 29);
                OkBtn.TabIndex = 6;
                OkBtn.Text = "&Ok";
                OkBtn.UseVisualStyleBackColor = true;
                OkBtn.Click += OkBtn_Click;


                // 
                // Form1
                //                 
                AutoScaleMode = AutoScaleMode.None;
                CancelButton = cancelBtn;
                ClientSize = new Size(248, 111);
                Controls.Add(OkBtn);
                Controls.Add(cancelBtn);
                Controls.Add(textBoxY);
                Controls.Add(txtBoxX);
                Controls.Add(label2);
                Controls.Add(label1);
                Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
                FormBorderStyle = FormBorderStyle.FixedToolWindow;
                Margin = new Padding(4, 4, 4, 4);
                Name = "Form1";
                ShowIcon = false;
                ShowInTaskbar = false;
                Text = "Add Point";
                StartPosition = FormStartPosition.Manual;
                ResumeLayout(false);
                PerformLayout();
            }

            private void OkBtn_Click(object sender, EventArgs e)
            {
                bool parsed = ValidateAndParseInput();
                if (parsed)
                {
                    DialogResult = DialogResult.OK;
                }
            }

            public PointF PointPosition
            {
                get { return point; }
            }

            private bool ValidateAndParseInput()
            {
                string strX = txtBoxX.Text.Trim();
                string strY = textBoxY.Text.Trim();

                float x = 0.0f;
                float y = 0.0f;
                point = new PointF(0, 0);

                try
                {
                    x = float.Parse(strX);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                    txtBoxX.Focus();
                    return false;
                }

                try
                {
                    y = float.Parse(strY);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                    textBoxY.Focus();
                    return false;
                }
                point = new PointF(x, y);
                return true;
            }

            private PointF point;
            private readonly Label label1;
            private readonly Label label2;
            private readonly TextBox txtBoxX;
            private readonly TextBox textBoxY;
            private readonly Button cancelBtn;
            private readonly Button OkBtn;
        }

        #endregion
    }

}
