using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SQLSplice
{
    [Export(typeof(IMouseProcessorProvider))]
    [Name("MyMouseHoverProcessorProvider")]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Order(After = "VisualStudioMouseProcessor")]
    public class MyMouseHoverProcessorProvider : IMouseProcessorProvider
    {
        private readonly IVsEditorAdaptersFactoryService _editorAdapters;
        private readonly CurrentWordPosition _currentWordPosition;

        [ImportingConstructor]
        public MyMouseHoverProcessorProvider(
            IVsEditorAdaptersFactoryService editorAdapters, CurrentWordPosition currentWordPosition)
        {
            _editorAdapters = editorAdapters;
            _currentWordPosition = currentWordPosition;
        }


        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return new MyMouseHoverProcessor(wpfTextView, _editorAdapters, _currentWordPosition);
        }
    }

    internal class MyMouseHoverProcessor : MouseProcessorBase
    {
        private readonly IWpfTextView _view;
        private readonly CurrentWordPosition _currentWordPosition;
        private readonly IVsTextView _viewAdapter;

        public MyMouseHoverProcessor(IWpfTextView wpfTextView, IVsEditorAdaptersFactoryService editorAdapters,
            CurrentWordPosition currentWordPosition)
        {
            _view = wpfTextView;
            _currentWordPosition = currentWordPosition;
            _viewAdapter = editorAdapters.GetViewAdapter(wpfTextView);
        }

        public override void PostprocessMouseDown(MouseButtonEventArgs e)
        {
            try
            {

                var mousePos = e.GetPosition(_view.VisualElement);

                int streamPosition = MouseHelper.GetMousePositionInTextView(_viewAdapter, _view, mousePos);

                var document = _view.TextSnapshot.TextBuffer.GetRelatedDocuments().First();
                var syntaxTree = document.GetSyntaxTreeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var root = syntaxTree.GetRoot();
                foreach (var item in root.ChildNodesAndTokens())
                {
                    if (item.IsKind(SyntaxKind.NamespaceDeclaration))
                    {
                        foreach (var c in item.ChildNodesAndTokens())
                        {
                            var _c = c;
                        }
                    }

                }
                SyntaxNodeOrToken token = root.FindToken(streamPosition);

                //var trivia = root.FindTrivia(streamPosition);
                SyntaxNodeOrToken node = root.FindNode(token.Span);
                var txt = node.Parent.GetText();

                //var ms = node.Parent.Parent.GetLocation().SourceTree.GetRoot().AncestorsAndSelf().OfType<MethodDeclarationSyntax>();




                //var m = node.Parent.Parent.ChildNodes().ToArray()[0] as MemberAccessExpressionSyntax;

                //var mm = m.Name.Identifier.GetLocation().SourceTree.GetRoot().AncestorsAndSelf().First();
                //var methodName = m.Name.Identifier.ValueText;
                //var p = m.Parent as MemberAccessExpressionSyntax;
                ////var method = m as BaseMethodDeclarationSyntax;

                //if (streamPosition != 0)
                //{
                //    Debug.WriteLine($"position = {streamPosition} token={token.ToString()}");
                //}
                //else
                //{
                //    Debug.WriteLine($"No token under mouse");
                //}

                var argsSyntax = (SyntaxNodeOrToken)((SyntaxNodeOrToken)token.Parent.Parent).GetPreviousSibling().Parent;//此情况是鼠标点击到具体某个参数上
                if (argsSyntax == null)
                    argsSyntax = token.Parent;

                var expression = argsSyntax.GetPreviousSibling();
                #region 判断是不是拼接sql的方法(Splice)
                var syntax = expression.ChildNodesAndTokens();
                if (syntax.Count != 3)
                    return;
                if (!syntax.First().IsKind(SyntaxKind.StringLiteralExpression) || !syntax.Last().IsKind(SyntaxKind.IdentifierName) || !syntax.Last().ToFullString().Equals("Splice", StringComparison.Ordinal))
                    return;
                #endregion
                var args = argsSyntax.ChildNodesAndTokens().Where(p => p.IsKind(SyntaxKind.Argument));
                var pointer = token.Span.Start;
                var idx = -1;
                foreach (var arg in args)
                {
                    idx++;
                    if (arg.Span.Start >= pointer && pointer <= arg.Span.End)
                        break;
                }

                var sqlSyntax = syntax.First();


                base.PostprocessMouseDown(e);
                _currentWordPosition.InvokeWordUnderMouseChanged(sqlSyntax.Span.Start, sqlSyntax.Span.End);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
            }
        }
    }
}
