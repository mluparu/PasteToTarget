using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using EnvDTE;
using Microsoft.PasteToTargetFeedbackLoop;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Windows.Forms;

namespace Microsoft.PasteToTarget
{
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidPasteToTargetPkgString)]
    public sealed class PasteToTargetPackage : Package
    {
        private DTEEvents m_packageEvents = null;
        private PasteToTargetActivity m_activity = null;

        public PasteToTargetPackage()
        {
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidPasteToTargetCmdSet, (int)PkgCmdIDList.cmdidPasteToTarget);
                MenuCommand menuItem = new MenuCommand(OnPasteToTargetClicked, menuCommandID);
                mcs.AddCommand( menuItem );
            }

            DTE2 dte = GetService(typeof(SDTE)) as DTE2;
            if (dte != null)
            {
                m_packageEvents = dte.Events.DTEEvents;
                m_packageEvents.OnBeginShutdown += DTEEvents_OnBeginShutdown;
            }

            m_activity = new PasteToTargetActivity();
        }

        private void DTEEvents_OnBeginShutdown()
        {
            m_activity.Dispose();
            m_activity = null;
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void OnPasteToTargetClicked(object sender, EventArgs e)
        {
            try
            {
                DTE dte = GetService(typeof(SDTE)) as DTE;
                var doc = dte.ActiveDocument;
                var win = doc.ActiveWindow;

                IVsTextManager vsEditors = (IVsTextManager)GetService(typeof(VsTextManagerClass)) as IVsTextManager;
                IVsTextView activeEditor = null;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(vsEditors.GetActiveView(-1, null, out activeEditor));

                m_activity.StartOperation(dte, activeEditor, win);
            }
            catch (Exception ex) // TODO: Remove generic catch stmt
            {
                VSHelpers.ShowException(ex);
            }
        }

    }
}
