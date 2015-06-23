using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasteToTargetFeedbackLoop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using EnvDTE;

namespace Microsoft.PasteToTargetFeedbackLoop
{
    public enum PasteToTargetState
    {
        Inactive,
        CaptureMode,
        CaptureModeInvalidContent,
        Pasting,
        Canceling
    }

    public class PasteToTargetActivity: IDisposable
    {
        Win32ClipboardMonitor m_monitor = null;

        public PasteToTargetActivity()
        {
            m_monitor = new Win32ClipboardMonitor();
        }

        public void Dispose()
        {
            m_monitor.Dispose();
        }

        public PasteToTargetOperation Operation { get; private set; }
        public PasteToTargetOperationUI OperationUI { get; private set; }

        public PasteToTargetOperation StartOperation(DTE dte, IVsTextView targetEditor, EnvDTE.Window targetWindow)
        {
            if (Operation != null)
            {
                Operation.Dispose();
                Operation = null;
            }

            if (OperationUI != null)
            {
                OperationUI.Dispose();
                OperationUI = null;
            }

            Operation = new PasteToTargetOperation(m_monitor, dte, targetEditor, targetWindow);
            OperationUI = new PasteToTargetOperationUI(Operation, VSHelpers.GetViewHost(targetEditor));
            return Operation;
        }
    }

    public  class TextSelectionState
    {
        private TextSelectionMode m_mode;
        private bool m_isReversed = false;
        private List<ITrackingSpan> m_trackedSpans = null;
        private CaretPosition m_caretPosition;
        private TextSelectionState() { }

        public NormalizedSnapshotSpanCollection GetSelectedSpans(ITextSnapshot snapshot)
        {
            return new NormalizedSnapshotSpanCollection(m_trackedSpans.Select(tspan => tspan.GetSpan(snapshot)));
        }

        public static TextSelectionState Save(ITextSelection selection, ITextCaret caret)
        {
            TextSelectionState ret = new TextSelectionState();
            ret.m_mode = selection.Mode;
            ret.m_isReversed = selection.IsReversed;
            ret.m_caretPosition = caret.Position;

            ret.m_trackedSpans = new List<ITrackingSpan>();
            foreach (var span in selection.SelectedSpans)
            {
                ret.m_trackedSpans.Add(selection.TextView.TextBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive));
            }

            return ret;
        }

        public static void Restore(TextSelectionState state, ITextSelection selection, ITextCaret caret)
        {
            caret.MoveTo(state.m_caretPosition.BufferPosition, state.m_caretPosition.Affinity);

            selection.Mode = state.m_mode;
            
            SnapshotSpan newSpan;
            if (state.m_mode == TextSelectionMode.Box)
            {
                newSpan = new SnapshotSpan(
                    state.m_trackedSpans[0].GetStartPoint(selection.TextView.TextBuffer.CurrentSnapshot),
                    state.m_trackedSpans[state.m_trackedSpans.Count - 1].GetEndPoint(selection.TextView.TextBuffer.CurrentSnapshot));
            }
            else
            {
                newSpan = state.m_trackedSpans[0].GetSpan(selection.TextView.TextBuffer.CurrentSnapshot);
            }
            selection.Select(newSpan, state.m_isReversed);
        }
    }

    public class PasteToTargetOperation: IDisposable
    {
        Win32ClipboardMonitor m_monitor = null;
        PasteToTargetState m_state;
        
        IVsTextView m_targetEditor = null;
        EnvDTE.Window m_targetWindow = null;
        IWpfTextViewHost m_targetHost = null;
        DTE m_dte = null;

        public TextSelectionState SelectionState { get; set; }
        public event EventHandler<PasteToTargetState> StateChanged;

        public PasteToTargetOperation(Win32ClipboardMonitor monitor, DTE dte, IVsTextView targetEditor, EnvDTE.Window targetWindow)
        {
            m_monitor = monitor;
            Win32Operations.AddClipboardFormatListener(m_monitor.Handle);
            m_monitor.ClipboardChanged += monitor_ClipboardChanged;

            m_dte = dte;
            m_targetEditor = targetEditor;
            m_targetWindow = targetWindow;
            
            m_targetHost = VSHelpers.GetViewHost(m_targetEditor);
            m_targetHost.Closed += targetHost_Closed;

            SelectionState = TextSelectionState.Save(m_targetHost.TextView.Selection, m_targetHost.TextView.Caret);

            _SetState(PasteToTargetState.CaptureMode);
        }

        void targetHost_Closed(object sender, EventArgs e)
        {
            _SetState(PasteToTargetState.Inactive);
        }

        void monitor_ClipboardChanged(object sender, EventArgs e)
        {
            if (m_targetWindow == null)
                return;

            if (Clipboard.ContainsText())
            {
                _SetState(PasteToTargetState.Pasting);
            }
            else
            {
                _SetState(PasteToTargetState.CaptureModeInvalidContent);
            }
        }

        public void Dispose()
        {
            Win32Operations.RemoveClipboardFormatListener(m_monitor.Handle);
            m_monitor = null;

            _SetState(PasteToTargetState.Inactive);
        }

        internal void _SetState(PasteToTargetState state)
        {
            try
            {
                if (m_state != state)
                {
                    m_state = state;
                    if (StateChanged != null)
                        StateChanged(this, m_state);

                    switch (state)
                    {
                        case PasteToTargetState.CaptureMode:
                            break;
                        case PasteToTargetState.CaptureModeInvalidContent:
                            break;
                        case PasteToTargetState.Pasting:
                            OnEnterPasting();
                            break;
                        case PasteToTargetState.Inactive:
                            OnEnterInactive();
                            break;
                        case PasteToTargetState.Canceling:
                            OnEnterCanceling();
                            break;
                    }
                }
            }
            catch (Exception ex) // TODO: Remove generic catch stmt 
            {
                MessageBox.Show(ex.ToString(), ex.Message);
            }
        }

        void OnEnterPasting()
        {
            // get actual text
            string sourceText = Clipboard.GetText();

            // move VS window into the foreground
            m_dte.MainWindow.Activate();

            // move editor into the foreground
            m_targetWindow.Activate();

            // restore selection & caret
            TextSelectionState.Restore(SelectionState, m_targetHost.TextView.Selection, m_targetHost.TextView.Caret);

            // Paste clipboard 
            m_dte.ExecuteCommand("Edit.Paste");

            _SetState(PasteToTargetState.Inactive);
        }

        void OnEnterInactive()
        {
            m_targetEditor = null;
            m_targetWindow = null;

            m_targetHost.Closed -= targetHost_Closed;
            m_targetHost = null;

            SelectionState = null;
        }

        void OnEnterCanceling()
        {
            // move VS window into the foreground
            m_dte.MainWindow.Activate();

            // move editor into the foreground
            m_targetWindow.Activate();

            // restore selection & caret
            TextSelectionState.Restore(SelectionState, m_targetHost.TextView.Selection, m_targetHost.TextView.Caret);

            _SetState(PasteToTargetState.Inactive);
        }
    }

    public class PasteToTargetOperationUI: IDisposable
    {
        private PasteToTargetOperation _op;
        private IWpfTextViewHost m_targetHost = null;
        private PasteToTargetStatusAdornment m_adornment = null;
        private PasteToTargetHighlight m_highlightTarget = null;

        public event EventHandler<PasteToTargetState> StateChanged
        {
            add { _op.StateChanged += value;  }
            remove { _op.StateChanged -= value; }
        }

        public PasteToTargetOperationUI(PasteToTargetOperation op, IWpfTextViewHost targetHost)
        {
            _op = op;
            _op.StateChanged += _op_StateChanged;

            m_targetHost = targetHost;

            m_adornment = new PasteToTargetStatusAdornment(m_targetHost.TextView);
            m_adornment.Cancel += adornment_Cancel;

            m_highlightTarget = m_targetHost.TextView.Properties.GetOrCreateSingletonProperty<PasteToTargetHighlight>(delegate
            {
                throw new Exception("Unexpected: ITagger PasteToTargetHighlight was not created");
            });
            m_highlightTarget.FireTagsChanged(
                PasteToTargetState.CaptureMode, 
                _op.SelectionState.GetSelectedSpans(m_targetHost.TextView.TextBuffer.CurrentSnapshot));
        }

        void _op_StateChanged(object sender, PasteToTargetState e)
        {
            switch(e)
            {
                case PasteToTargetState.CaptureMode:
                    m_adornment.Visible = true;
                    
                    break;
                case PasteToTargetState.Inactive:
                    m_adornment.Visible = false;
                    m_adornment = null;

                    m_highlightTarget.FireTagsChanged(e, null);
                    m_highlightTarget = null;

                    break;
            }

            if (m_adornment != null)
                m_adornment.State = e;
        }

        void adornment_Cancel(object sender, EventArgs e)
        {
            _op._SetState(PasteToTargetState.Canceling);
        }

        public void Dispose()
        {
            if (m_adornment != null)
            {
                m_adornment.Visible = false;
                m_adornment = null;
            }
        }
    }
}
