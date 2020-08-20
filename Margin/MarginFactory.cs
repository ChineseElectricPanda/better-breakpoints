using System.ComponentModel.Composition;
using BetterBreakpoints.Common;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace BetterBreakpoints.Margin
{
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(Margin.MarginName)]
    [Order(After = PredefinedMarginNames.Glyph, Before = PredefinedMarginNames.LineNumber)]  // Just right of the indicator margin
    [MarginContainer(PredefinedMarginNames.LeftSelection)] // LeftSelection scales with text editor zoom
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class MarginFactory : IWpfTextViewMarginProvider
    {

        #region IWpfTextViewMarginProvider

        /// <summary>
        /// Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
        /// </summary>
        /// <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
        /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
        /// <returns>The <see cref="IWpfTextViewMargin"/>.
        /// The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does not participate for this context.
        /// </returns>
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            if (!ExtensionState.IsInitialized())
            {
                ExtensionState.Initialize(Package.GetGlobalService(typeof(DTE)) as DTE2);
            }
            return new Margin(wpfTextViewHost.TextView);
        }

        #endregion
    }
}
