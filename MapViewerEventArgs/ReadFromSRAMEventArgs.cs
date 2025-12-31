using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class ReadFromSRAMEventArgs : System.EventArgs
    {
        private string _mapname;

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public ReadFromSRAMEventArgs(string mapname)
        {
            this._mapname = mapname;
        }
    }
}
