using Microsoft.PasteToTargetFeedbackLoop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasteToTargetFeedbackLoop
{
    public sealed class PasteToTargetHighlight: ITagger<ClassificationTag>
    {
        internal NormalizedSnapshotSpanCollection activeSpans = null;
        private ITextView View { get; set; }
        private IClassificationType ClassificationType { get; set; }

        public event EventHandler<Microsoft.VisualStudio.Text.SnapshotSpanEventArgs> TagsChanged;

        public PasteToTargetHighlight(ITextView view, IClassificationType classificationType)
        {
            this.View = view;
            this.ClassificationType = classificationType;
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection spans)
        {
            if (activeSpans == null)
                yield break;

            if (activeSpans.Count == 0 || spans.Count == 0)
                yield break;

            if (activeSpans[0].Snapshot != spans[0].Snapshot)
            {
                activeSpans = new NormalizedSnapshotSpanCollection(
                    activeSpans.Select(
                        span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)
                    ));
            }

            foreach (var span in NormalizedSnapshotSpanCollection.Overlap(spans, activeSpans))
            {
                yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(ClassificationType));
            }
        }

        public void FireTagsChanged(PasteToTargetState state, NormalizedSnapshotSpanCollection spans)
        {
            if (TagsChanged != null)
            {
                if (state == PasteToTargetState.CaptureMode || state == PasteToTargetState.CaptureModeInvalidContent)
                {
                    activeSpans = spans;
                }

                if (activeSpans != null)
                {
                    var union = new SnapshotSpan(activeSpans[0].Start, activeSpans[activeSpans.Count - 1].End);
                    
                    if (spans == null)
                        activeSpans = null;

                    TagsChanged(this, new SnapshotSpanEventArgs(union));
                }
            }
        }       
    }
}
