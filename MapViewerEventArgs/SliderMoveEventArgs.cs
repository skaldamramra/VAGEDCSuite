using System;

namespace VAGSuite.MapViewerEventArgs
{
    using VAGSuite;

    public class SliderMoveEventArgs : System.EventArgs
    {
        private int _slider_position;
        private string _mapname;
        private string _filename;

        public int SliderPosition
        {
            get { return _slider_position; }
        }

        public string SymbolName
        {
            get { return _mapname; }
        }

        public string Filename
        {
            get { return _filename; }
        }

        public SliderMoveEventArgs(int slider_position, string mapname, string filename)
        {
            this._slider_position = slider_position;
            this._mapname = mapname;
            this._filename = filename;
        }
    }
}
