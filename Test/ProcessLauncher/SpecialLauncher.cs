using System;
using System.Security;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessLauncher
{
    public class SpecialLauncher
    {
        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        #endregion

        #region Enumerations

        enum TOKEN_TYPE : int
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }

        enum SECURITY_IMPERSONATION_LEVEL : int
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3,
        }

        #endregion

        #region Constants

        public const int TOKEN_DUPLICATE = 0x0002;
        public const uint MAXIMUM_ALLOWED = 0x2000000;
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const int CREATE_NEW_CONSOLE = 0x00000010;
        public const uint STARTF_USESTDHANDLES = 0x00000100;

        public const int IDLE_PRIORITY_CLASS = 0x40;
        public const int NORMAL_PRIORITY_CLASS = 0x20;
        public const int HIGH_PRIORITY_CLASS = 0x80;
        public const int REALTIME_PRIORITY_CLASS = 0x100;

        public const int HANDLE_FLAG_INHERIT = 0x00000001;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12;

        public const int PROCESS_STILL_ACTIVE = 259;

        #endregion

        #region Win32 API Imports
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hSnapshot);

        [DllImport("kernel32.dll")]
        static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, 
                                                        String lpApplicationName, 
                                                        String lpCommandLine, 
                                                        ref SECURITY_ATTRIBUTES lpProcessAttributes,
                                                        ref SECURITY_ATTRIBUTES lpThreadAttributes, 
                                                        bool bInheritHandle, 
                                                        int dwCreationFlags, 
                                                        IntPtr lpEnvironment,
                                                        String lpCurrentDirectory, 
                                                        ref STARTUPINFO lpStartupInfo, 
                                                        out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        static extern bool GetExitCodeProcess(IntPtr handle, out uint wait);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreatePipe(ref IntPtr hReadPipe, ref IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetHandleInformation(IntPtr hObject, int dwMask, int dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool WriteFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        static extern bool DuplicateTokenEx(IntPtr existingTokenHandle, 
                                            uint dwDesiredAccess,
                                            ref SECURITY_ATTRIBUTES lpThreadAttributes, 
                                            int tokenType,
                                            int impersonationLevel, 
                                            ref IntPtr duplicateTokenHandle);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

        #endregion

        /// <summary>
        /// This function launches a program in a non-zero session.  In short, this works by:
        /// - Grab the winlogon process for the user who launched this tool
        /// - Copy the permissions of that process.  winlogon does not run on session 0, which ensures the child process is in a different session
        /// - Launch the process with the copied permissions
        /// - Grab the output of the child process, and print it out on this process's console
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cmdArgs"></param>
        /// <returns></returns>
        public static int LaunchProcessInNonZeroSession(string path, string cmdArgs)
        {
            //CreateProcessAsUser doesn't like forward slashes
            path = path.Replace("/", "\\");

            //The CreateProcessAsUser has separate parameters for "lpApplicationName" and "lpCommandLine".  
            //But the documentation says:
            //      "If the executable module is a 16-bit application, lpApplicationName should be NULL, and the 
            //      string pointed to by lpCommandLine should specify the executable module as well as its arguments"
            //To be robust (and since we generally are passing in batch files), just construct the full command line here
            string fullCommandLine = string.Format("{0} {1}", path, cmdArgs);
            
            // obtain the currently active session id; every logged on user in the system has a unique session id
            uint dwSessionId = WTSGetActiveConsoleSessionId();

            // obtain the process id of the winlogon process that is running within the currently active session
            Process[] processes = Process.GetProcessesByName("winlogon");
            uint winlogonPid = 0;
            foreach (Process p in processes)
            {
                if ((uint)p.SessionId == dwSessionId)
                {
                    winlogonPid = (uint)p.Id;
                    break;
                }
            }
            if (winlogonPid == 0)
            {
                //MRM: this is one assumption I'm not sure about.  Will the user who launches this process always
                //have a winlogon process running?
                //The Jenkins slave server has a winlogon running for the system account (which is what the Jenkins
                //service is running under).  But will this always be the case, including bamboo servers?
                //If this fails, some ideas:
                // - find a way to ensure the local user has winlogon running
                // - launch jenkins as a different user, and make sure that user is logged on
                // - search for a different process that is guaranteed to be running, and not in session 0
                string err = string.Format("Unable to find winlogon process for user launching the process ({0})",
                                           Process.GetCurrentProcess().StartInfo.UserName);
                Console.WriteLine(err);
                throw new Exception(err);
            }

            // obtain a handle to the winlogon process
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, false, winlogonPid);
            Console.WriteLine("Process handle: {0}", hProcess.ToInt64());
            if (hProcess.ToInt64() <= 0)
            {
                Console.WriteLine("OpenProcess returned invalid handle: {0}", Marshal.GetLastWin32Error());
                throw new Exception("OpenProcess returned invalid handle");
            }

            // obtain a handle to the access token of the winlogon process
            IntPtr hPToken = IntPtr.Zero;
            if (!OpenProcessToken(hProcess, TOKEN_DUPLICATE, ref hPToken))
            {
                CloseHandle(hProcess);
                Console.WriteLine("Failed to OpenProcessToken: {0}", Marshal.GetLastWin32Error());
                throw new Exception("Failed to OpenProcessToken");
            }

            // Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser
            // I would prefer to not have to use a security attribute variable and to just 
            // simply pass null and inherit (by default) the security attributes
            // of the existing token. However, in C# structures are value types and therefore
            // cannot be assigned the null value.
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.Length = Marshal.SizeOf(sa);
            sa.bInheritHandle = true;
            
            // copy the access token of the winlogon process; the newly created token will be a primary token
            //This is the magic that copies this process's permissions to the new process, and makes it launch in a user (non-zero) session
            IntPtr hUserTokenDup = IntPtr.Zero;
            if (!DuplicateTokenEx(hPToken, MAXIMUM_ALLOWED, ref sa, (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, (int)TOKEN_TYPE.TokenPrimary, ref hUserTokenDup))
            {
                CloseHandle(hProcess);
                CloseHandle(hPToken);
                throw new Exception("Failed to DuplicateToken");
            }

            //Setup handles to redirect the output so it can be printed out to this proccess's console
            IntPtr hChildProcessOutputRd = new IntPtr();
            IntPtr hChildProcessOutputWr = new IntPtr();
            if (!CreatePipe(ref hChildProcessOutputRd, ref hChildProcessOutputWr, ref sa, 0))
            {
                CloseHandle(hChildProcessOutputRd);
                CloseHandle(hChildProcessOutputWr);
                Console.WriteLine("Failed to CreatePipe: {0}", Marshal.GetLastWin32Error());
                throw new Exception("Failed to CreatePipe");
            }
            if (!SetHandleInformation(hChildProcessOutputRd, HANDLE_FLAG_INHERIT, 0))
            {
                CloseHandle(hChildProcessOutputRd);
                CloseHandle(hChildProcessOutputWr);
                Console.WriteLine("Failed to SetHandleInformation: {0}", Marshal.GetLastWin32Error());
                throw new Exception("Failed to SetHandleInformation");
            }

            // By default CreateProcessAsUser creates a process on a non-interactive window station, meaning
            // the window station has a desktop that is invisible and the process is incapable of receiving
            // user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
            // interaction with the new process.
            STARTUPINFO si = new STARTUPINFO();
            si.cb = (int)Marshal.SizeOf(si);
            si.lpDesktop = @"winsta0\default"; // interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop
            //Define a handle for the output of the launched process, so we can grab the output and print it to the console
            si.hStdOutput = hChildProcessOutputWr;
            si.hStdError = hChildProcessOutputWr;
            si.dwFlags |= STARTF_USESTDHANDLES;

            // Without CREATE_NEW_CONSOLE, creating the process in a new session doesn't work.
            //(which leads to the hassle of redirecting the child's output)
            const int dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;
            
            PROCESS_INFORMATION procInfo = new PROCESS_INFORMATION();
            // create a new process in the current user's logon session
            bool result = CreateProcessAsUser(hUserTokenDup,        // client's access token (the "magic").  Set this to IntPtr.Zero if testing locally and don't want to bother with that magic
                                            null,                   // file to execute
                                            fullCommandLine,        // command line
                                            ref sa,                 // pointer to process SECURITY_ATTRIBUTES
                                            ref sa,                 // pointer to thread SECURITY_ATTRIBUTES
                                            true,                   // handles are inheritable, necessary to grab the child's output
                                            dwCreationFlags,        // creation flags
                                            IntPtr.Zero,            // pointer to new environment block 
                                            null,                   // name of current directory 
                                            ref si,                 // pointer to STARTUPINFO structure
                                            out procInfo            // receives information about new process
                                            );
            if (!result || procInfo.dwProcessId == 0)
            {
                Console.WriteLine("Error creating process: {0}", Marshal.GetLastWin32Error());
                throw new Exception("Error creating process");
            }

            Console.WriteLine("ProcessId:{0}", procInfo.dwProcessId);
            Process newProc = Process.GetProcessById((int)procInfo.dwProcessId);
            Console.WriteLine("SessionId:{0}", newProc.SessionId);

            // invalidate the handles
            CloseHandle(hProcess);
            CloseHandle(hPToken);
            CloseHandle(hUserTokenDup);

            //Get the handle to this process's console window
            IntPtr hParentStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

            //Close this handle or the last ReadFile operation will hang
            CloseHandle(hChildProcessOutputWr);

            const int bufSize = 1024;
            var chBuf = new byte[bufSize];
            uint exitCode;
            
            while (true)
            {
                //Must read the output while the process is running.  
                //Otherwise the buffer fills up and everything freezes
                GetExitCodeProcess(procInfo.hProcess, out exitCode);

                //read output from child process
                uint dwRead;
                uint dwWritten;
                ReadFile(hChildProcessOutputRd, chBuf, bufSize, out dwRead, IntPtr.Zero);
                //write it out to this process's console
                WriteFile(hParentStdOut, chBuf, dwRead, out dwWritten, IntPtr.Zero);
                //or: Console.Write(System.Text.Encoding.Default.GetString(chBuf));

                if (exitCode != PROCESS_STILL_ACTIVE)
                    break;
            }

            CloseHandle(hChildProcessOutputRd);

            return (int)exitCode;
        }

    }
}