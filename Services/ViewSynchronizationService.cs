using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using VAGSuite;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service for handling view synchronization between multiple map viewers
    /// Extracted from frmMain.cs to improve maintainability
    /// </summary>
    public class ViewSynchronizationService
    {
        private AppSettings _appSettings;
        
        public ViewSynchronizationService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        #region View Type Changed

        /// <summary>
        /// Synchronizes view type changes across all map viewers
        /// </summary>
        public void OnViewTypeChanged(object sender, MapViewerEx.ViewTypeChangedEventArgs e, DockManager dockManager)
        {
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (DockPanel pnl in dockManager.Panels)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is MapViewerEx)
                        {
                            if (c != sender)
                            {
                                MapViewerEx vwr = (MapViewerEx)c;
                                if (vwr.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr.Viewtype = e.View;
                                    vwr.ReShowTable();
                                    vwr.Invalidate();
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel tpnl = (DockPanel)c;
                            foreach (Control c2 in tpnl.Controls)
                            {
                                if (c2 is MapViewerEx)
                                {
                                    if (c2 != sender)
                                    {
                                        MapViewerEx vwr2 = (MapViewerEx)c2;
                                        if (vwr2.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr2.Viewtype = e.View;
                                            vwr2.ReShowTable();
                                            vwr2.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel cntr = (DockPanel)c;
                            foreach (Control c3 in cntr.Controls)
                            {
                                if (c3 is MapViewerEx)
                                {
                                    if (c3 != sender)
                                    {
                                        MapViewerEx vwr3 = (MapViewerEx)c3;
                                        if (vwr3.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr3.Viewtype = e.View;
                                            vwr3.ReShowTable();
                                            vwr3.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Surface Graph View Changed

        /// <summary>
        /// Synchronizes surface graph view changes across all map viewers
        /// </summary>
        public void OnSurfaceGraphViewChangedEx(object sender, MapViewerEx.SurfaceGraphViewChangedEventArgsEx e, DockManager dockManager)
        {
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (DockPanel pnl in dockManager.Panels)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is MapViewerEx)
                        {
                            if (c != sender)
                            {
                                MapViewerEx vwr = (MapViewerEx)c;
                                if (vwr.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr.SetSurfaceGraphViewEx(e.DepthX, e.DepthY, e.Zoom, e.Rotation, e.Elevation);
                                    vwr.Invalidate();
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel tpnl = (DockPanel)c;
                            foreach (Control c2 in tpnl.Controls)
                            {
                                if (c2 is MapViewerEx)
                                {
                                    if (c2 != sender)
                                    {
                                        MapViewerEx vwr2 = (MapViewerEx)c2;
                                        if (vwr2.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr2.SetSurfaceGraphViewEx(e.DepthX, e.DepthY, e.Zoom, e.Rotation, e.Elevation);
                                            vwr2.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel cntr = (DockPanel)c;
                            foreach (Control c3 in cntr.Controls)
                            {
                                if (c3 is MapViewerEx)
                                {
                                    if (c3 != sender)
                                    {
                                        MapViewerEx vwr3 = (MapViewerEx)c3;
                                        if (vwr3.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr3.SetSurfaceGraphViewEx(e.DepthX, e.DepthY, e.Zoom, e.Rotation, e.Elevation);
                                            vwr3.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Splitter Moved

        /// <summary>
        /// Synchronizes splitter position changes across all map viewers
        /// </summary>
        public void OnSplitterMoved(object sender, MapViewerEx.SplitterMovedEventArgs e, DockManager dockManager)
        {
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (DockPanel pnl in dockManager.Panels)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is MapViewerEx)
                        {
                            if (c != sender)
                            {
                                MapViewerEx vwr = (MapViewerEx)c;
                                if (vwr.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr.SetSplitter(e.Panel1height, e.Panel2height, e.Splitdistance, e.Panel1collapsed, e.Panel2collapsed);
                                    vwr.Invalidate();
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel tpnl = (DockPanel)c;
                            foreach (Control c2 in tpnl.Controls)
                            {
                                if (c2 is MapViewerEx)
                                {
                                    if (c2 != sender)
                                    {
                                        MapViewerEx vwr2 = (MapViewerEx)c2;
                                        if (vwr2.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr2.SetSplitter(e.Panel1height, e.Panel2height, e.Splitdistance, e.Panel1collapsed, e.Panel2collapsed);
                                            vwr2.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel cntr = (DockPanel)c;
                            foreach (Control c3 in cntr.Controls)
                            {
                                if (c3 is MapViewerEx)
                                {
                                    if (c3 != sender)
                                    {
                                        MapViewerEx vwr3 = (MapViewerEx)c3;
                                        if (vwr3.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr3.SetSplitter(e.Panel1height, e.Panel2height, e.Splitdistance, e.Panel1collapsed, e.Panel2collapsed);
                                            vwr3.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Selection Changed

        /// <summary>
        /// Synchronizes cell selection changes across all map viewers
        /// </summary>
        public void OnSelectionChanged(object sender, MapViewerEx.CellSelectionChangedEventArgs e, DockManager dockManager)
        {
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (DockPanel pnl in dockManager.Panels)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is MapViewerEx)
                        {
                            if (c != sender)
                            {
                                MapViewerEx vwr = (MapViewerEx)c;
                                if (vwr.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr.SelectCell(e.Rowhandle, e.Colindex);
                                    vwr.Invalidate();
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel tpnl = (DockPanel)c;
                            foreach (Control c2 in tpnl.Controls)
                            {
                                if (c2 is MapViewerEx)
                                {
                                    if (c2 != sender)
                                    {
                                        MapViewerEx vwr2 = (MapViewerEx)c2;
                                        if (vwr2.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr2.SelectCell(e.Rowhandle, e.Colindex);
                                            vwr2.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel cntr = (DockPanel)c;
                            foreach (Control c3 in cntr.Controls)
                            {
                                if (c3 is MapViewerEx)
                                {
                                    if (c3 != sender)
                                    {
                                        MapViewerEx vwr3 = (MapViewerEx)c3;
                                        if (vwr3.Map_name == e.Mapname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                        {
                                            vwr3.SelectCell(e.Rowhandle, e.Colindex);
                                            vwr3.Invalidate();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Slider Move

        /// <summary>
        /// Synchronizes slider position changes across all map viewers
        /// </summary>
        public void OnSliderMove(object sender, MapViewerEx.SliderMoveEventArgs e, DockManager dockManager)
        {
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                SetMapSliderPosition(e.Filename, e.SymbolName, e.SliderPosition, dockManager);
            }
        }

        /// <summary>
        /// Sets the slider position for all viewers showing a specific symbol
        /// </summary>
        public void SetMapSliderPosition(string filename, string symbolname, int sliderposition, DockManager dockManager)
        {
            foreach (DockPanel pnl in dockManager.Panels)
            {
                foreach (Control c in pnl.Controls)
                {
                    if (c is MapViewerEx)
                    {
                        MapViewerEx vwr = (MapViewerEx)c;
                        if (vwr.Map_name == symbolname)
                        {
                            vwr.SliderPosition = sliderposition;
                            vwr.Invalidate();
                        }
                    }
                    else if (c is DockPanel)
                    {
                        DockPanel tpnl = (DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is MapViewerEx)
                            {
                                MapViewerEx vwr2 = (MapViewerEx)c2;
                                if (vwr2.Map_name == symbolname)
                                {
                                    vwr2.SliderPosition = sliderposition;
                                    vwr2.Invalidate();
                                }
                            }
                        }
                    }
                    else if (c is DockPanel)
                    {
                        DockPanel cntr = (DockPanel)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is MapViewerEx)
                            {
                                MapViewerEx vwr3 = (MapViewerEx)c3;
                                if (vwr3.Map_name == symbolname)
                                {
                                    vwr3.SliderPosition = sliderposition;
                                    vwr3.Invalidate();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Set Map Scale

        /// <summary>
        /// Sets the scale (axis max value and lock mode) for all viewers showing a specific symbol
        /// </summary>
        public void SetMapScale(string filename, string symbolname, int axismax, int lockmode, DockManager dockManager)
        {
            foreach (DockPanel pnl in dockManager.Panels)
            {
                foreach (Control c in pnl.Controls)
                {
                    if (c is MapViewerEx)
                    {
                        MapViewerEx vwr = (MapViewerEx)c;
                        if (vwr.Map_name == symbolname || _appSettings.SynchronizeMapviewersDifferentMaps)
                        {
                            vwr.Max_y_axis_value = axismax;
                            vwr.LockMode = lockmode;
                            vwr.Invalidate();
                        }
                    }
                    else if (c is DockPanel)
                    {
                        DockPanel tpnl = (DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is MapViewerEx)
                            {
                                MapViewerEx vwr2 = (MapViewerEx)c2;
                                if (vwr2.Map_name == symbolname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr2.Max_y_axis_value = axismax;
                                    vwr2.LockMode = lockmode;
                                    vwr2.Invalidate();
                                }
                            }
                        }
                    }
                    else if (c is DockPanel)
                    {
                        DockPanel cntr = (DockPanel)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is MapViewerEx)
                            {
                                MapViewerEx vwr3 = (MapViewerEx)c3;
                                if (vwr3.Map_name == symbolname || _appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr3.Max_y_axis_value = axismax;
                                    vwr3.LockMode = lockmode;
                                    vwr3.Invalidate();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Find Max Table Value

        /// <summary>
        /// Finds the maximum value in a table across all viewers showing a specific symbol
        /// </summary>
        public int FindMaxTableValue(string symbolname, int orgvalue, DockManager dockManager)
        {
            int retval = orgvalue;
            foreach (DockPanel pnl in dockManager.Panels)
            {
                foreach (Control c in pnl.Controls)
                {
                    if (c is MapViewerEx)
                    {
                        MapViewerEx vwr = (MapViewerEx)c;
                        if (vwr.Map_name == symbolname)
                        {
                            if (vwr.MaxValueInTable > retval) retval = vwr.MaxValueInTable;
                        }
                    }
                    else if (c is DockPanel)
                    {
                        DockPanel tpnl = (DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is MapViewerEx)
                            {
                                MapViewerEx vwr2 = (MapViewerEx)c2;
                                if (vwr2.Map_name == symbolname)
                                {
                                    if (vwr2.MaxValueInTable > retval) retval = vwr2.MaxValueInTable;
                                }
                            }
                        }
                    }
                    else if (c is DockPanel)
                    {
                        DockPanel cntr = (DockPanel)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is MapViewerEx)
                            {
                                MapViewerEx vwr3 = (MapViewerEx)c3;
                                if (vwr3.Map_name == symbolname)
                                {
                                    if (vwr3.MaxValueInTable > retval) retval = vwr3.MaxValueInTable;
                                }
                            }
                        }
                    }
                }
            }
            return retval;
        }

        #endregion

        #region Axis Lock

        /// <summary>
        /// Handles axis lock event and synchronizes across all viewers
        /// </summary>
        public void OnAxisLock(object sender, MapViewerEx.AxisLockEventArgs e, DockManager dockManager)
        {
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                int axismaxvalue = e.AxisMaxValue;
                if (e.LockMode == 1)
                {
                    axismaxvalue = FindMaxTableValue(e.SymbolName, axismaxvalue, dockManager);
                }
                SetMapScale(e.Filename, e.SymbolName, axismaxvalue, e.LockMode, dockManager);
            }
        }

        #endregion
    }
}
