using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLSpliceExtension
{
    public class HighlightWordTagger : ITagger<HighlightWordTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        ITextView View { get; set; }
        IWpfTextView WpfTextView { get; set; }
        ITextBuffer SourceBuffer { get; set; }
        ITextSearchService TextSearchService { get; set; }
        ITextStructureNavigator TextStructureNavigator { get; set; }
        NormalizedSnapshotSpanCollection WordSpans { get; set; }
        SnapshotSpan? CurrentWord { get; set; }
        SnapshotPoint RequestedPoint { get; set; }
        object updateLock = new object();


        public HighlightWordTagger(ITextView view, IWpfTextView wpfTextView, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
   ITextStructureNavigator textStructureNavigator)
        {
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            this.TextSearchService = textSearchService;
            this.TextStructureNavigator = textStructureNavigator;
            this.WordSpans = new NormalizedSnapshotSpanCollection();
            this.CurrentWord = null;
            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;
            WpfTextView = wpfTextView;

        }
        void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            // If a new snapshot wasn't generated, then skip this layout 
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(View.Caret.Position);
            }
        }

        void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            SnapshotPoint? point = caretPosition.Point.GetPoint(SourceBuffer, caretPosition.Affinity);

            if (!point.HasValue)
                return;

            // If the new caret position is still within the current word (and on the same snapshot), we don't need to check it 
            if (CurrentWord.HasValue
                && CurrentWord.Value.Snapshot == View.TextSnapshot
                && point.Value >= CurrentWord.Value.Start
                && point.Value <= CurrentWord.Value.End)
            {
                return;
            }

            RequestedPoint = point.Value;
            UpdateWordAdornments();
        }

        void TraversideNodes(ChildSyntaxList nodes, List<SyntaxNodeOrToken> methods)
        {
            foreach (var node in nodes)
            {
                if (node.IsKind(SyntaxKind.IdentifierName) && node.ToString().Equals("Splice"))
                {
                    methods.Add(node);
                    continue;
                }
                var child = node.ChildNodesAndTokens();
                if (child.Any())
                    TraversideNodes(child, methods);
            }
        }

        void UpdateWordAdornments()
        {


            SnapshotPoint currentRequest = RequestedPoint;


            var document = View.TextSnapshot.TextBuffer.GetRelatedDocuments().FirstOrDefault();
            if (document == null)
            {
                SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
                return;
            }

            var syntaxTree = document.GetSyntaxTreeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var root = syntaxTree.GetRoot();

            //SyntaxNodeOrToken token = root.FindToken(currentRequest.Position);

            var line = RequestedPoint.GetContainingLine();
            SyntaxNodeOrToken lineSyntax = root.FindNode(TextSpan.FromBounds(line.Start, line.End));
            var methods = new List<SyntaxNodeOrToken>();
            TraversideNodes(lineSyntax.ChildNodesAndTokens(), methods);

            if (!methods.Any())
            {
                SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
                return;
            }
            var idx = -1;
            TextSpan currentArgSpan = TextSpan.FromBounds(0, 0);
            SyntaxNodeOrToken _sqlSyntax = null;
            foreach (var method in methods)
            {
                var _argsSyntax = ((SyntaxNodeOrToken)method.Parent).GetNextSibling().ChildNodesAndTokens().Where(p => p.IsKind(SyntaxKind.Argument));
                foreach (var _arg in _argsSyntax)
                {
                    idx++;
                    if (_arg.Span.Start <= currentRequest.Position && currentRequest.Position <= _arg.Span.End)
                    {
                        currentArgSpan = _arg.Span;
                        var dot = method.GetPreviousSibling();
                        //如果方法名签名不是一个点（.），终止
                        if (!dot.IsKind(SyntaxKind.DotToken))
                        {
                            SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
                            return;
                        }
                        _sqlSyntax = dot.GetPreviousSibling();
                        break;
                    }
                }
            }
            //如果没有选择参数，并且Splice方法前不是字符串
            if (idx == -1 || !_sqlSyntax.IsKind(SyntaxKind.StringLiteralExpression))
            {
                SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
                return;
            }

            //SyntaxNodeOrToken argsSyntax = null;

            //if (token.Parent.IsKind(SyntaxKind.ArgumentList))
            //    argsSyntax = token.Parent;
            //else
            //    argsSyntax = ((SyntaxNodeOrToken)token.Parent?.Parent).GetPreviousSibling().Parent;//此情况是光标在具体某个参数上

            //if (argsSyntax.RawKind == 0)
            //    argsSyntax = ((SyntaxNodeOrToken)token.Parent?.Parent?.Parent).GetPreviousSibling().Parent;

            //if (argsSyntax == null || !argsSyntax.IsKind(SyntaxKind.ArgumentList))
            //{
            //    SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
            //    return;
            //}

            //var expression = argsSyntax.GetPreviousSibling();

            //#region 判断是不是拼接sql的方法(Splice)
            //var syntax = expression.ChildNodesAndTokens();
            //if (syntax.Count != 3)
            //{
            //    SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
            //    return;
            //}
            //if (!syntax.First().IsKind(SyntaxKind.StringLiteralExpression) || !syntax.Last().IsKind(SyntaxKind.IdentifierName) || !syntax.Last().ToFullString().Equals("Splice", StringComparison.Ordinal))
            //{
            //    SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
            //    return;
            //}
            //#endregion
            //var args = argsSyntax.ChildNodesAndTokens().Where(p => p.IsKind(SyntaxKind.Argument)).ToList();
            //var pointer = token.Span.Start;
            ////获取当前焦点在第几个参数

            //foreach (var arg in args)
            //{
            //    idx++;
            //    if (arg.Span.Start >= pointer && pointer <= arg.Span.End)
            //        break;
            //}

            //var sqlSyntax = syntax.First();

            var sql = _sqlSyntax.ToString();
            var startIdx = -1;
            var endIdx = -1;
            var currentIdx = 0;
            for (var i = 0; i <= idx; i++)
            {
                startIdx = sql.IndexOf("{", currentIdx);
                if (startIdx == -1)
                    break;
                endIdx = sql.IndexOf("}", startIdx);
                if (endIdx == -1)
                    break;
                currentIdx = endIdx;
            }
            if (startIdx == -1 || endIdx == -1)
            {
                SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(), null);
                return;
            }


            List<SnapshotSpan> wordSpans = new List<SnapshotSpan>();
            //添加当前选中的参数高亮
            //wordSpans.Add(new SnapshotSpan(View.TextSnapshot, currentArgSpan.Start, currentArgSpan.End - currentArgSpan.Start));
            //添加对应的拼接sql部分高亮
            wordSpans.Add(new SnapshotSpan(View.TextSnapshot, _sqlSyntax.Span.Start + startIdx, endIdx - startIdx + 1));


            //Find all words in the buffer like the one the caret is on
            TextExtent word = TextStructureNavigator.GetExtentOfWord(currentRequest);



            SnapshotSpan currentWord = word.Span;
            //If this is the current word, and the caret moved within a word, we're done. 
            if (CurrentWord.HasValue && currentWord == CurrentWord)
                return;


            //If another change hasn't happened, do a real update 
            if (currentRequest == RequestedPoint)
                SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(wordSpans), currentWord);
        }
        static bool WordExtentIsValid(SnapshotPoint currentRequest, TextExtent word)
        {
            return word.IsSignificant
                && currentRequest.Snapshot.GetText(word.Span).Any(c => char.IsLetter(c));
        }

        void SynchronousUpdate(SnapshotPoint currentRequest, NormalizedSnapshotSpanCollection newSpans, SnapshotSpan? newCurrentWord)
        {
            lock (updateLock)
            {
                if (currentRequest != RequestedPoint)
                    return;

                WordSpans = newSpans;
                CurrentWord = newCurrentWord;

                var tempEvent = TagsChanged;
                if (tempEvent != null)
                    tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0, SourceBuffer.CurrentSnapshot.Length)));
            }
        }

        public IEnumerable<ITagSpan<HighlightWordTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (CurrentWord == null)
                yield break;

            // Hold on to a "snapshot" of the word spans and current word, so that we maintain the same
            // collection throughout
            SnapshotSpan currentWord = CurrentWord.Value;
            NormalizedSnapshotSpanCollection wordSpans = WordSpans;

            if (spans.Count == 0 || wordSpans.Count == 0)
                yield break;

            // If the requested snapshot isn't the same as the one our words are on, translate our spans to the expected snapshot 
            if (spans[0].Snapshot != wordSpans[0].Snapshot)
            {
                wordSpans = new NormalizedSnapshotSpanCollection(
                    wordSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));

                currentWord = currentWord.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
            }

            // First, yield back the word the cursor is under (if it overlaps) 
            // Note that we'll yield back the same word again in the wordspans collection; 
            // the duplication here is expected. 
            //if (spans.OverlapsWith(new NormalizedSnapshotSpanCollection(currentWord)))
            //    yield return new TagSpan<HighlightWordTag>(currentWord, new HighlightWordTag());

            // Second, yield all the other words in the file 
            foreach (SnapshotSpan span in NormalizedSnapshotSpanCollection.Overlap(spans, wordSpans))
            {
                yield return new TagSpan<HighlightWordTag>(span, new HighlightWordTag());
            }
        }
    }

}
