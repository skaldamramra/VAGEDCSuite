using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ComponentFactory.Krypton.Docking;
using ComponentFactory.Krypton.Navigator;
using VAGSuite;
using VAGSuite.MapViewerEventArgs;

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
        public void OnViewTypeChanged(object sender, MapViewerEventArgs.ViewTypeChangedEventArgs e, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (KryptonPage pnl in dockManager.Pages)
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
                    }
                }
            }
        }

        #endregion

        #region Surface Graph View Changed

        /// <summary>
        /// Synchronizes surface graph view changes across all map viewers
        /// </summary>
        public void OnSurfaceGraphViewChangedEx(object sender, MapViewerEventArgs.SurfaceGraphViewChangedEventArgsEx e, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (KryptonPage pnl in dockManager.Pages)
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
                    }
                }
            }
        }

        #endregion

        #region Splitter Moved

        /// <summary>
        /// Synchronizes splitter position changes across all map viewers
        /// </summary>
        public void OnSplitterMoved(object sender, MapViewerEventArgs.SplitterMovedEventArgs e, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (KryptonPage pnl in dockManager.Pages)
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
                    }
                }
            }
        }

        #endregion

        #region Selection Changed

        /// <summary>
        /// Synchronizes cell selection changes across all map viewers
        /// </summary>
        public void OnSelectionChanged(object sender, MapViewerEventArgs.CellSelectionChangedEventArgs e, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                foreach (KryptonPage pnl in dockManager.Pages)
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
                    }
                }
            }
        }

        #endregion

        #region Slider Move

        /// <summary>
        /// Synchronizes slider position changes across all map viewers
        /// </summary>
        public void OnSliderMove(object sender, MapViewerEventArgs.SliderMoveEventArgs e, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            if (_appSettings.SynchronizeMapviewers || _appSettings.SynchronizeMapviewersDifferentMaps)
            {
                SetMapSliderPosition(e.Filename, e.SymbolName, e.SliderPosition, dockManager);
            }
        }

        /// <summary>
        /// Sets the slider position for all viewers showing a specific symbol
        /// </summary>
        public void SetMapSliderPosition(string filename, string symbolname, int sliderposition, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            foreach (KryptonPage pnl in dockManager.Pages)
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
                }
            }
        }

        #endregion

        #region Set Map Scale

        /// <summary>
        /// Sets the scale (axis max value and lock mode) for all viewers showing a specific symbol
        /// </summary>
        public void SetMapScale(string filename, string symbolname, int axismax, int lockmode, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return;
            foreach (KryptonPage pnl in dockManager.Pages)
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
                }
            }
        }

        #endregion

        #region Find Max Table Value

        /// <summary>
        /// Finds the maximum value in a table across all viewers showing a specific symbol
        /// </summary>
        public int FindMaxTableValue(string symbolname, int orgvalue, KryptonDockingManager dockManager)
        {
            if (dockManager == null) return orgvalue;
            int retval = orgvalue;
            foreach (KryptonPage pnl in dockManager.Pages)
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
                }
            }
            return retval;
        }

        #endregion

        #region Axis Lock

        /// <summary>
        /// Handles axis lock event and synchronizes across all viewers
        /// </summary>
        public void OnAxisLock(object sender, MapViewerEventArgs.AxisLockEventArgs e, KryptonDockingManager dockManager)
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
