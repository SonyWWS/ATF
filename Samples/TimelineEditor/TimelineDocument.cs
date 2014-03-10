//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Dom;
using MoveManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dMoveManipulator;
using ScaleManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dScaleManipulator;
using ScrubberManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dScrubberManipulator;
using SelectionManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dSelectionManipulator;
using SnapManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dSnapManipulator;
using SplitManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dSplitManipulator;
using TimelineControl = Sce.Atf.Controls.Timelines.Direct2D.D2dTimelineControl;
using TimelineRenderer = Sce.Atf.Controls.Timelines.Direct2D.D2dTimelineRenderer;

namespace TimelineEditorSample
{
    /// <summary>
    /// Timeline document, as a DOM hierarchy and identified by an URI. Each TimelineControl has one
    /// or more TimelineDocuments.</summary>
    public class TimelineDocument : DomDocument, ITimelineDocument, IPrintableDocument, IObservableContext
    {
        /// <summary>
        /// Parameterless constructor for DomNodeAdapters. Set Renderer before using this class.</summary>
        public TimelineDocument()
        {
        }

        /// <summary>
        /// Constructor taking a timeline renderer</summary>
        public TimelineDocument(TimelineRenderer timelineRenderer)
        {
            Renderer = timelineRenderer;
        }

        /// <summary>
        /// Gets the timeline control for editing the document</summary>
        public TimelineControl TimelineControl
        {
            get { return m_timelineControl; }
        }

        /// <summary>
        /// Gets the document's root timeline</summary>
        public ITimeline Timeline
        {
            get { return DomNode.As<ITimeline>(); }
        }

        /// <summary>
        /// Gets or sets the document's timeline renderer</summary>
        public TimelineRenderer Renderer
        {
            get { return m_renderer; }
            set
            {
                if (m_renderer != null)
                    throw new InvalidOperationException("The timeline renderer can only be set once");
                m_renderer = value;

                // Due to recursion, we need m_timelineControl to be valid before m_timelineControl.TimelineDocument is set.
                // So, we pass in 'null' into TimelineControl's constructor.
                m_timelineControl = new TimelineControl(null, m_renderer, new TimelineConstraints(), false);
                m_timelineControl.TimelineDocument = this;

                m_timelineControl.SetZoomRange(0.1f, 50f, 1f, 100f);
                AttachManipulators();
            }
        }

        /// <summary>
        /// Gets an enumeration of all editing contexts in the document</summary>
        public IEnumerable<EditingContext> EditingContexts
        {
            get
            {
                yield return DomNode.As<EditingContext>();
            }
        }

        #region IResource Members

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return Localizer.Localize("Timeline"); }
        }

        #endregion

        #region IPrintableDocument Implementation

        /// <summary>
        /// Gets a PrintDocument to work with the standard Windows print dialogs</summary>
        /// <returns>PrintDocument to work with the standard Windows print dialogs</returns>
        public PrintDocument GetPrintDocument()
        {
            // static allocation, to remember print settings
            if (s_printDocument == null)
                s_printDocument = new TimelinePrintDocument();

            s_printDocument.SetControl(TimelineControl);
            return s_printDocument;
        }

        private static TimelinePrintDocument s_printDocument;

        private class TimelinePrintDocument : Sce.Atf.Applications.CanvasPrintDocument
        {
            internal void SetControl(TimelineControl timelineControl)
            {
                m_timelineControl = timelineControl;
                SetDefaultPrinterSettings();
            }

            protected override RectangleF GetSelectionBounds()
            {
                RectangleF result = m_timelineControl.GetSelectionBoundingRect();
                result = GdiUtil.InverseTransform(m_timelineControl.Transform, result);
                return result;
            }

            protected override RectangleF GetAllPagesBounds()
            {
                RectangleF result = m_timelineControl.GetBoundingRect();
                result = GdiUtil.InverseTransform(m_timelineControl.Transform, result);
                return result;
            }

            protected override RectangleF GetCurrentPageBounds()
            {
                TimelineLayout layout = m_timelineControl.GetLayout();
                RectangleF visibleBounds = GdiUtil.InverseTransform(m_timelineControl.Transform, m_timelineControl.ClientRectangle);
                RectangleF result = new RectangleF();
                bool firstTime = true;
                foreach (KeyValuePair<TimelinePath, RectangleF> pair in layout)
                {
                    if (pair.Value.IntersectsWith(visibleBounds))
                    {
                        if (firstTime)
                        {
                            firstTime = false;
                            result = pair.Value;
                        }
                        else
                        {
                            result = RectangleF.Union(result, pair.Value);
                        }
                    }
                }
                return result;
            }

            protected override bool AllowNonUniformScale
            {
                get { return true; }
            }

            protected override void Render(RectangleF sourceBounds, Matrix transform, Graphics g)
            {
                //g.SmoothingMode = SmoothingMode.AntiAlias;
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                m_timelineControl.Renderer.Print(
                    m_timelineControl.TimelineDocument.Timeline,
                    new SelectionContext(),
                    null,
                    null,
                    m_timelineControl.Transform,
                    m_timelineControl.ClientRectangle,
                    MarginBounds);
            }

            private TimelineControl m_timelineControl;
        }

        #endregion

        /// <summary>
        /// Attaches manipulators to the TimelineControl to provide additional capabilities</summary>
        protected virtual void AttachManipulators()
        {
            // The order here determines the order of receiving Paint events and is the reverse
            //  order of receiving picking events. For example, a custom Control that is drawn
            //  on top of everything else and that can be clicked on should come last in this
            //  list so that it is drawn last and is picked first.
            D2dSelectionManipulator selectionManipulator = new D2dSelectionManipulator(m_timelineControl);
            D2dMoveManipulator moveManipulator = new D2dMoveManipulator(m_timelineControl);
            D2dScaleManipulator scaleManipulator = new D2dScaleManipulator(m_timelineControl);
            m_splitManipulator = new D2dSplitManipulator(m_timelineControl);
            D2dSnapManipulator snapManipulator = new D2dSnapManipulator(m_timelineControl);
            D2dScrubberManipulator scrubberManipulator = new ScrubberManipulator(m_timelineControl);

            //// Allow the snap manipulator to snap objects to the scrubber.
            snapManipulator.Scrubber = scrubberManipulator;
        }

        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnDirtyChanged(EventArgs e)
        {
            UpdateControlInfo();

            base.OnDirtyChanged(e);
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">UriChangedEventArgs containing event data</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        private void UpdateControlInfo()
        {
            TimelineContext context = this.As<TimelineContext>();

            string filePath;
            if (Uri.IsAbsoluteUri)
                filePath = Uri.LocalPath;
            else
                filePath = Uri.OriginalString;

            string fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            context.ControlInfo.Name = fileName;
            context.ControlInfo.Description = filePath;
        }

        /// <summary>
        /// Gets the SplitManipulator that was attached to the TimelineControl</summary>
        public SplitManipulator SplitManipulator
        {
            get { return m_splitManipulator; }
        }

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when an item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted
        {
            add { this.As<TimelineContext>().ItemInserted += value; }
            remove { this.As<TimelineContext>().ItemInserted -= value; }
        }

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
        {
            add { this.As<TimelineContext>().ItemRemoved += value; }
            remove { this.As<TimelineContext>().ItemRemoved -= value; }
        }

        /// <summary>
        /// Event that is raised when an item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
        {
            add { this.As<TimelineContext>().ItemChanged += value; }
            remove { this.As<TimelineContext>().ItemChanged -= value; }
        }

        /// <summary>
        /// Event that is raised when the collection has been reloaded</summary>
        public event EventHandler Reloaded
        {
            add { this.As<TimelineContext>().Reloaded += value; }
            remove { this.As<TimelineContext>().Reloaded -= value; }
        }

        #endregion

        private TimelineControl m_timelineControl;
        private TimelineRenderer m_renderer;
        private SplitManipulator m_splitManipulator;
    }
}
