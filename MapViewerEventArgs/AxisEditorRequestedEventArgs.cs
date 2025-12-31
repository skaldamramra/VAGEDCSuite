using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class AxisEditorRequestedEventArgs : System.EventArgs
    {
        private string _mapname;
        private string _filename;
        private AxisIdent _axisident;

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public AxisIdent Axisident
        {
            get { return _axisident; }
            set { _axisident = value; }
        }

        public AxisEditorRequestedEventArgs(AxisIdent ident, string mapname, string filename)
        {
            this._axisident = ident;
            this._mapname = mapname;
            this._filename = filename;
        }
    }
}
