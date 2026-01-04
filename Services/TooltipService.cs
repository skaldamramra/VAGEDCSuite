using System;
using System.Drawing;
using System.Windows.Forms;

namespace VAGSuite.Services
{
    /// <summary>
    /// Simple static tooltip service that uses System.Windows.Forms.ToolTip internally.
    /// Replaces DevExpress.ToolTipController usage for programmatic show/hide.
    /// Designed for .NET 4.0 and Windows Forms.
    /// 
    /// Usage (example):
    ///     // show near cursor relative to owner control
    ///     TooltipService.ShowForControl(tvSymbols, tvSymbols.PointToClient(Cursor.Position), "Varname", "Description...");
    ///     // hide
    ///     TooltipService.Hide();
    /// 
    /// Rationale:
    /// - Verified that WinForms.ToolTip supports programmatic Show(Control, Point, int).
    /// - Using a simple wrapper centralizes behavior and lets us swap implementations later
    ///   (for example a custom Form-based tooltip) without touching callers.
    /// </summary>
    public static class TooltipService
    {
        // Single shared ToolTip instance
        private static ToolTip s_tooltip;
        private static Control s_lastOwner;
        private static readonly object s_lock = new object();

        // Default persistent duration in milliseconds when Show is used; we'll keep very large
        // and rely on explicit Hide() to remove the tooltip.
        private const int PersistentDurationMs = Int32.MaxValue / 1000; // reduce risk of overflow in some implementations

        private static void EnsureInitialized()
        {
            if (s_tooltip != null) return;

            lock (s_lock)
            {
                if (s_tooltip != null) return;

                s_tooltip = new ToolTip();
                // We will manage showing/hiding manually; keep delays reasonable for other usages.
                try
                {
                    s_tooltip.AutoPopDelay = 60000; // fallback long duration; explicit Hide also used
                    s_tooltip.InitialDelay = 200;
                    s_tooltip.ReshowDelay = 100;
                    s_tooltip.ShowAlways = true; // show even when parent form is inactive (subject to OS behavior)
                }
                catch
                {
                    // On older frameworks some properties might throw in rare cases; ignore and continue
                }
            }
        }

        /// <summary>
        /// Show tooltip relative to the specified owner control at the specified client position.
        /// Title is prepended to the text (joined with newline) because WinForms.ToolTip doesn't support separate captions.
        /// This method marshals to the UI thread of the owner control automatically.
        /// </summary>
        /// <param name="owner">The control to anchor the tooltip to (must be non-null).</param>
        /// <param name="clientPosition">Point in owner.ClientCoordinates where the tooltip should appear (typically mouse location).</param>
        /// <param name="title">Short title/caption (can be null or empty).</param>
        /// <param name="text">Tooltip text body.</param>
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

            // Hide any previous tooltip on different control
            if (s_lastOwner != null && s_lastOwner != owner)
            {
                try { s_tooltip.Hide(s_lastOwner); } catch { }
                s_lastOwner = null;
            }

            // Offset so the tooltip doesn't overlap the cursor exactly
            Point offsetPoint = new Point(clientPosition.X + 12, clientPosition.Y + 12);

            try
            {
                // Store last owner for Hide call
                s_lastOwner = owner;

                // Show with a very long duration and rely on explicit Hide on mouse leave.
                // The ToolTip.Show overload expects the point to be client coordinates relative to owner.
                s_tooltip.Show(text, owner, offsetPoint, PersistentDurationMs);
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
                if (s_lastOwner != null && !s_lastOwner.IsDisposed)
                {
                    if (s_lastOwner.InvokeRequired)
                    {
                        try { s_lastOwner.BeginInvoke(new Action(() => s_tooltip.Hide(s_lastOwner))); }
                        catch { }
                    }
                    else
                    {
                        s_tooltip.Hide(s_lastOwner);
                    }
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