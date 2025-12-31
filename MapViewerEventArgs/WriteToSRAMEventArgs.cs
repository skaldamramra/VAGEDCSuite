using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class WriteToSRAMEventArgs : System.EventArgs
    {
        private byte[] _data;
        private string _mapname;

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public WriteToSRAMEventArgs(string mapname, byte[] data)
        {
            this._mapname = mapname;
            this._data = data;
        }
    }
}
