using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public class Histogram
    {
        private ItemInHistogram[] items;

        private int numberOfBins = 20;
        protected double binWidth;
        public virtual Series histogramChart { get; }


        private double sampleMean;
        private double sampleStandardDeviation;


        int maxValue = Int32.MinValue;
        int minValue = Int32.MaxValue;

        public int MinValue { get => minValue; set => minValue = value; }
        public int MaxValue { get => maxValue; set => maxValue = value; }

        public int NumberOfBins
        {
            get => numberOfBins;
            set
            {
                if (value != numberOfBins)
                {
                    numberOfBins = value;
                    updateItems();
                }

            }
        }

        public ItemInHistogram[] Items { get => items; set => items = value; }
        public double SampleMean { get => sampleMean; set => sampleMean = value; }
        public double SampleStandardDeviation { get => sampleStandardDeviation; set => sampleStandardDeviation = value; }

        public virtual bool updateItems()
        {
            throw new Exception("No Implementation!");
        }
    }
}
