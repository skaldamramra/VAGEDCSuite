using System;
using System.Drawing;
using System.Windows.Forms;

namespace VAGSuite.Services
{
    /// <summary>
    /// Static tooltip service that uses a custom WrappedTooltipForm for text wrapping support.
    /// Replaces legacy tooltip controller usage with a custom Form-based tooltip that supports
    /// automatic word wrapping for long descriptions.
    /// 
    /// Usage (example):
    ///     // show near cursor relative to owner control
    ///     TooltipService.ShowForControl(tvSymbols, tvSymbols.PointToClient(Cursor.Position), "Varname", "Description...");
    ///     // hide
    ///     TooltipService.Hide();
    /// 
    /// Rationale:
    /// - WinForms.ToolTip doesn't support automatic text wrapping, causing long descriptions
    ///   to extend beyond the screen width.
    /// - Custom WrappedTooltipForm uses Label with AutoSize=false and MaxWidth to enable word wrapping.
    /// - Uses TextRenderer.MeasureText with WordBreak flag for accurate sizing.
    /// </summary>
    public static class TooltipService
    {
        // Single shared wrapped tooltip form instance (supports text wrapping)
        private static WrappedTooltipForm s_wrappedTooltip;
        private static Control s_lastOwner;
        private static readonly object s_lock = new object();

        private static void EnsureInitialized()
        {
            if (s_wrappedTooltip != null) return;

            lock (s_lock)
            {
                if (s_wrappedTooltip != null) return;

                // Create the custom wrapped tooltip form
                s_wrappedTooltip = new WrappedTooltipForm();
            }
        }

        /// <summary>
        /// Show tooltip relative to the specified owner control at the specified client position.
        /// Title is prepended to the text (joined with newline).
        /// This method marshals to the UI thread of the owner control automatically.
        /// </summary>
        /// <param name="owner">The control to anchor the tooltip to (must be non-null).</param>
        /// <param name="clientPosition">Point in owner.ClientCoordinates where the tooltip should appear (typically mouse location).</param>
        /// <param name="title">Short title/caption (can be null or empty).</param>
        /// <param name="text">Tooltip text body - will wrap if longer than 600px.</param>
        public static void ShowForControl(Control owner, Point clientPosition, string title, string text)
        {
            if (owner == null) return;
            EnsureInitialized();

            if (owner.IsDisposed) return;

            // Build combined text: include title if provided
            string fullText;
            if (!string.IsNullOrEmpty(title))
            {
                // Simple formatting: title on first line, description after
                fullText = title + Environment.NewLine + (text ?? string.Empty);
            }
            else
            {
                fullText = text ?? string.Empty;
            }

            // Show on UI thread of owner
            if (owner.InvokeRequired)
            {
                try
                {
                    owner.BeginInvoke(new Action(() => ShowInternal(owner, clientPosition, fullText)));
                }
                catch
                {
                    // swallow invocation errors
                }
            }
            else
            {
                ShowInternal(owner, clientPosition, fullText);
            }
        }

        private static void ShowInternal(Control owner, Point clientPosition, string text)
        {
            if (owner == null || owner.IsDisposed) return;
            EnsureInitialized();

            try
            {
                // Store last owner for Hide call
                s_lastOwner = owner;

                // Set the content - WrappedTooltipForm handles word wrapping automatically
                s_wrappedTooltip.Content = text;

                // Convert client position to screen coordinates
                Point screenLocation = owner.PointToScreen(clientPosition);

                // Show at the calculated screen location
                s_wrappedTooltip.ShowAt(owner, screenLocation);
            }
            catch
            {
                // best-effort: swallow exceptions to avoid crashing the UI if tooltip fails
            }
        }

        /// <summary>
        /// Hides any currently visible tooltip shown by this service.
        /// </summary>
        public static void Hide()
        {
            EnsureInitialized();

            try
            {
                if (s_wrappedTooltip != null && s_wrappedTooltip.Visible)
                {
                    s_wrappedTooltip.HideAnimated();
                }
            }
            catch
            {
                // ignore errors during hide
            }
            finally
            {
                s_lastOwner = null;
            }
        }
    }
}