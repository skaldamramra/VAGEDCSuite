using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class ReadSymbolEventArgs : System.EventArgs
    {
        private string _mapname;
        private string _filename;

        public string SymbolName
        {
            get { return _mapname; }
        }

        public string Filename
        {
            get { return _filename; }
        }

        public ReadSymbolEventArgs(string mapname, string filename)
        {
            this._mapname = mapname;
            this._filename = filename;
        }
    }
}
