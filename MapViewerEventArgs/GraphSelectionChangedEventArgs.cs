using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class GraphSelectionChangedEventArgs : System.EventArgs
    {
        private string _mapname;
        private int _tabpageindex;

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public int Tabpageindex
        {
            get { return _tabpageindex; }
            set { _tabpageindex = value; }
        }

        public GraphSelectionChangedEventArgs(int tabpageindex, string mapname)
        {
            this._tabpageindex = tabpageindex;
            this._mapname = mapname;
        }
    }
}
