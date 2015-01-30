//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

using TimelineEditorSample.DomNodeAdapters;

namespace TimelineEditorSample
{
    /// <summary>
    /// Timeline validator, ensuring that:
    ///     1) events have integral starting frames
    ///     2) intervals have integral lengths</summary>
    public class TimelineValidator : Validator
    {
        /// <summary>
        /// Performs actions after attribute changed to validate events and intervals</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">AttributeEventArgs containing event data</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            BaseEvent _event = e.DomNode.As<BaseEvent>();
            if (_event != null)
            {
                if (e.AttributeInfo.Equivalent(Schema.eventType.startAttribute))
                {
                    float value = (float)e.NewValue;
                    float constrained = Math.Max(value, 0);                 // >= 0
                    constrained = (float)MathUtil.Snap(constrained, 1.0);   // snapped to nearest integral frame number
                    if (constrained != value)
                        throw new InvalidTransactionException("Timeline events must have a positive integer start time".Localize());
                    return;
                }

                Interval interval = _event.As<Interval>();
                if (interval != null)
                {
                    if (e.AttributeInfo.Equivalent(Schema.intervalType.lengthAttribute))
                    {
                        float value = (float)e.NewValue;
                        float constrained = Math.Max(value, 1);                 // >= 1
                        constrained = (float)MathUtil.Snap(constrained, 1.0);   // snapped to nearest integral frame number
                        if (constrained != value)
                            throw new InvalidTransactionException("Timeline intervals must have an integer length".Localize());
                        return;
                    }
                }
            }
        }
    }
}
