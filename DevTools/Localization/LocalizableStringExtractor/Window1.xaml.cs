//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LocalizableStringExtractor
{
    /// <summary>
    /// GUI plus command-line parsing.</summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            // Bind the ProgressBar.Value to Extractor.Progress.
            var progressBar = (ProgressBar)this.FindName("ExtractionProgressBar");
            var binding = new Binding();
            binding.Source = m_extractor;
            binding.Mode = BindingMode.OneWay;
            binding.Path = new PropertyPath("Progress");
            progressBar.SetBinding(ProgressBar.ValueProperty, binding);

            // Check command-line arguments. Skip args[0] which is the executable name.
            string[] args = Environment.GetCommandLineArgs();
            bool printHelp = false;
            for(int i = 1; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-s")
                {
                    i++;
                    if (i < args.Length)
                        m_settingsPath = args[i];
                }
                else if (arg == "-auto")
                {
                    m_autoMode = true;
                }
                else
                    printHelp = true;
            }

            if (printHelp)
            {
                Console.WriteLine(
@"Command line options:
  -s {path}  Uses the given path to the settings file instead
              of using the default DirectoriesToLocalize.txt.
  -auto      Automatically does the processing without user input
              and outputs the log file to the command line.
  -help      Shows the command line options.
");
                Application.Current.Shutdown(-1);
                return;
            }

            if (!File.Exists(m_settingsPath))
            {
                string message = "The settings file could not be found: " + m_settingsPath;
                if (m_autoMode)
                    Console.WriteLine(message);
                else
                    MessageBox.Show(message);
                Application.Current.Shutdown(-2);
                return;
            }

            if (m_autoMode)
            {
                ExtractStrings();
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            ExtractStrings();
        }

        private void ExtractStrings()
        {
            CancelBtn.IsEnabled = true;
            StartBtn.IsEnabled = false;
            DirectoriesBtn.IsEnabled = false; 

            m_backgroundWorker = new BackgroundWorker();
            m_backgroundWorker.WorkerSupportsCancellation = true;

            m_backgroundWorker.DoWork += (o, args) =>
            {
                m_extractor.ExtractAll(m_settingsPath);
            };

            m_backgroundWorker.RunWorkerCompleted += (o, args) => Dispatcher.Invoke((Action)(() =>
            {
                CancelBtn.IsEnabled = false;
                StartBtn.IsEnabled = true;
                DirectoriesBtn.IsEnabled = true;
            
                if (m_autoMode)
                {
                    Console.Write(m_extractor.Log);
                    Application.Current.Shutdown(0);
                }
                else
                {
                    Clipboard.SetText(m_extractor.Log);
                    MessageBox.Show("A log report was pasted into the clipboard");
                }
            }));

            m_backgroundWorker.RunWorkerAsync();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_backgroundWorker != null)
            {
                m_extractor.CancelAsync();
                m_backgroundWorker.CancelAsync();
                // The RunWorkerCompleted event will fire which will restore the buttons
                //  to their proper enabled states.
            }
        }

        private void DirectoriesBtn_Click(object sender, RoutedEventArgs e)
        {
            m_extractor.OpenSettingsFile(m_settingsPath);
        }

        private readonly Extractor m_extractor = new Extractor();
        private BackgroundWorker m_backgroundWorker;
        private readonly string m_settingsPath = "DirectoriesToLocalize.txt";
        private readonly bool m_autoMode; //if true, the program needs to run and exit without user input
    }
}
