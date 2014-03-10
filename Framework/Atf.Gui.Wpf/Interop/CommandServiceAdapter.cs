//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using Sce.Atf.Wpf.Applications;

#pragma warning disable 0067 // Event never used

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Presents commands in menu and toolbar controls.
    /// This class adapts Sce.Atf.Wpf.Applications.ICommandService to Sce.Atf.Applications.CommandService. 
    /// This allows legacy code to be run in a WPF based application.</summary>
    [Export(typeof(Sce.Atf.Applications.ICommandService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandServiceAdapter : Sce.Atf.Applications.ICommandService
    {
        /// <summary>
        /// Constructor</summary>
        public CommandServiceAdapter()
        {
            ComponentDispatcher.ThreadIdle += new EventHandler(ComponentDispatcher_ThreadIdle);
        }

        [Import(AllowDefault = true)]
        private IContextMenuService m_contextMenuService = null;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;

        #region ICommandService Members

        /// <summary>
        /// Registers a menu for the application. NOTE: MenuInfo.MenuItem and MenuInfo.ToolStrip will not be valid.</summary>
        /// <param name="info">Menu description; standard menus are defined as static members on the MenuInfo class</param>
        public void RegisterMenu(Sce.Atf.Applications.MenuInfo info)
        {
            if (m_commandService == null)
                throw new InvalidOperationException("ICommandService not found");

            m_commandService.RegisterMenu(new MenuDef(info.MenuTag, info.MenuText, info.Description));
        }

        /// <summary>
        /// Registers a command for a command client.
        /// NOTE: CommandInfo.MenuItem and CommandInfo.Button will not be valid.
        /// Shortcut related properties and methods on CommandInfo will have no effect.</summary>
        /// <param name="info">Command description; standard commands are defined as static
        /// members on the CommandInfo class</param>
        /// <param name="client">Client that handles the command</param>
        public void RegisterCommand(Sce.Atf.Applications.CommandInfo info, Sce.Atf.Applications.ICommandClient client)
        {
            // Embedded image resources will not be available as WPF app resources
            // If image resource does not exist we need to create it and add it to app resources
            object imageResourceKey = null;
            if (!string.IsNullOrEmpty(info.ImageName))
            {
                var embeddedImage = ResourceUtil.GetImage(info.ImageName);
                if (embeddedImage == null)
                    throw new InvalidOperationException("Could not find embedded image: " + info.ImageName);

                Util.GetOrCreateResourceForEmbeddedImage(embeddedImage);
                imageResourceKey = embeddedImage;
            }

            // Convert text and path
            string displayText = GetDisplayMenuText(info.MenuText);
            info.DisplayedMenuText = displayText;

            string[] menuPath = GetMenuPath(info.MenuText);

            // Convert shortcuts
            var inputGestures = new List<InputGesture>();
            foreach (var formsKey in info.Shortcuts)
                inputGestures.Add(Util.ConvertKey(formsKey));

            // Create and register command passing this as command client
            var def = new CommandDef(
                info.CommandTag, 
                info.MenuTag, 
                info.GroupTag, 
                displayText,
                menuPath,
                info.Description,
                imageResourceKey,
                inputGestures.ToArray<InputGesture>(),
                info.Visibility);

            var clientAdapter = GetOrCreateClientAdapter(client);

            var command = m_commandService.RegisterCommand(def, clientAdapter);

            clientAdapter.AddCommand(command);
        }

        /// <summary>
        /// Unregisters a command for a command client</summary>
        /// <param name="commandTag">Command tag that identifies CommandInfo used to register the command</param>
        /// <param name="client">Client that handles the command</param>
        public void UnregisterCommand(object commandTag, Sce.Atf.Applications.ICommandClient client)
        {
            // TODO: is this sequence of operations thread safe?
            var clientAdapter = GetOrCreateClientAdapter(client);
            var command = clientAdapter.RemoveCommand(commandTag);
            
            if (command != null)
            {
                m_commandService.UnregisterCommand(command, clientAdapter);
            }
        }

        /// <summary>
        /// Displays a context menu at the point, in screen coordinates</summary>
        /// <param name="commandTags">Commands to display in menu</param>
        /// <param name="screenPoint">Point, in screen coordinates, of menu top left corner</param>
        public void RunContextMenu(IEnumerable<object> commandTags, System.Drawing.Point screenPoint)
        {
            if (m_contextMenuService == null)
                throw new InvalidOperationException("IContextMenuService not found");

            // Convert screenPoint to device independent pixels
            // TODO: could move this into a Util class
            var source = PresentationSource.FromVisual(Application.Current.MainWindow);

            var transformFromDevice = source.CompositionTarget.TransformFromDevice;

            int x = (int)(screenPoint.X * transformFromDevice.M11);
            int y = (int)(screenPoint.Y * transformFromDevice.M22);

            m_contextMenuService.RunContextMenu(commandTags, new Point(x, y));
        }

        /// <summary>
        /// Sets the active client that receives a command for the case when multiple
        /// ICommandClient objects have registered for the same command tag (such as the
        /// StandardCommand.EditCopy enum, for example). Set to null to reduce the priority
        /// of the previously active client.</summary>
        /// <param name="client">Command client, null if client is deactivated</param>
        public void SetActiveClient(Sce.Atf.Applications.ICommandClient client)
        {
            var clientAdapter = GetOrCreateClientAdapter(client);
            m_commandService.SetActiveClient(clientAdapter);
        }

        /// <summary>
        /// Reserves a shortcut key, so it is not available as a command shortcut</summary>
        /// <param name="key">Reserved key</param>
        /// <param name="reason">Reason why key is reserved, to display to user</param>
        public void ReserveKey(Sce.Atf.Input.Keys key, string reason)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to process the key as a command shortcut</summary>
        /// <param name="key">Key to process</param>
        /// <returns>True iff the key was processed as a command shortcut</returns>
        public bool ProcessKey(Sce.Atf.Input.Keys key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event that is raised when processing a key; clients can subscribe to this event
        /// to intercept certain hot keys for custom handling</summary>
        public event EventHandler<Sce.Atf.Input.KeyEventArgs> ProcessingKey;

        #endregion

        private CommandClientAdapter GetOrCreateClientAdapter(Sce.Atf.Applications.ICommandClient client)
        {
            CommandClientAdapter adapter;
            if (!m_clientAdapters.TryGetValue(client, out adapter))
            {
                adapter = new CommandClientAdapter(client);
                lock (m_clientAdapters)
                {
                    m_clientAdapters.Add(client, adapter);
                }
            }
            return adapter;
        }

        private void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            lock (m_clientAdapters)
            {
                foreach (var adapter in m_clientAdapters.Values)
                    adapter.UpdateCommands();
            }
        }

        private static string GetDisplayMenuText(string menuText)
        {
            Requires.NotNullOrEmpty(menuText, "menuText");

            int textStart = 1;
            // for non-literal menu text, get last segment of path
            if (menuText[0] != '@')
            {
                // a little subtle here, if there's no separator, -1 bumps textStart back to 0
                textStart += menuText.LastIndexOfAny(s_pathDelimiters);
            }

            return menuText.Substring(textStart, menuText.Length - textStart);
        }

        private static string[] GetMenuPath(string menuText)
        {
            Requires.NotNullOrEmpty(menuText, "menuText");

            if (menuText[0] == '@')
            {
                // TODO: not tested
                return new string[] { menuText.Substring(1, menuText.Length - 1) };
            }
            else
            {
                var segments = menuText.Split(s_pathDelimiters);
                if (segments.Length > 1)
                {
                    string[] result = new string[segments.Length-1];
                    Array.Copy(segments, result, segments.Length-1);
                    return result;
                }
            }
            return null;
        }

        private static char[] s_pathDelimiters = new char[] { '/', '\\' };

        private Dictionary<Sce.Atf.Applications.ICommandClient, CommandClientAdapter> m_clientAdapters
            = new Dictionary<Sce.Atf.Applications.ICommandClient, CommandClientAdapter>();

    }
}
