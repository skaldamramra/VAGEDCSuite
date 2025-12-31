using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class SurfaceGraphViewChangedEventArgsEx : System.EventArgs
    {
        private string _mapname;
        private float _depthx;
        private float _depthy;
        private float _zoom;
        private float _rotation;
        private float _elevation;

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public float DepthX
        {
            get { return _depthx; }
            set { _depthx = value; }
        }

        public float DepthY
        {
            get { return _depthy; }
            set { _depthy = value; }
        }

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public float Elevation
        {
            get { return _elevation; }
            set { _elevation = value; }
        }

        public SurfaceGraphViewChangedEventArgsEx(float depthx, float depthy, float zoom, float rotation, float elevation, string mapname)
        {
            this._depthx = depthx;
            this._depthy = depthy;
            this._zoom = zoom;
            this._rotation = rotation;
            this._elevation = elevation;
            this._mapname = mapname;
        }
    }
}
