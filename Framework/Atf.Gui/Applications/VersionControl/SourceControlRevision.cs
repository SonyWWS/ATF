//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Source control revison</summary>
    public class SourceControlRevision
    {
        /// <summary>
        /// Constructor for revision explicitly identified by its revision number</summary>
        /// <param name="revisionNumber">Revison number</param>
        public SourceControlRevision(int revisionNumber)
        {
            m_kind = SourceControlRevisionKind.Number;
            m_revisionNumber = revisionNumber;
        }

        /// <summary>
        /// Unspecified revision constructor</summary>       
        public SourceControlRevision()
        {
            m_kind = SourceControlRevisionKind.Unspecified;
        }

        /// <summary>
        /// Constructor for revision implicitly identified by a specific point in time</summary>
        /// <param name="referenceDate">Revision date</param>
        public SourceControlRevision(DateTime referenceDate)
        {
            m_kind = SourceControlRevisionKind.Date;
            m_dateTime = referenceDate;
        }

        /// <summary>
        /// Constructor for a kind of revision</summary>
        /// <param name="kind">The revision kind. See <see cref="SourceControlRevisionKind"/></param> 
        public SourceControlRevision(SourceControlRevisionKind kind)
        {
            m_kind = kind;
        }

        /// <summary>
        /// Gets or sets the enum indicating how the revision is formatted or what
        /// kind of revision this is</summary>
        public SourceControlRevisionKind Kind
        {
            get { return m_kind; }
            set { m_kind = value; }
        }

        /// <summary>
        /// Gets or sets the revision as a number. Note that the 'get' only works
        /// if the revision was previously stored as a number and the Kind property
        /// is SourceControlRevisionKind.Number.</summary>
        public int Number
        {
            get
            {
                if (m_kind != SourceControlRevisionKind.Number)
                    throw new InvalidOperationException("This revision is not a Number");
                return m_revisionNumber;
            }
            set
            {
                m_kind = SourceControlRevisionKind.Number;
                m_revisionNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the revision as a date. Note that the 'get' only works
        /// if the revision was previously stored as a date and the Kind property
        /// is SourceControlRevisionKind.Date.</summary>
        public DateTime Date
        {
            get
            {
                if (m_kind != SourceControlRevisionKind.Date)
                    throw new InvalidOperationException("This revision is not a Date");
                return m_dateTime;
            }
            set
            {
                m_kind = SourceControlRevisionKind.Date;
                m_dateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the change list number. Note that the 'get' only works
        /// if the revision was previously stored as a change list and the Kind property
        /// is SourceControlRevisionKind.ChangeList.</summary>
        public int ChangeListNumber
        {
            get
            {
                if (m_kind != SourceControlRevisionKind.ChangeList)
                    throw new InvalidOperationException("This revision is not a changelist number");
                return m_revisionNumber;
            }
            set
            {
                m_kind = SourceControlRevisionKind.ChangeList;
                m_revisionNumber = value;
            }
        }

        /// <summary>
        /// Gets a new SourceControlRevision object to represent the head revision</summary>
        public static SourceControlRevision Head
        {
            get { return new SourceControlRevision(SourceControlRevisionKind.Head); }
        }

        /// <summary>
        /// Gets a new SourceControlRevision object to represent an unspecified kind of revision</summary>
        public static SourceControlRevision Unspecified
        {
            get { return new SourceControlRevision(SourceControlRevisionKind.Unspecified); }
        }

        /// <summary>
        /// Gets a new SourceControlRevision object to represent the working revision</summary>
        public static SourceControlRevision Working
        {
            get { return new SourceControlRevision(SourceControlRevisionKind.Working); }
        }

        /// <summary>
        /// Gets a new SourceControlRevision object to represent the base revision</summary>
        public static SourceControlRevision Base
        {
            get { return new SourceControlRevision(SourceControlRevisionKind.Base); }
        }

        private SourceControlRevisionKind m_kind;
        private int m_revisionNumber;
        private DateTime m_dateTime;
    }
}
