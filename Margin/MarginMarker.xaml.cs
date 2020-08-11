using BetterBreakpoints.Common;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BetterBreakpoints.Margin
{
    /// <summary>
    /// Interaction logic for MarginMarker.xaml
    /// </summary>
    public partial class MarginMarker : Canvas
    {
        public BreakpointInfo bpInfo { get; }
        
        public MarginMarker(BreakpointInfo bpInfo)
        {
            this.bpInfo = bpInfo;
            this.bpInfo.OnBreakpointChanged += _ => this.InvalidateVisual();

            InitializeComponent();
        }

        protected override void OnRender(DrawingContext dc)
        {
            Outline.Fill = bpInfo.GetBrush();

            switch (bpInfo.mode)
            {
                case BreakpointMode.TriggerAndBreak:
                    {
                        Fill.Visibility = Visibility.Hidden;
                        TriggerIcon.Visibility = Visibility.Visible;
                        break;
                    }
                case BreakpointMode.TriggerAndContinue:
                    {
                        Fill.Visibility = Visibility.Visible;
                        TriggerIcon.Visibility = Visibility.Visible;
                        break;
                    }
                case BreakpointMode.NotTriggered:
                    {
                        Fill.Visibility = Visibility.Visible;
                        TriggerIcon.Visibility = Visibility.Hidden;
                        break;
                    }
                case BreakpointMode.Triggered:
                    {
                        Fill.Visibility = Visibility.Hidden;
                        TriggerIcon.Visibility = Visibility.Hidden;
                        break;
                    }
            }

            base.OnRender(dc);
        }


        public void UpdatePosition(ITextSnapshot snapshot, ITextView textView)
        {
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(bpInfo.lineNumber - 1);
            ITextViewLine lineView = textView.GetTextViewLineContainingBufferPosition(line.Start);

            var span = new SnapshotSpan(line.Start, line.End);
            if (!textView.TextViewLines.FormattedSpan.IntersectsWith(span))
            {
                Visibility = Visibility.Hidden;
                return;
            }

            Canvas.SetTop(this, lineView.Top - textView.ViewportTop);

            switch (lineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                case VisibilityState.PartiallyVisible:
                    Visibility = Visibility.Visible;
                    break;
                case VisibilityState.Hidden:
                case VisibilityState.Unattached:
                    Visibility = Visibility.Hidden;
                    break;
            }
        }
    }

}
