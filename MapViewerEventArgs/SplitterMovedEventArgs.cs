using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class SplitterMovedEventArgs : System.EventArgs
    {
        private int _splitdistance;
        private int _panel1height;
        private int _panel2height;
        private bool _panel1collapsed;
        private bool _panel2collapsed;
        private string _mapname;

        public int Splitdistance
        {
            get { return _splitdistance; }
            set { _splitdistance = value; }
        }

        public int Panel1height
        {
            get { return _panel1height; }
            set { _panel1height = value; }
        }

        public int Panel2height
        {
            get { return _panel2height; }
            set { _panel2height = value; }
        }

        public bool Panel1collapsed
        {
            get { return _panel1collapsed; }
            set { _panel1collapsed = value; }
        }

        public bool Panel2collapsed
        {
            get { return _panel2collapsed; }
            set { _panel2collapsed = value; }
        }

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public SplitterMovedEventArgs(int panel1height, int panel2height, int splitdistance, bool panel1collapsed, bool panel2collapsed, string mapname)
        {
            this._splitdistance = splitdistance;
            this._panel1collapsed = panel1collapsed;
            this._panel1height = panel1height;
            this._panel2collapsed = panel2collapsed;
            this._panel2height = panel2height;
            this._mapname = mapname;
        }
    }
}
