using BetterBreakpoints.Common;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace BetterBreakpoints.Margin
{
    /// <summary>
    /// Interaction logic for MarginPopup.xaml
    /// </summary>
    public partial class MarginPopup : Popup
    {
        private Margin _margin;
        public MarginPopup(Margin margin)
        {
            _margin = margin;
            InitializeComponent();
        }

        public void UpdateSelectedBreakpoint()
        {
            BreakpointInfo bpInfo = _margin.selectedBreakpoint?.bpInfo;

            if (bpInfo != null)
            {
                switch (bpInfo.mode)
                {
                    case BreakpointMode.TriggerAndBreak:
                    {
                        RadioButtonTrigger.IsChecked = true;
                        CheckBoxBreak.IsChecked = true;
                        break;
                    }
                    case BreakpointMode.TriggerAndContinue:
                    {
                        RadioButtonTrigger.IsChecked = true;
                        CheckBoxBreak.IsChecked = false;
                        break;
                    }
                    case BreakpointMode.NotTriggered:
                    case BreakpointMode.Triggered:
                    {
                        RadioButtonTriggeredBy.IsChecked = true;
                        break;
                    }
                }

                switch (bpInfo.color)
                {
                    case BreakpointColor.Red:
                        RadioButtonRed.IsChecked = true;
                        break;
                    case BreakpointColor.Orange:
                        RadioButtonOrange.IsChecked = true;
                        break;
                    case BreakpointColor.Yellow:
                        RadioButtonYellow.IsChecked = true;
                        break;
                    case BreakpointColor.Green:
                        RadioButtonGreen.IsChecked = true;
                        break;
                    case BreakpointColor.Blue:
                        RadioButtonBlue.IsChecked = true;
                        break;
                    case BreakpointColor.Purple:
                        RadioButtonPurple.IsChecked = true;
                        break;
                    case BreakpointColor.Pink:
                        RadioButtonPink.IsChecked = true;
                        break;
                }
            }
        }

        private void OnChangeBreakpointMode(object sender, RoutedEventArgs e)
        {
            if (RadioButtonTrigger.IsChecked.Value)
            {
                CheckBoxBreak.IsEnabled = true;

                if (CheckBoxBreak.IsChecked.Value)
                {
                    _margin.SetSelectedBreakpointMode(BreakpointMode.TriggerAndBreak);
                }
                else
                {
                    _margin.SetSelectedBreakpointMode(BreakpointMode.TriggerAndContinue);
                }
            }
            if (RadioButtonTriggeredBy.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointMode(BreakpointMode.NotTriggered);
                CheckBoxBreak.IsEnabled = false;
            }
        }

        private void OnSelectColor(object sender, RoutedEventArgs e)
        {
            if (RadioButtonRed.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Red);
            }
            if (RadioButtonOrange.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Orange);
            }
            if (RadioButtonYellow.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Yellow);
            }
            if (RadioButtonGreen.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Green);
            }
            else if (RadioButtonBlue.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Blue);
            }
            else if (RadioButtonPurple.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Purple);
            }
            else if (RadioButtonPink.IsChecked.Value)
            {
                _margin.SetSelectedBreakpointColor(BreakpointColor.Pink);
            }
        }
    }
}
