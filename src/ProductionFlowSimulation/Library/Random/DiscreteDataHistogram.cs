using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    class DiscreteDataHistogram:Histogram
    {

        int[] instances;

        public DiscreteDataHistogram(int[] instances)
        {
            this.instances = instances;
            updateItems();
        }

        public override Series histogramChart
        {
            get
            {

                Series hisStep = new Series();
                hisStep.ChartType = SeriesChartType.Column;
                hisStep.Color = Color.Green;
                for (int i = 0; i < Items.Length; i++)
                {
                    hisStep.Points.AddXY(Items[i].value, Items[i].count / (double)instances.Length);
                }
                return hisStep;
            }
        }



        public override bool updateItems()
        {

            foreach (int i in instances)
            {
                if (i > MaxValue) MaxValue = i;
                if (i < MinValue) MinValue = i;
            }

            NumberOfBins = MaxValue - MinValue + 1;
            Items = new ItemInHistogram[NumberOfBins];
            for (int i = 0; i < NumberOfBins; i++)
            {
                Items[i] = new ItemInHistogram();
                Items[i].value = MinValue + i;
                Items[i].count = 0;
            }

            for (int i = 0; i < instances.Length; i++)
            {
                for (int j = 0; j < NumberOfBins; j++)
                {
                    if (Items[j].value == instances[i])
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
