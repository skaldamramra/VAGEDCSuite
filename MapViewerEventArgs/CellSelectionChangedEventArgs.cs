using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class CellSelectionChangedEventArgs : System.EventArgs
    {
        private int _rowhandle;
        private int _colindex;
        private string _mapname;

        public int Rowhandle
        {
            get { return _rowhandle; }
            set { _rowhandle = value; }
        }

        public int Colindex
        {
            get { return _colindex; }
            set { _colindex = value; }
        }

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public CellSelectionChangedEventArgs(int rowhandle, int colindex, string mapname)
        {
            this._rowhandle = rowhandle;
            this._colindex = colindex;
            this._mapname = mapname;
        }
    }
}
