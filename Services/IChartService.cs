using System.Data;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Manages 3D and 2D chart rendering
    /// </summary>
    public interface IChartService
    {
        /// <summary>
        /// Configures 3D mesh surface chart
        /// </summary>
        void Configure3DChart(object chartControl, MapViewerState state);
        
        /// <summary>
        /// Updates 3D chart data
        /// </summary>
        void Update3DChartData(object surfaceSeries, DataTable data, ViewConfiguration config);
        
        /// <summary>
        /// Configures 2D line chart
        /// </summary>
        void Configure2DChart(object chartControl, MapMetadata metadata);
        
        /// <summary>
        /// Updates 2D chart slice data
        /// </summary>
        void Update2DChartSlice(object chartControl, byte[] data, int sliceIndex, MapViewerState state);

        /// <summary>
        /// Fills 3D surface data from DataTable
        /// </summary>
        void FillSurfaceData(Nevron.Chart.NMeshSurfaceSeries surface, System.Data.DataTable dt, ViewType viewType, double correctionFactor, double correctionOffset, bool isCompareViewer);

        /// <summary>
        /// Fills original map data to surface series
        /// </summary>
        void FillOriginalData(Nevron.Chart.NMeshSurfaceSeries surface, System.Data.DataTable dt, byte[] originalContent, bool isSixteenBit, ViewType viewType, double correctionFactor, double correctionOffset);

        /// <summary>
        /// Fills compare data to surface series
        /// </summary>
        void FillCompareData(Nevron.Chart.NMeshSurfaceSeries surface, System.Data.DataTable dt, byte[] compareContent, bool isSixteenBit, ViewType viewType, double correctionFactor, double correctionOffset);
    }
}
