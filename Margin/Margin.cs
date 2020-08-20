using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BetterBreakpoints.Common;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace BetterBreakpoints.Margin
{
    /// <summary>
    /// Margin's canvas and visual definition including both size and content
    /// </summary>
    public class Margin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "BetterBreakpointsMargin";

        public MarginMarker selectedBreakpoint
        {
            get { return _selectedMarker; }
        }

        private bool _isDisposed;

        private List<MarginMarker> _breakpointMarkers = new List<MarginMarker>();
        private string _filePath;
        private ITextView _textView;

        private MarginPopup _popup;
        private MarginMarker _selectedMarker = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Margin"/> class for a given <paramref name="textView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public Margin(IWpfTextView textView)
        {
            this._textView = textView;

            this.Width = 17;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));
            this.Cursor = Cursors.Arrow;

            // Get the path of the file this margin was created for.
            ITextDocument document;
            document = (ITextDocument)_textView.TextDataModel.DocumentBuffer.Properties.GetProperty(typeof(ITextDocument));
            this._filePath = document.FilePath;


            _textView.LayoutChanged += OnLayoutChanged;

            _popup = new MarginPopup(this);

            this.Children.Add(_popup);
        }

        public void SetSelectedBreakpointMode(BreakpointMode mode)
        {
            _selectedMarker.bpInfo.mode = mode;
        }

        public void SetSelectedBreakpointColor(BreakpointColor color)
        {
            _selectedMarker.bpInfo.color = color;
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            ITextSnapshot snapshot = _textView.TextBuffer.CurrentSnapshot;
            foreach (MarginMarker marker in _breakpointMarkers)
            {
                marker.UpdatePosition(snapshot, (ITextView)sender);
            }

            _breakpointMarkers.RemoveAll(bpMarker =>
            {
                if (!bpMarker.bpInfo.IsValid())
                {
                    ExtensionState.g_extensionState.RemoveBreakpoint(bpMarker.bpInfo.filePath, bpMarker.bpInfo.lineNumber);
                    this.Children.Remove(bpMarker);
                    return true;
                }
                else
                {
                    return false;
                }
            });

            // Hide the popup
            _popup.PlacementTarget = null;
            _popup.IsOpen = false;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            ITextSnapshot snapshot = _textView.TextBuffer.CurrentSnapshot;
            try
            {
                // Get the 0-indexed line number from the snapshot, based on mouse position.
                int mousePosition = (int)e.GetPosition(this).Y + (int)_textView.ViewportTop;
                IEnumerable<ITextSnapshotLine> lines = snapshot.Lines;
                int lineNumber = 0;

                foreach(ITextSnapshotLine line in lines)
                {
                    var lineView = _textView.GetTextViewLineContainingBufferPosition(line.Start);
                    if(lineView.Top <= mousePosition && mousePosition < lineView.Bottom)
                    {
                        lineNumber = line.LineNumber + 1;
                        break;
                    }
                }

                if (lineNumber == 0)
                {
                    return;
                }

                // Handle left click
                if (e.ChangedButton == MouseButton.Left)
                {
                    // If we already have a breakpoint, clicking removes it
                    BreakpointInfo bp = ExtensionState.g_extensionState.GetBreakpoint(_filePath, lineNumber);

                    if (bp != null)
                    {
                        ExtensionState.g_extensionState.RemoveBreakpoint(_filePath, lineNumber);
                        _breakpointMarkers.RemoveAll((bpMarker) =>
                        {
                            if (bpMarker.bpInfo.lineNumber == lineNumber)
                            {
                                this.Children.Remove(bpMarker);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                    }
                    else
                    {
                        BreakpointInfo bpInfo = ExtensionState.g_extensionState.CreateBreakpoint(_filePath, lineNumber);
                        MarginMarker bpMarker = new MarginMarker(bpInfo);
                        _breakpointMarkers.Add(bpMarker);
                        this.Children.Add(bpMarker);
                    }

                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    MarginMarker existingMarker = _breakpointMarkers.Find((bpMarker) =>
                    {
                        return (bpMarker.bpInfo.lineNumber == lineNumber);
                    });

                    if ((existingMarker != null) && (_selectedMarker != existingMarker))
                    {
                        _selectedMarker = existingMarker;
                        _popup.IsOpen = true;
                        _popup.PlacementTarget = _selectedMarker;
                        _popup.PlacementRectangle = new Rect(
                            0, /* Left */
                            0, /* Top */
                            this.Width,
                            0 /* Height */);

                        // Force the popup to reposition to where the breakpoint is
                        _popup.HorizontalOffset += 1;
                        _popup.HorizontalOffset -= 1;
                        _popup.InvalidateVisual();
                        _popup.UpdateSelectedBreakpoint();
                    }
                    else
                    {
                        _selectedMarker = null;
                        _popup.IsOpen = false;
                    }
                }

                InvalidateVisual();
                e.Handled = true;

            }
            catch (ArgumentOutOfRangeException) { }

            base.OnMouseDown(e);
        }

        #region IWpfTextViewMargin

        /// <summary>
        /// Gets the <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation of the margin.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                this.ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin

        /// <summary>
        /// Gets the size of the margin.
        /// </summary>
        /// <remarks>
        /// For a horizontal margin this is the height of the margin,
        /// since the width will be determined by the <see cref="ITextView"/>.
        /// For a vertical margin this is the width of the margin,
        /// since the height will be determined by the <see cref="ITextView"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public double MarginSize
        {
            get
            {
                this.ThrowIfDisposed();

                // Since this is a horizontal margin, its width will be bound to the width of the text view.
                // Therefore, its size is its height.
                return this.ActualWidth;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the margin is enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public bool Enabled
        {
            get
            {
                this.ThrowIfDisposed();

                // The margin should always be enabled
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="ITextViewMargin"/> with the given <paramref name="marginName"/> or null if no match is found
        /// </summary>
        /// <param name="marginName">The name of the <see cref="ITextViewMargin"/></param>
        /// <returns>The <see cref="ITextViewMargin"/> named <paramref name="marginName"/>, or null if no match is found.</returns>
        /// <remarks>
        /// A margin returns itself if it is passed its own name. If the name does not match and it is a container margin, it
        /// forwards the call to its children. Margin name comparisons are case-insensitive.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, BetterBreakpoints.Margin.Margin.MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        /// <summary>
        /// Disposes an instance of <see cref="Margin"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!this._isDisposed)
            {
                GC.SuppressFinalize(this);
                this._isDisposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
