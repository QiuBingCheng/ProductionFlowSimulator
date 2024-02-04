using System;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public class ContinuousRandomVariateGenerator:RandomVariateGenerator
    {
        public override Histogram GenerateRandomVariateGetHistogram(
          int numberOfInstances)
        {
            double[] instances = new double[numberOfInstances];
            double rand;
            double x_bar = 0;
            double std = 0;
            for (int i = 0; i < instances.Length; i++)
            {
                rand = GetRandomVariate();
                instances[i] = rand;
                x_bar += rand;
            }

            x_bar /= instances.Length;

            for (int i = 0; i < instances.Length; i++)
            {
                std += Math.Pow(x_bar - instances[i], 2);
            }
            Histogram his = new ContinuousDataHistogram(instances);
            his.SampleMean = x_bar;
            his.SampleStandardDeviation = Math.Sqrt(std / (numberOfInstances - 1));

            return his;
        }

        public Series GetDensityFunctionSeries(double start, double end, int resolution)
        {
            Series lineSeries = new Series();
            lineSeries.ChartType = SeriesChartType.Line;
            lineSeries.Color = Color.Red;
            double delta = (end - start) / (resolution - 1);
            for (double x = start; x <= end; x += delta)
            {
                lineSeries.Points.AddXY(x, GetProbabilityDensity(x));
            }

            return lineSeries;
        }

        protected virtual double GetProbabilityDensity(double x)
        {
            throw new NotImplementedException();
        }
    }
}
