using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace DiscreteEventSimulationLibrary
{
    public class ExponentialRandomGenerator : ContinuousRandomVariateGenerator
    {
        // data fileds

        private double mean;

        public ExponentialRandomGenerator(double mean=1)
        {
            this.mean = mean;

            theoreticMean = mean;
            theoreticStandardDeviation = mean;
        }

        public override void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"Mean: {mean}");
        }

        public override void ReadFromFile(StreamReader sr)
        {
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            mean = double.Parse(str);
        }

        [Browsable(false)]
        public override double GetRandomVariate()
        {
            return -mean * Math.Log(1.0 - randomizer.NextDouble());
        }

        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double Mean
        {
            get => mean;
            set
            {
                mean = value;
            }
        }
        public override string ToString()
        {
            return $"Exponential Random Generator ";
        }

    }
}