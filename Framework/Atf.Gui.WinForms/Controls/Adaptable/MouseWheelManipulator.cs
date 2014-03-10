//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Control adapter that converts mouse wheel rotation into scales
    /// using an ITransformAdapter</summary>
    public class MouseWheelManipulator : ControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">Transform adapter</param>
        public MouseWheelManipulator(ITransformAdapter transformAdapter)
        {
            m_transformAdapter = transformAdapter;
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseWheel += control_MouseWheel;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.MouseWheel -= control_MouseWheel;
        }

        private void control_MouseWheel(object sender, MouseEventArgs e)
        {
            PointF translation = m_transformAdapter.Translation;
            PointF scale = m_transformAdapter.Scale;
            PointF scaleCenterStart = new PointF(
                (e.X - translation.X) / scale.X,
                (e.Y - translation.Y) / scale.Y);

            float delta = 1.0f + e.Delta / 1200.0f;
            scale = new PointF(
                scale.X * delta,
                scale.Y * delta);

            // constrain scale before calculating translation to maintain scroll center position
            scale = m_transformAdapter.ConstrainScale(scale);

            translation = new PointF(
                e.X - scaleCenterStart.X * scale.X,
                e.Y - scaleCenterStart.Y * scale.Y);

            m_transformAdapter.SetTransform(
                scale.X,
                scale.Y,
                translation.X,
                translation.Y);
        }

        private readonly ITransformAdapter m_transformAdapter;
    }
}
