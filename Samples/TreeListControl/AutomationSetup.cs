//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Timers;
using System.Windows.Forms;

using Sce.Atf;

namespace TreeListControlDemo
{
    /// <summary>
    /// Set up a timer to automatically close the main form, hence the sample app</summary>
    /// <remarks>Since this sample app, unlike other ATF sample apps,  does not instantiate  
    /// AutomationService or any other standard ATF components, we need to shutdown the app 
	/// by closing the main form for automated functional tests
    /// </remarks>
    internal class AutomationSetup
    {
        public AutomationSetup(Form mainForm)
        {
            m_maiForm = mainForm;
            string[] args = Environment.GetCommandLineArgs();
            bool automation = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower().CompareTo("-automation") == 0)
                {
                    // running automated tests
                    automation = true;
                    break;
                }
            }

            if (automation)
            {
                var shutDownTimer = new System.Timers.Timer(1500);
                shutDownTimer.Elapsed += ShutDownTimer_Elapsed;
                shutDownTimer.AutoReset = false;
                shutDownTimer.Enabled = true;
            }
        }

        private  void ShutDownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            WinFormsUtil.InvokeIfRequired(m_maiForm, () => m_maiForm.Close());
        }

        private Form m_maiForm;
    }
}
