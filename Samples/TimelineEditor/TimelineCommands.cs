//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;

namespace TimelineEditorSample
{
    /// <summary>
    /// Command component that defines Timeline-specific commands. All of these are
    /// popup commands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(TimelineCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TimelineCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public TimelineCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering timeline commands</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(                
                Command.RemoveGroup,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Remove/Group",
                "Removes the Group",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(                
                Command.RemoveTrack,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Remove/Track",
                "Removes the track",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(                
                Command.RemoveEmptyGroupsAndTracks,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Remove/Empty Groups and Tracks",
                "Removes empty Groups and Tracks",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(                
                Command.ToggleSplitMode,
                StandardMenu.Edit,
                null,
                "Interval Splitting Mode",
                "Toggles the interval splitting mode",
                Keys.S,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(StandardCommand.ViewZoomExtents, CommandVisibility.All, this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return DoCommand(commandTag, false);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            DoCommand(commandTag, true);
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState)
        {
            TimelineDocument document = m_contextRegistry.GetActiveContext<TimelineDocument>();
            if (document == null)
                return;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.ToggleSplitMode:
                        commandState.Check = document.SplitManipulator != null ? document.SplitManipulator.Active : false;
                        break;
                }
            }
        }

        #endregion

        private bool DoCommand(object commandTag, bool doing)
        {
            TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
            if (context == null)
                return false;

            TimelineDocument document = context.As<TimelineDocument>();
            if (document == null)
                return false;

            if (commandTag is Command)
            {
                if ((Command)commandTag == Command.ToggleSplitMode)
                {
                    if (doing && document.SplitManipulator != null)
                        document.SplitManipulator.Active = !document.SplitManipulator.Active;
                    return true;
                }

                ITimelineObject target = m_contextRegistry.GetCommandTarget<ITimelineObject>();
                if (target == null)
                    return false;

                IInterval activeInterval = target as IInterval;
                ITrack activeTrack =
                    (activeInterval != null) ? activeInterval.Track : target.As<ITrack>();
                IGroup activeGroup =
                    (activeTrack != null) ? activeTrack.Group : target.As<IGroup>();
                ITimeline activeTimeline =
                    (activeGroup != null) ? activeGroup.Timeline : target.As<ITimeline>();
                ITransactionContext transactionContext = context.TimelineControl.TransactionContext;

                switch ((Command)commandTag)
                {
                    case Command.RemoveGroup:
                        if (activeGroup == null)
                            return false;

                        if (doing)
                        {
                            transactionContext.DoTransaction(delegate
                            {
                                activeGroup.Timeline.Groups.Remove(activeGroup);
                            },
                            "Remove Group");
                        }
                        return true;

                    case Command.RemoveTrack:
                        if (activeTrack == null)
                            return false;

                        if (doing)
                        {
                            transactionContext.DoTransaction(delegate
                            {
                                activeTrack.Group.Tracks.Remove(activeTrack);
                            },
                            "Remove Track");
                        }
                        return true;

                    case Command.RemoveEmptyGroupsAndTracks:
                        if (activeTimeline == null)
                            return false;

                        if (doing)
                        {
                            transactionContext.DoTransaction(delegate
                            {
                                IList<IGroup> groups = activeTimeline.Groups;
                                for (int i = 0; i < groups.Count; )
                                {
                                    IList<ITrack> tracks = groups[i].Tracks;
                                    for (int j = 0; j < tracks.Count; )
                                    {
                                        if (tracks[j].Intervals.Count == 0 && tracks[j].Keys.Count == 0)
                                            tracks.RemoveAt(j);
                                        else
                                            j++;
                                    }

                                    if (tracks.Count == 0)
                                        groups.RemoveAt(i);
                                    else
                                        i++;
                                }
                            },
                            "Remove Empty Groups and Tracks");
                        }
                        return true;
                }
            }
            else if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewZoomExtents:
                        if (doing)
                        {
                            document.TimelineControl.Frame();
                        }
                        return true;
                }
            }

            return false;
        }

        private enum Command
        {
            RemoveGroup,
            RemoveTrack,
            RemoveEmptyGroupsAndTracks,
            ToggleSplitMode,
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
    }
}
