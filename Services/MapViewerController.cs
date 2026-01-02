using System;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
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
            if (parent is DockPanel pnl)
            {
                if (pnl.FloatForm == null)
                {
                    pnl.FloatSize = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    pnl.FloatLocation = new System.Drawing.Point(1, 1);
                    pnl.MakeFloat();
                    floatAction();
                }
                else
                {
                    pnl.Restore();
                    restoreAction();
                }
            }
            else if (parent is ControlContainer container)
            {
                if (container.Panel.FloatForm == null)
                {
                    container.Panel.FloatSize = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    container.Panel.FloatLocation = new System.Drawing.Point(1, 1);
                    container.Panel.MakeFloat();
                    floatAction();
                }
                else
                {
                    container.Panel.Restore();
                    restoreAction();
                }
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