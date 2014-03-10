using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Curve interface layer</summary>
    public interface ICurveSet
    {
        /// <summary>
        /// Gets list of ICurves</summary>
        IList<ICurve> Curves { get; }
    }

    /// <summary>
    /// Curve interface</summary>
    public interface ICurve
    {
        /// <summary>
        /// Gets curve name</summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets curve Interpolation type</summary>        
        InterpolationTypes CurveInterpolation { get; set; }

        /// <summary>
        /// Gets or sets visibility</summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets minimum x value</summary>
        float MinX { get; set; }

        /// <summary> 
        /// Gets or sets maximum x value</summary>
        float MaxX { get; set; }

        /// <summary> 
        /// Gets or sets minimum y value</summary>
        float MinY { get; set; }

        /// <summary> 
        /// Gets or sets maximum y value</summary>
        float MaxY { get; set; }

        /// <summary>
        /// Gets x axis label</summary>
        string XLabel { get; }

        /// <summary>
        /// Gets y axis label</summary>
        string YLabel { get; }

        /// <summary>
        /// Gets or sets curve color</summary>
        Color CurveColor { get; set; }

        /// <summary>
        /// Gets or sets values before first control point</summary>
        CurveLoopTypes PreInfinity { get; set; }

        /// <summary>
        /// Gets or sets values after last control point</summary>
        CurveLoopTypes PostInfinity { get; set; }

        /// <summary>
        /// Creates a control point</summary>
        /// <returns>Control point</returns>
        IControlPoint CreateControlPoint();

        /// <summary>
        /// Adds control point at the end of the internal list</summary>
        /// <param name="cp">Control point</param> 
        void AddControlPoint(IControlPoint cp);

        /// <summary>
        /// Inserts control point with the specified index into the internal list</summary>
        /// <param name="index">Index</param>
        /// <param name="cp">Control point</param>
        void InsertControlPoint(int index, IControlPoint cp);

        /// <summary>
        /// Removes given control point from the internal list</summary>
        /// <param name="cp">Control point</param>
        void RemoveControlPoint(IControlPoint cp);

        /// <summary>
        /// Gets readonly list of control points</summary>
        ReadOnlyCollection<IControlPoint> ControlPoints { get; }

        /// <summary>
        /// Deletes all the control points</summary>
        void Clear();
    }


    /// <summary>
    /// Interface for a control point, which represents a point on a curve</summary>
    public interface IControlPoint
    {
        /// <summary>
        /// Gets parent curve for this control point</summary>
        ICurve Parent
        {
            get;
        }

        /// <summary>
        /// Gets or sets x coordinate</summary>
        float X { get; set; }

        /// <summary>
        /// Gets or sets y coordinate</summary>
        float Y { get; set; }

        /// <summary>
        /// Gets or sets tangent in value</summary>
        Vec2F TangentIn { get; set; }

        /// <summary>
        /// Gets or sets tangent in type</summary>
        CurveTangentTypes TangentInType { get; set; }

        /// <summary>
        /// Gets or sets tangent out</summary>
        Vec2F TangentOut { get; set; }

        /// <summary>
        /// Gets or sets tanget out type</summary>
        CurveTangentTypes TangentOutType { get; set; }

        /// <summary>
        /// Gets or sets whether the tangents are broken</summary>
        bool BrokenTangents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets editor's data.
        /// PointEditorData is used by the editor and
        /// does not need to be persisted.</summary>
        PointEditorData EditorData
        {
            get;
        }

        /// <summary>
        /// Clones this control point</summary>
        /// <returns>Cloned control point</returns>
        IControlPoint Clone();
    }

    /// <summary>
    /// Class that contains runtime data used by a curve editor.
    /// This class contains all the additional properties 
    /// for control point that are strictly 
    /// used by a curve editor.</summary>
    public class PointEditorData
    {
        /// <summary>
        /// Point region types</summary>
        public PointSelectionRegions SelectedRegion = PointSelectionRegions.None;
    }
}
