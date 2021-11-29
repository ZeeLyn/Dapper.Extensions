using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SQLSplice
{
    internal static class MouseHelper
    {
        public static int GetMousePositionInTextView(IVsTextView view, IWpfTextView wpfTextView, Point wpfClientPoint)
        {
            ITextViewLine textViewLine =
                wpfTextView.TextViewLines.GetTextViewLineContainingYCoordinate(wpfClientPoint.Y + wpfTextView.ViewportTop);

            if (textViewLine == null) return 0;

            double xCoordinate = wpfClientPoint.X + GetPhysicalLeftColumn(view);

            SnapshotPoint? bufferPositionFromXCoordinate = textViewLine.GetBufferPositionFromXCoordinate(xCoordinate);
            if (!bufferPositionFromXCoordinate.HasValue)
            {
                return 0;
            }

            // Following can get line and column if necessary
            // view.GetLineAndColumn(bufferPositionFromXCoordinate.Value.Position, out line, out column);

            return bufferPositionFromXCoordinate.Value.Position;
        }

        private static IWpfTextViewHost GetTextViewHost(IVsTextView view)
        {
            var vsTextView = view as IVsUserData;
            if (vsTextView == null)
            {
                return null;
            }
            Guid guidIWpfTextViewHost = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;
            vsTextView.GetData(ref guidIWpfTextViewHost, out object obj2);
            return (IWpfTextViewHost)obj2;
        }

        public enum ScrollBarConstants { SB_HORZ, SB_VERT, SB_CTL, SB_BOTH }

        public static int GetPhysicalLeftColumn(IVsTextView view)
        {
            GetScrollInfo(view, ScrollBarConstants.SB_HORZ, out int num, out int num2, out int num3, out int num4);
            return num4;
        }

        private static void GetScrollInfo(IVsTextView view, ScrollBarConstants bar,
            out int minUnit, out int maxUnit, out int visibleLineCount, out int firstVisibleUnit)
        {
            view.GetScrollInfo((int)bar, out minUnit, out maxUnit, out visibleLineCount, out firstVisibleUnit);
        }
    }
}