using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SQLSplice
{
    public class DocumentedCodeHighlighterTaggerProvider : IViewTaggerProvider
    {
        private readonly CurrentWordPosition _currentWordPosition;

        [ImportingConstructor]
        public DocumentedCodeHighlighterTaggerProvider(CurrentWordPosition currentWordPosition)
        {
            _currentWordPosition = currentWordPosition;
        }
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // Only provide highlighting on the top-level buffer
            if (textView.TextBuffer != buffer)
                return null;

            return new HighlighterTagger(textView, buffer, _currentWordPosition) as ITagger<T>;
        }
    }

    public class HighlighterTag : TextMarkerTag
    {
        public HighlighterTag()
            : base("MarkerFormatDefinition/FormatDefinition1") { }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/FormatDefinition1")]
    [UserVisible(true)]
    internal class DocumentedCodeFormatDefinition : MarkerFormatDefinition
    {
        public DocumentedCodeFormatDefinition()
        {
            var orange = Brushes.Pink.Clone();
            orange.Opacity = 0.25;
            this.Fill = orange;
            this.Border = new Pen(Brushes.Gray, 1.0);
            this.DisplayName = "Highlight Word 123";
            this.ZOrder = 5;
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class CurrentWordPosition
    {
        public event Action<int, int> WordUnderMouseChanged;

        public void InvokeWordUnderMouseChanged(int spanStart, int spanEnd)
        {
            WordUnderMouseChanged?.Invoke(spanStart, spanEnd);
        }
    }

    class HighlighterTagger : ITagger<HighlighterTag>
    {
        private ITextView _textView;
        private ITextBuffer _buffer;
        private bool _clearing = false;

        public HighlighterTagger(ITextView textView, ITextBuffer buffer, CurrentWordPosition currentWordPosition)
        {
            _textView = textView;
            _buffer = buffer;
            currentWordPosition.WordUnderMouseChanged += (start, end) =>
            {
                _clearing = true;
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(new SnapshotPoint(_buffer.CurrentSnapshot, Start), (End - Start))));
                _clearing = false;
                Start = start;
                End = end;
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(new SnapshotPoint(_buffer.CurrentSnapshot, Start), (End - Start))));
            };
        }

        public int Start { get; set; }
        public int End { get; set; }


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<HighlighterTag>> GetTags(
              NormalizedSnapshotSpanCollection spans)
        {

            List<ITagSpan<HighlighterTag>> res =
                new List<ITagSpan<HighlighterTag>>();
            if (_clearing)
                return res;
            var currentSnapshot = _buffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(new SnapshotPoint(_buffer.CurrentSnapshot, Start), (End - Start));
            res.Add(new TagSpan<HighlighterTag>(snapshotSpan, new HighlighterTag()));

            return res;
        }
    }
}
