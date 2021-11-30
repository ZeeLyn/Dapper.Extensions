using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace SQLSpliceExtension
{
    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/HighlightWordFormatDefinition")]
    [UserVisible(true)]
    public class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            //this.BackgroundColor = Color.FromRgb(9, 240, 180);
            this.ForegroundColor = Color.FromRgb(9, 240, 180);
            this.ZOrder = 999;
        }
    }
}
