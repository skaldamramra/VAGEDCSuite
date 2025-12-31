using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class AxisLockEventArgs : System.EventArgs
    {
        private int _y_axis_max_value;
        private string _mapname;
        private string _filename;
        private int _lock_mode;

        public int AxisMaxValue
        {
            get { return _y_axis_max_value; }
        }

        public int LockMode
        {
            get { return _lock_mode; }
        }

        public string SymbolName
        {
            get { return _mapname; }
        }

        public string Filename
        {
            get { return _filename; }
        }

        public AxisLockEventArgs(int max_value, int lockmode, string mapname, string filename)
        {
            this._y_axis_max_value = max_value;
            this._lock_mode = lockmode;
            this._mapname = mapname;
            this._filename = filename;
        }
    }
}
