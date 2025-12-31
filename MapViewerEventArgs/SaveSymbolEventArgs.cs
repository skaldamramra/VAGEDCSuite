using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class SaveSymbolEventArgs : System.EventArgs
    {
        private int _address;
        private int _length;
        private byte[] _mapdata;
        private string _mapname;
        private string _filename;

        public int SymbolAddress
        {
            get { return _address; }
        }

        public int SymbolLength
        {
            get { return _length; }
        }

        public byte[] SymbolDate
        {
            get { return _mapdata; }
        }

        public string SymbolName
        {
            get { return _mapname; }
        }

        public string Filename
        {
            get { return _filename; }
        }

        public SaveSymbolEventArgs(int address, int length, byte[] mapdata, string mapname, string filename)
        {
            this._address = address;
            this._length = length;
            this._mapdata = mapdata;
            this._mapname = mapname;
            this._filename = filename;
        }
    }
}
