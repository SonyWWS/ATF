//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Base class to describe target information, contains parameters that define a target</summary>
    public abstract class TargetInfo: INotifyPropertyChanged
    {
        /// <summary>
        /// Get or set the name of the target</summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Get or set the type of protocol the target can use</summary>
        public string Platform
        {
            get { return m_platform; }
            set
            {
                if (m_platform != value)
                {
                    m_platform = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Platform"));
                }
            }
        }

        /// <summary>
        /// Get or set the network endpoint in string format</summary>
        public string Endpoint
        {
            get { return m_endPoint; }
            set
            {
                if (m_endPoint != value)
                {
                    m_endPoint = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Endpoint"));
                }
            }
        }

        /// <summary>
        /// Get or set the type of protocol the target can use</summary>
        public string Protocol
        {
            get { return m_protocol; }
            set
            {
                if (m_protocol != value)
                {
                    m_protocol = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Protocol"));
                }
            }
        }

        /// <summary>
        /// Get or set whether the target is persisted for the current application only, current user, 
        /// or all users on a machine</summary>
        public TargetScope Scope
        {
            get { return m_scope; }
            set
            {
                if (m_scope != value)
                {
                    m_scope = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Scope"));
                }
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Property changed event</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Performs actions when property changed event occurs</summary>
        /// <param name="e">Event args</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }


        /// <summary>
        /// Creates string representation of target</summary>
        /// <returns>String representation of target</returns>
        public override string ToString()
        {
            return string.Format("Name: {0}, Platform: {1}, Endpoint: {2}, Protocol: {3}, Scope: {4}", Name, Platform, Endpoint, Protocol, Scope);
        }

        private string m_name;
        private string m_platform;
        private string m_endPoint;
        private string m_protocol;
        private TargetScope m_scope;
    }

    

  

}
