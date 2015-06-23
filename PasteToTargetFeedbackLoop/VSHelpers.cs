using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.PasteToTargetFeedbackLoop
{
    public static class VSHelpers
    {
        public static IWpfTextViewHost GetViewHost(IVsTextView view)
        {
            IVsUserData userData = view as IVsUserData;
            if (userData == null)
            {
                return null;
            }
            else
            {
                IWpfTextViewHost viewHost;
                object holder;
                Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
                userData.GetData(ref guidViewHost, out holder);
                viewHost = (IWpfTextViewHost)holder;
                return viewHost;
            }
        }
        public static ITextDocument GetTextDocumentForView(IWpfTextViewHost viewHost)
        {
            ITextDocument document = null;
            viewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);
            return document;
        }
     
        //public static ITextSelection GetSelection(IWpfTextViewHost viewHost)
        //{
        //    return viewHost.TextView.Selection;
        //}

        public static void ShowException(Exception ex)
        {
            MessageBox.Show(ex.ToString(), ex.Message);
        }
    }
}
