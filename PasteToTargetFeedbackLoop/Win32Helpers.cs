using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.PasteToTargetFeedbackLoop
{
    public static class Win32Operations
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int WM_CLIPBOARDUPDATE = 0x031D;

        public static IntPtr HWND_MESSAGE = new IntPtr(-3);
    }

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public sealed class Win32ClipboardMonitor : NativeWindow, IDisposable
    {
        public event EventHandler ClipboardChanged;

        public Win32ClipboardMonitor()
        {
            CreateParams cp = new CreateParams();

            // Fill in the CreateParams details.
            cp.Caption = "Paste-to-target invisible window";

            // Set the position on the form
            cp.X = 100;
            cp.Y = 100;
            cp.Height = 100;
            cp.Width = 100;

            // Specify the form as the parent.
            cp.Parent = Win32Operations.HWND_MESSAGE;

            // Create as a child of the specified parent
            cp.Style = 0;
            cp.ExStyle |= 0x80; /*WS_EX_TOOLWINDOW*/

            // Create the actual window 
            this.CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32Operations.WM_CLIPBOARDUPDATE:
                    if (ClipboardChanged != null)
                        ClipboardChanged(this, new EventArgs());
                    break;
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
