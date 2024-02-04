using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    class ConstantRandomGenerator : ContinuousRandomVariateGenerator
    {
        private double mean;

        public ConstantRandomGenerator(double mean)
        {
            this.mean = mean;
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
            return mean;
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
            return $"Constant Random Generator";
        }


    }
}
