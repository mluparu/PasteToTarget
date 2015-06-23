using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using PasteToTargetStatus;

namespace Microsoft.PasteToTargetFeedbackLoop
{
    internal sealed class PasteToTargetStatusAdornment
    {
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;
        private PasteToTargetStatusView _root;
        private bool _visible = false;

        public PasteToTargetStatusAdornment(IWpfTextView view)
        {
            _view = view;

            _root = new PasteToTargetStatusView();
            _root.Cancel += _root_Cancel;

            //Place the image in the top right hand corner of the Viewport
            Canvas.SetLeft(_root, _view.ViewportRight - 245);
            Canvas.SetTop(_root, _view.ViewportTop + 30);

            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("PasteToTargetFeedbackLoop");
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _root, null);
            _visible = true;

            _view.ViewportHeightChanged += _view_ViewportSizeChanged;
            _view.ViewportWidthChanged += _view_ViewportSizeChanged;
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (value && !_visible)
                {
                    _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _root, null);
                    _visible = true;
                }
                else
                {
                    _adornmentLayer.RemoveAdornment(_root);
                    _visible = false;
                }
            }
        }

        public PasteToTargetState State
        {
            set 
            {
                switch (value)
                {
                    case PasteToTargetState.Inactive:
                        _root.Status = "Done";
                        break;
                    case PasteToTargetState.CaptureMode:
                        _root.Status = "Capture in progress...";
                        break;
                    case PasteToTargetState.CaptureModeInvalidContent:
                        _root.Status = "No text found. Still capturing...";
                        break;
                    case PasteToTargetState.Pasting:
                        _root.Status = "Pasting to target...";
                        break;
                    default:
                        _root.Status = "";
                        break;
                }
            }
        }

        public event EventHandler Cancel;

        void _view_ViewportSizeChanged(object sender, System.EventArgs e)
        {
            if (_visible)
            {
                _adornmentLayer.RemoveAdornment(_root);

                //Place the image in the top right hand corner of the Viewport
                Canvas.SetLeft(_root, _view.ViewportRight - 245);
                Canvas.SetTop(_root, _view.ViewportTop + 30);

                //add the image to the adornment layer and make it relative to the viewport
                _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _root, null);
            }
        }

        void _root_Cancel(object sender, System.EventArgs e)
        {
            if (Cancel != null)
                Cancel(this, e);
        }
    }
}
