using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProcessLauncher
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return 1;
            }
            var sb = new StringBuilder();
            string path = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                    path = args[i];
                else
                    sb.Append(string.Format("{0} ", args[i]));
            }
            string cmdArgs = sb.ToString().Trim();
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist: {0}", path);
                PrintUsage();
                return 1;
            }

            if (Process.GetCurrentProcess().SessionId == 0)
            {
                return LaunchProcessInNewSession(path, cmdArgs);
            }
            else
            {
                return LaunchProcessNormally(path, cmdArgs);
            }
        }

        private static int LaunchProcessInNewSession(string path, string cmdArgs)
        {
            try
            {
                Console.WriteLine("Session 0 detected, launching process in special mode");
                int ret = SpecialLauncher.LaunchProcessInNonZeroSession(path, cmdArgs);
                Console.WriteLine("Exit code from special sesssion mode:{0}", ret);
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error launching process in special mode: {0}", e.ToString());
                return 1;
            }
        }

        private static int LaunchProcessNormally(string path, string cmdArgs)
        {
            Console.WriteLine("Launching process directly");
            var newProcess = new Process();
            newProcess.StartInfo.FileName = path;
            newProcess.StartInfo.Arguments = cmdArgs;
            newProcess.StartInfo.UseShellExecute = false;
            newProcess.StartInfo.RedirectStandardOutput = true;

            newProcess.Start();

            newProcess.OutputDataReceived += new DataReceivedEventHandler(newProcess_OutputDataReceived);
            newProcess.BeginOutputReadLine();

            if (!newProcess.WaitForExit(1000 * 60 *120))
            {
                Console.WriteLine("Process {0} timed out after 2 hours", path);
                newProcess.Kill();
                return 1;
            }
            Console.WriteLine("Process exit code: {0}", newProcess.ExitCode);

            return newProcess.ExitCode;
        }

        static void newProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ProcessLauncher.exe <pathToProgram> [args ...]");
        }
    }
}
