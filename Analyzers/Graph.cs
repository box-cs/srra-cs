using ScottPlot.Avalonia;

namespace srra.Analyzers
{
    internal class Graph
    {
        public double[]? xData { get; set; }
        public double[]? yData { get; set; }
        public AvaPlot avaPlot;
        public Graph(AvaPlot plot, string title, string xLabel, string yLabel)
        {
            plot.Plot.Clear();
            avaPlot = plot;
            avaPlot.Plot.Title(title);
            avaPlot.Plot.XLabel(xLabel);
            avaPlot.Plot.YLabel(yLabel);
            avaPlot.Plot.AxisAuto(0, 0.1);
        }

        public void ShowGraph()
        {
            if (xData is null || yData is null || xData.Length == 0 || yData.Length == 0) return;
            avaPlot.Plot.Palette = ScottPlot.Palette.Frost;
            avaPlot.Plot.AddScatter(xData, yData);
            avaPlot.Refresh();
        }
    }
}
