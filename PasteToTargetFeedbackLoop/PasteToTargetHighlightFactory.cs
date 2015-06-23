using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PasteToTargetFeedbackLoop
{
    public static class ClassificationTypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("paste-to-target-clasification")]
        public static ClassificationTypeDefinition OrdinaryClassificationType;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "paste-to-target-clasification")]
    [Name("paste-to-target-highlight")]
    [DisplayName("Paste-to-target Highlight Color")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class PasteToTargetHighlightDefinition : ClassificationFormatDefinition
    {
        public PasteToTargetHighlightDefinition()
        {
            DisplayName = "Paste-to-target Highlight Color";
            BackgroundColor = Colors.LightSalmon;
        }
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(ClassificationTag))]
    public sealed class PasteToTargetHighlightFactory : IViewTaggerProvider
    {
        [Import(typeof(IClassificationTypeRegistryService))]
        public IClassificationTypeRegistryService ClassificationTypeRegistryService = null;

        public ITagger<T> CreateTagger<T>(Microsoft.VisualStudio.Text.Editor.ITextView textView, Microsoft.VisualStudio.Text.ITextBuffer buffer) where T : ITag
        {
            if (textView.TextBuffer != buffer)
                return null;

            IClassificationType classificationType = ClassificationTypeRegistryService.GetClassificationType("paste-to-target-clasification");
            var instance = textView.Properties.GetOrCreateSingletonProperty<PasteToTargetHighlight>(delegate
            {
                return new PasteToTargetHighlight(textView, classificationType);
            });
            return instance as ITagger<T>;
        }
    }
}
