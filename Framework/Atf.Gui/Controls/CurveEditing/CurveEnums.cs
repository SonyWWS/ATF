namespace Sce.Atf.Controls.CurveEditing
{
    // Enums used by Curve interfaces and curve control
       
    /// <summary>
    /// Curve loop types</summary>
    public enum CurveLoopTypes
    {
        /// <summary>Constant</summary>
        Constant = 0,

        /// <summary>Cycle</summary>
        Cycle,

        /// <summary>CycleWithOffset</summary>
        CycleWithOffset,

        /// <summary>Oscillate</summary>
        Oscillate,

        /// <summary>Linear</summary>
        Linear,
    }
   
    /// <summary>
    /// Tangent types used by curve editor</summary>
    public enum CurveTangentTypes
    {
        /// <summary>Spline</summary>
        Spline = 0,

        /// <summary>Linear</summary>
        Linear,

        /// <summary>Clamped</summary>
        Clamped,

        /// <summary>Stepped</summary>
        Stepped,

        /// <summary>Stepped Next</summary>
        SteppedNext,

        /// <summary>Flat</summary>                
        Flat,

        /// <summary>Fixed</summary>
        Fixed,

        /// <summary>Plateau</summary>
        Plateau,
    }
    
    /// <summary>
    /// Point region types</summary>
    public enum PointSelectionRegions
    {
        /// <summary>None</summary>
        None = 0,

        /// <summary>Tangent in selected</summary>
        TangentIn = 1,

        /// <summary>Tangent out selected</summary>
        TangentOut = 2,

        /// <summary>Control point selected</summary>
        Point = 4,        
    }

    /// <summary>
    /// Curve type</summary>
    public enum InterpolationTypes
    {
        /// <summary>
        /// Undefined interpolation type</summary>
        /// <remarks>Added by Guerrilla and promoted to ATF</remarks>
        None = 0,
        
        /// <summary>
        /// Cubic hermite spline aka cspline</summary>
        Hermite,

        /// <summary>
        /// Linear curve</summary>
        Linear,
    }
}
