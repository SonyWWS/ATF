//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Interface for a service that provides network support</summary>
    public interface ITargetService
    {
        /// <summary>
        /// Gets an array of all targets. Each target provides the name, IP address, and port
        /// that can be used to connect.</summary>
        /// <returns>Array of all targets</returns>
        Target[] GetAllTargets();

        /// <summary>
        /// Gets an array of all selected targets. Each target provides the name, IP address,
        /// and port that can be used to connect.</summary>
        /// <returns>Array of all selected targets</returns>
        Target[] GetSelectedTargets();

        /// <summary>
        /// Gets the selected target</summary>
        /// <returns>Selected target</returns>
        Target GetSelectedTarget();

        /// <summary>
        /// Shows the target dialog</summary>
        /// <returns>Target dialog result</returns>
        DialogResult ShowTargetDialog();

        /// <summary>
        /// Sets the list of supported protocols</summary>
        /// <param name="protocols">Array of supported protocols</param>
        void SetProtocols(string[] protocols);

        /// <summary>
        /// Selects a target, given its name.
        /// An exception is thrown if the parameter is invalid or the target is not found.</summary>
        /// <param name="name">Name of the target to be selected</param>
        void SelectTarget(string name);

        /// <summary>
        /// Adds a new target. If the target already exists, an exception is thrown.</summary>
        /// <param name="name">Name of the target machine</param>
        /// <param name="host">Host name or IP address</param>
        /// <param name="port">Port number</param>
        void AddTarget(string name, string host, int port);

        /// <summary>
        /// Gets or sets the single selection mode</summary>
        /// <remarks>Default value is true</remarks>
        bool SingleSelectionMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default port number. If set, the default port is pre-filled in the
        /// "Add New Target" dialog box.</summary>
        int DefaultPortNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Enable/Disable if the user can edit the port number</summary>
        /// <remarks>Default value is true</remarks>
        bool CanEditPortNumber
        {
            get;
            set;
        }
    }
}
