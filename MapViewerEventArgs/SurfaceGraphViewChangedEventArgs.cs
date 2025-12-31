using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class SurfaceGraphViewChangedEventArgs : System.EventArgs
    {
        private string _mapname;
        private int _pov_x;
        private int _pov_y;
        private int _pov_z;
        private int _pan_x;
        private int _pan_y;
        private double _pov_d;

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public int Pov_x
        {
            get { return _pov_x; }
            set { _pov_x = value; }
        }

        public int Pov_y
        {
            get { return _pov_y; }
            set { _pov_y = value; }
        }

        public int Pov_z
        {
            get { return _pov_z; }
            set { _pov_z = value; }
        }

        public int Pan_x
        {
            get { return _pan_x; }
            set { _pan_x = value; }
        }

        public int Pan_y
        {
            get { return _pan_y; }
            set { _pan_y = value; }
        }

        public double Pov_d
        {
            get { return _pov_d; }
            set { _pov_d = value; }
        }

        public SurfaceGraphViewChangedEventArgs(int povx, int povy, int povz, int panx, int pany, double povd, string mapname)
        {
            this._pan_x = panx;
            this._pan_y = pany;
            this._pov_d = povd;
            this._pov_x = povx;
            this._pov_y = povy;
            this._pov_z = povz;
            this._mapname = mapname;
        }
    }
}
