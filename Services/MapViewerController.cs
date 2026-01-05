using System;
using System.Windows.Forms;
using ComponentFactory.Krypton.Navigator;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Orchestrates UI actions for MapViewerEx, decoupling the view from complex logic.
    /// </summary>
    public class MapViewerController
    {
        private readonly MapViewerEx _view;

        public MapViewerController(MapViewerEx view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void HandleDockingAction(Control parent, Action restoreAction, Action floatAction)
        {
            // In Krypton, the dockable unit is a KryptonPage.
            // Floating/Restoring is typically handled by the KryptonDockingManager
            // which manages the pages.
            if (parent is KryptonPage page)
            {
                // KryptonPage doesn't have MakeFloat/Restore directly like legacy DockPanel.
                // These actions are performed via the KryptonDockingManager.
                // For now, we neutralize this as the bulk of docking logic is in frmMain.KryptonDocking.cs
                floatAction();
            }
        }

        public ViewType GetViewTypeFromIndex(int index)
        {
            switch (index)
            {
                case 1: return ViewType.Decimal;
                case 2: return ViewType.Easy;
                case 3: return ViewType.ASCII;
                default: return ViewType.Hexadecimal;
            }
        }
    }
}