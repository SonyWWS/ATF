using System;

namespace Sce.Atf.Input
{
    /// <summary> 
    /// Class that implements a Windows message</summary>
    public class Message 
    {
        IntPtr hWnd;
        int msg; 
        IntPtr wparam; 
        IntPtr lparam;
        IntPtr result; 

        /// <summary>
        /// Gets or sets the window handle of the message</summary>
        public IntPtr HWnd { 
            get { return hWnd; }
            set { hWnd = value; } 
        }

        /// <summary>
        /// Gets or sets the ID number for the message</summary>
        public int Msg { 
            get { return msg; }
            set { msg = value; } 
        }

        /// <summary>
        /// Gets or sets the System.Windows.Forms.Message.wparam of the message</summary>
        public IntPtr WParam { 
            get { return wparam; }
            set { wparam = value; } 
        }

        /// <summary>
        /// Gets or sets the System.Windows.Forms.Message.lparam of the message</summary>
        public IntPtr LParam { 
            get { return lparam; }
            set { lparam = value; } 
        }

        /// <summary>
        /// Gets or sets the return value of the message</summary>
        public IntPtr Result { 
             get { return result; }
             set { result = value; } 
        }

        /// <summary>
        /// Tests equality of the instance with an object</summary>
        /// <param name="o">Object to compare</param>
        /// <returns>True iff the messages are identical</returns>
        public override bool Equals(object o) {
            if (!(o is Message)) { 
                return false; 
            }
 
            Message m = (Message)o;
            return hWnd == m.hWnd &&
                   msg == m.msg &&
                   wparam == m.wparam && 
                   lparam == m.lparam &&
                   result == m.result; 
        }

        /// <summary>
        /// Operator that tests inequality of the Message instance with another Message</summary>
        /// <param name="a">Message 1 to compare</param>
        /// <param name="b">Message 2 to compare</param>
        /// <returns>True iff the messages are not identical</returns>
        public static bool operator !=(Message a, Message b) { 
            return !a.Equals(b);
        }

        /// <summary>
        /// Operator that tests equality of the Message instance with another Message</summary>
        /// <param name="a">Message 1 to compare</param>
        /// <param name="b">Message 2 to compare</param>
        /// <returns>True iff the messages are identical</returns>
        public static bool operator ==(Message a, Message b) { 
            return a.Equals(b);
        } 
 
        /// <summary>
        /// Returns instance's hash code</summary>
        /// <returns>Instance's hash code</returns>
        public override int GetHashCode() { 
            return (int)hWnd << 4 | msg;
        }
    } 
}
