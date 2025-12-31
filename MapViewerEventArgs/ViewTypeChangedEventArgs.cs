using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class ViewTypeChangedEventArgs : System.EventArgs
    {
        private string _mapname;
        private ViewType _view;

        public string Mapname
        {
            get { return _mapname; }
            set { _mapname = value; }
        }

        public ViewType View
        {
            get { return _view; }
            set { _view = value; }
        }

        public ViewTypeChangedEventArgs(ViewType view, string mapname)
        {
            this._view = view;
            this._mapname = mapname;
        }
    }
}
