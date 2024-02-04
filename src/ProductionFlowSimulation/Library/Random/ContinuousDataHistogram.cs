using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    class ContinuousDataHistogram:Histogram
    {
        double[] instances;
        public ContinuousDataHistogram(double[] instances)
        {
            this.instances = instances;
            updateItems();
        }
        public override Series histogramChart
        {
            get
            {

                Series hisStep = new Series();
                hisStep.ChartType = SeriesChartType.StepLine;
                hisStep.Color = Color.Green;
                hisStep.BorderWidth = 3;
                for (int i = 0; i < Items.Length; i++)
                {
                    hisStep.Points.AddXY(Items[i].value, Items[i].count / (double)instances.Length);
                }
                return hisStep;
            }
        }

        public override bool updateItems()
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            foreach (double i in instances)
            {
                if (i > max) max = i;
                if (i < min) min = i;
            }
            binWidth = (max - min) / (NumberOfBins - 1);
            Items = new ItemInHistogram[NumberOfBins];

            double offset = binWidth / 2;
            for (int i = 0; i < NumberOfBins; i++)
            {
                Items[i] = new ItemInHistogram();
                Items[i].value = min + i * binWidth;
                Items[i].count = 0;
            }

            for (int i = 0; i < instances.Length; i++)
            {
                for (int j = 0; j < NumberOfBins; j++)
                {
                    if (Items[j].value - offset <= instances[i] && instances[i] < Items[j].value + offset)
                    {
                        Items[j].count++;
                        break;
                    }
                }
            }
            return true;
        }
    }
}
