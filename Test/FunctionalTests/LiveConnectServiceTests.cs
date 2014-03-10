// The ATF 3 LevelEditor has been removed. Until we have another sample app that uses LiveConnect, we have to disable these tests.
/*
//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Sce.Atf.Applications;

namespace FunctionalTests
{
    [TestFixture]
    public class LiveConnectServiceTests : TestBase
    {
        public LiveConnectServiceTests()
        {
            m_processes = new List<Process>();
        }

        protected override string GetAppName()
        {
            return "LiveConnectService";
        }

        [TearDown]
        public override void TearDown()
        {
            foreach (Process p in m_processes)
            {
                if (p != null)
                {
                    try
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                    }
                    catch { }
                }
            }
        }

        private void EnsureLiveConnectIsReady(AutomationService automationService)
        {
            // It seems to take a while for the LiveConnectService's native dll or one of its
            //  dependencies (Bonjour?) to be ready. 500ms wasn't always enough. Until we know
            //  what is going on, let's just wait. :-(
            Thread.Sleep(1000);
        }

        private AutomationService LaunchLevelEditor()
        {
            const string appPath = @".\LevelEditor.exe";
            int port;
            Process p = LaunchTestApplication(appPath, out port);
            m_processes.Add(p);

            AutomationService automationService = Connect(port);
            EnsureLiveConnectIsReady(automationService);
            return automationService;
        }

        private AutomationService LaunchTimelineEditor()
        {
            const string appPath = @".\TimelineEditor.exe";
            int port;
            Process p = LaunchTestApplication(appPath, out port);
            m_processes.Add(p);

            AutomationService automationService = Connect(port);
            EnsureLiveConnectIsReady(automationService);
            return automationService;
        }

        private void VerifyLastMessage(AutomationService service, string expected)
        {
            string actual = null;
            int cnt = 0;
            do
            {
                Thread.Sleep(50);
                actual = service.GetLastMessage();
            } while (actual != expected && cnt++ < 50);

            Console.WriteLine("Last message={0}, loop={1}", actual, cnt);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SendMessageBetweenTimelineEditorAndLevelEditor()
        {
            AutomationService levelEditorService = LaunchLevelEditor();
            AutomationService timelineEditorService = LaunchTimelineEditor();

            //Test LevelEditor -> TimelineEditor
            string msg = Guid.NewGuid().ToString();
            levelEditorService.SendMessage(msg);
            VerifyLastMessage(levelEditorService, msg);
            VerifyLastMessage(timelineEditorService, msg);

            //Test TimelineEditor -> LevelEditor
            msg = Guid.NewGuid().ToString();
            timelineEditorService.SendMessage(msg);
            VerifyLastMessage(timelineEditorService, msg);
            VerifyLastMessage(levelEditorService, msg);
        }

        [Test]
        public void SendMessageBetweenMultipleInstances()
        {
            AutomationService levelEditorService = LaunchLevelEditor();
            AutomationService timelineEditorService = LaunchTimelineEditor();
            AutomationService levelEditorService2 = LaunchLevelEditor();
            AutomationService timelineEditorService2 = LaunchTimelineEditor();

            List<AutomationService> allServices = new List<AutomationService>();
            allServices.Add(levelEditorService);
            allServices.Add(levelEditorService2);
            allServices.Add(timelineEditorService);
            allServices.Add(timelineEditorService2);

            string msg = Guid.NewGuid().ToString();
            levelEditorService.SendMessage(msg);
            foreach (AutomationService service in allServices)
            {
                VerifyLastMessage(service, msg);
            }

            msg = Guid.NewGuid().ToString();
            levelEditorService2.SendMessage(msg);
            foreach (AutomationService service in allServices)
            {
                VerifyLastMessage(service, msg);
            }

            msg = Guid.NewGuid().ToString();
            timelineEditorService.SendMessage(msg);
            foreach (AutomationService service in allServices)
            {
                VerifyLastMessage(service, msg);
            }

            msg = Guid.NewGuid().ToString();
            timelineEditorService2.SendMessage(msg);
            foreach (AutomationService service in allServices)
            {
                VerifyLastMessage(service, msg);
            }
        }

        [Test]
        public void SpamMessagesBetweenTimelineEditorAndLevelEditor()
        {
            AutomationService levelEditorService = LaunchLevelEditor();
            AutomationService timelineEditorService = LaunchTimelineEditor();

            //Test LevelEditor -> TimelineEditor
            string msg = null;
            for (int i = 0; i < 1000; i++)
            {
                msg = string.Format("From level editor loop#{0}", i);
                levelEditorService.SendMessage(msg);
                msg = string.Format("From timeline editor loop#{0}", i);
                timelineEditorService.SendMessage(msg);
            }

            //No guarantee the messages will be in order, so pause for a second
            Thread.Sleep(1000);

            //Now send a new message, and verify it goes through
            //Test TimelineEditor -> LevelEditor
            msg = Guid.NewGuid().ToString();
            timelineEditorService.SendMessage(msg);
            VerifyLastMessage(timelineEditorService, msg);
            VerifyLastMessage(levelEditorService, msg);

            //Test LevelEditor -> TimelineEditor
            msg = Guid.NewGuid().ToString();
            levelEditorService.SendMessage(msg);
            VerifyLastMessage(levelEditorService, msg);
            VerifyLastMessage(timelineEditorService, msg);
        }

        private List<Process> m_processes;
    }
}
*/