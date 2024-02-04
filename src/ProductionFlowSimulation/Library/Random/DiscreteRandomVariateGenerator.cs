using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public  class DiscreteRandomVariateGenerator:RandomVariateGenerator
    {
        public virtual int GetAIntegerRandomNumber()
        {
            throw new System.NotImplementedException();
        }

        public override Histogram GenerateRandomVariateGetHistogram(
            int numberOfInstances)
        {
            int[] instances = new int[numberOfInstances];
            int rand;
            double x_bar = 0;
            double std = 0;
            for (int i = 0; i < instances.Length; i++)
            {
                rand = GetAIntegerRandomNumber();
                instances[i] = rand;
                x_bar += rand;
            }

            x_bar /= instances.Length;

            for (int i = 0; i < instances.Length; i++)
            {
                std += Math.Pow(x_bar - instances[i], 2);
            }
            Histogram his = new DiscreteDataHistogram(instances);
            his.SampleMean = x_bar;
            his.SampleStandardDeviation = Math.Sqrt(std / (numberOfInstances - 1));

            return his;
        }

        public Series GetProbabilityMassFunctionSeries(int start, int end)
        {
            Series lineSeries = new Series();
            lineSeries.ChartType = SeriesChartType.Point;
            lineSeries.Color = Color.Red;
            for (int x = start; x <= end; x++)
            {
                lineSeries.Points.AddXY(x, GetProbabilityMass(x));
            }

            return lineSeries;
        }

        public virtual double GetProbabilityMass(int x)
        {
            throw new NotImplementedException();
        }
    }
}
