namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Interface for evaluating curves, i.e., calculating y-coordinate from x-coordinate using appropriate interpolation for a curve</summary>
    public interface ICurveEvaluator
    {       
        /// <summary>        
        /// Resets the curve. Call this whenever curve changes.</summary>
        void Reset();

        /// <summary>
        /// Evaluates point on curve</summary>
        /// <param name="x">X-coordinate for which y-coordinate is calculated</param>
        /// <remarks>Calculates y-coordinate from x-coordinate using appropriate interpolation for a curve</remarks>
        float Evaluate(float x);        
    }
}
