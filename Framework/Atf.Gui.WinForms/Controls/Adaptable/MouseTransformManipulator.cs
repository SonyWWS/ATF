//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Control adapter that converts mouse drags into translations and scales
    /// of the canvas using an ITransformAdapter</summary>
    public class MouseTransformManipulator : ControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">Transform adapter</param>
        public MouseTransformManipulator(ITransformAdapter transformAdapter)
        {
            m_transformAdapter = transformAdapter;
        }

        /// <summary>
        /// Gets or sets the modifier keys that must be pressed during drags
        /// to manipulate the transform. Default is 'Alt'.</summary>
        public Keys ModifierKeys
        {
            get { return m_modifierKeys.HasValue ? m_modifierKeys.Value : s_defaultModifierKeys; }
            set { m_modifierKeys = value; }
        }
        private Keys? m_modifierKeys;

        /// <summary>
        /// Gets or sets the default modifier keys if ModifierKeys isn't set. Default
        /// is 'Alt'.</summary>
        public static Keys DefaultModifierKeys
        {
            get { return s_defaultModifierKeys; }
            set { s_defaultModifierKeys = value; }
        }
        private static Keys s_defaultModifierKeys = Keys.Alt;

        /// <summary>
        /// Gets or sets the modifier keys used to constrain the transform of the view
        /// of the canvas to either the x-axis or y-axis. The default is Keys.None, which
        /// disables this capability, because this setting interferes with D2dGraphNodeEditAdapter's
        /// constraining of moving of the objects on the canvas, which is more useful.</summary>
        public Keys ConstrainModifierKeys
        {
            get { return m_constrainModifierKeys.HasValue ? m_constrainModifierKeys.Value : s_defaultConstrainModifierKeys; }
            set { m_constrainModifierKeys = value; }
        }
        private Keys? m_constrainModifierKeys;

        /// <summary>
        /// Gets or sets the default modifier keys used to constrain the transform of the view
        /// of the canvas to either the x-axis or y-axis. The default is Keys.None, which
        /// disables this capability, because this setting interferes with D2dGraphNodeEditAdapter's
        /// constraining of moving of the objects on the canvas, which is more useful.</summary>
        public static Keys DefaultConstrainModifierKeys
        {
            get { return s_defaultConstrainModifierKeys; }
            set { s_defaultConstrainModifierKeys = value; }
        }
        private static Keys s_defaultConstrainModifierKeys = Keys.None;

        /// <summary>
        /// Gets or sets the mouse button used when dragging to modify translation.
        /// Default is left mouse button.</summary>
        /// <remarks>You may specify multiple buttons connected by logical OR (e.g., MouseButtons.Left | MouseButtons.Middle).
        /// Translation is activated when one or more of the OR'd buttons are pressed.</remarks>
        public MouseButtons TranslationButton
        {
            get { return m_translationButton.HasValue ? m_translationButton.Value : m_defaultTranslationButton; }
            set { m_translationButton = value; }
        }
        private MouseButtons? m_translationButton;

        /// <summary>
        /// Gets or sets the default mouse button used when dragging to modify translation.
        /// Default is left mouse button.</summary>
        /// <remarks>You may specify multiple buttons connected by logical OR (e.g., MouseButtons.Left | MouseButtons.Middle).
        /// Translation is activated when one or more of the OR'd buttons are pressed.</remarks>
        public static MouseButtons DefaultTranslationButton
        {
            get { return m_defaultTranslationButton; }
            set { m_defaultTranslationButton = value; }
        }
        private static MouseButtons m_defaultTranslationButton = MouseButtons.Left;

        /// <summary>
        /// Gets or sets the mouse button used when dragging to modify scale.
        /// Default is right mouse button.</summary>
        /// <remarks>You may specify multiple buttons connected by logical OR (e.g., MouseButtons.Middle | MouseButtons.Right).
        /// Scaling is activated when one or more of the OR'd buttons are pressed.</remarks>
        public MouseButtons ScaleButton
        {
            get { return m_scaleButton.HasValue ? m_scaleButton.Value : s_defaultScaleButton; }
            set { m_scaleButton = value; }
        }
        private MouseButtons? m_scaleButton;

        /// <summary>
        /// Gets or sets the default mouse button used when dragging to modify scale.
        /// Default is right mouse button.</summary>
        /// <remarks>You may specify multiple buttons connected by logical OR (e.g., MouseButtons.Middle | MouseButtons.Right).
        /// Scale is activated when one or more of the OR'd buttons are pressed.</remarks>
        public static MouseButtons DefaultScaleButton
        {
            get { return s_defaultScaleButton; }
            set { s_defaultScaleButton = value; }
        }
        private static MouseButtons s_defaultScaleButton = MouseButtons.Right;

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseDown += control_MouseDown;
            control.MouseMove += control_MouseMove;
            control.MouseUp += control_MouseUp;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.MouseDown -= control_MouseDown;
            control.MouseMove -= control_MouseMove;
            control.MouseUp -= control_MouseUp;
        }

        /// <summary>
        /// Tests wether or not a translation operation should begin</summary>
        /// <param name="modifiers">Current modifier keys being pressed</param>
        /// <param name="e">Mouse event arg being responded to</param>
        /// <returns><c>True</c> if a translation operation should begin</returns>
        /// <remarks>By default, this method considers the ModifierKeys and TranslationButton
        /// properties, plus allows for the middle mouse button to be used.</remarks>
        protected virtual bool TestForTranslation(Keys modifiers, MouseEventArgs e)
        {
            if (modifiers == Keys.None &&
                e.Button == MouseButtons.Middle)
                return true;

            return
                (modifiers & ModifierKeys) == ModifierKeys &&
                (e.Button & TranslationButton) != 0;
        }

        /// <summary>
        /// Tests whether or not a scale operation should begin</summary>
        /// <param name="modifiers">Current modifier keys being pressed</param>
        /// <param name="e">Mouse event arg being responded to</param>
        /// <returns><c>True</c> if a scale operation should begin</returns>
        /// <remarks>By default, this method considers the ModifierKeys and ScaleButton properties.</remarks>
        protected virtual bool TestForScale(Keys modifiers, MouseEventArgs e)
        {
            return
                (modifiers & ModifierKeys) == ModifierKeys &&
                (e.Button & ScaleButton) != 0;
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            // make sure we can capture the mouse
            if (!AdaptedControl.Capture)
            {
                Keys modifiers = Control.ModifierKeys;
                m_firstPoint = new Point(e.X, e.Y);

                // check for navigation gestures
                if (TestForTranslation(modifiers, e))
                {
                    m_isTranslating = true;
                    m_startingTranslation = m_transformAdapter.Translation;

                    AdaptedControl.Capture = true;
                    m_saveCursor = AdaptedControl.Cursor;
                    AdaptedControl.Cursor = Cursors.Hand;
                }
                else if (TestForScale(modifiers, e))
                {
                    m_isScaling = true;
                    PointF startingTranslation = m_transformAdapter.Translation;
                    m_scaleStart = m_transformAdapter.Scale;
                    m_scaleCenterStart = new PointF(
                        (m_firstPoint.X - startingTranslation.X) / m_scaleStart.X,
                        (m_firstPoint.Y - startingTranslation.Y) / m_scaleStart.Y);

                    AdaptedControl.Capture = true;
                    m_saveCursor = AdaptedControl.Cursor;
                    AdaptedControl.Cursor = Cursors.SizeAll;
                }
            }
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_isTranslating || m_isScaling)
            {
                Point currentPoint = new Point(e.X, e.Y);

                int dx = currentPoint.X - m_firstPoint.X;
                int dy = currentPoint.Y - m_firstPoint.Y;

                if (m_selfSetTranslation != m_transformAdapter.Translation)
                {
                    // It is quite possible that another transformation adapter binded to the same canvas can 
                    // modify the shared ITransformAdapter in succession without releasing the mouse button. 
                    // For example  MMD panning with this adapter followed by MouseWheelManipulator for zooming. 
                    // Need to resync mouse starting position
                    m_startingTranslation = m_transformAdapter.Translation;                
                    m_firstPoint = currentPoint;
                }

                Keys modifiers = Control.ModifierKeys;

                bool constrain =
                    (
                        ConstrainModifierKeys != Keys.None &&
                        (modifiers & ConstrainModifierKeys) == ConstrainModifierKeys
                    ) ||
                    m_isScaling && m_transformAdapter.UniformScale;

                if (constrain)
                {
                    if (!m_constrainXDetermined)
                    {
                        float xDistance = Math.Abs(dx);
                        float yDistance = Math.Abs(dy);
                        Size dragSize = SystemInformation.DragSize;
                        if (xDistance > dragSize.Width ||
                            yDistance > dragSize.Height)
                        {
                            m_constrainX = (xDistance < yDistance);
                            m_constrainXDetermined = true;
                        }
                    }
                }

                if (m_isTranslating)
                {
                    if (constrain)
                    {
                        if (m_constrainX)
                            dx = 0;
                        else
                            dy = 0;
                    }

                    PointF translation = new PointF(
                        m_startingTranslation.X + dx,
                        m_startingTranslation.Y + dy);

                    m_transformAdapter.Translation = m_selfSetTranslation = translation;
                }
                else if (m_isScaling)
                {
                    float xScale = 4 * dx / (float)AdaptedControl.Width;
                    float yScale = 4 * dy / (float)AdaptedControl.Height;

                    if (constrain)
                    {
                        if (m_constrainX)
                            xScale = yScale;
                        else
                            yScale = xScale;
                    }

                    PointF scale = new PointF(
                        Math.Max(0.001f, m_scaleStart.X * (float)Math.Pow(2, xScale)),
                        Math.Max(0.001f, m_scaleStart.Y * (float)Math.Pow(2, yScale)));

                    // constrain scale before calculating translation to maintain scroll center position
                    scale = m_transformAdapter.ConstrainScale(scale);

                    PointF translation = new PointF(
                        m_firstPoint.X - m_scaleCenterStart.X * scale.X,
                        m_firstPoint.Y - m_scaleCenterStart.Y * scale.Y);
                    
                    m_transformAdapter.SetTransform(
                        scale.X,
                        scale.Y,
                        translation.X,
                        translation.Y);

                    m_selfSetTranslation = translation;
                }
            }
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_isTranslating || m_isScaling)
            {
                m_isTranslating = false;
                m_isScaling = false;
                m_constrainXDetermined = false;

                AdaptedControl.Cursor = m_saveCursor;
                AdaptedControl.Capture = false;
            }
        }

        private readonly ITransformAdapter m_transformAdapter;
        private Cursor m_saveCursor;
        private Point m_firstPoint;
        private PointF m_startingTranslation;
        private PointF m_scaleStart;
        private PointF m_scaleCenterStart;
        private bool m_isTranslating;
        private bool m_isScaling;
        private bool m_constrainXDetermined;
        private bool m_constrainX;

        private PointF m_selfSetTranslation; // the translation component set by this adpater to ITransformAdapter
    }
}
