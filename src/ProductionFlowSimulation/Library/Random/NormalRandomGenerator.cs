using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class NormalRandomGenerator : ContinuousRandomVariateGenerator
    {
        private double mean;
        private double sigma;
        private double lowerBound;
        private double upperBound;

        public NormalRandomGenerator(double mean=0, double sigma=1)
        {
            this.mean = mean;
            this.sigma= sigma;

            lowerBound = 0;
            upperBound = mean+3 * sigma;
        }

        [Browsable(false)]
        public override double GetRandomVariate()
        {
          
            double u1 = 1.0 - randomizer.NextDouble();
            double u2 = 1.0 - randomizer.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2);
            double value = mean + sigma * randStdNormal;

            if (value < lowerBound)
                return lowerBound;
            else if (value > upperBound)
                return upperBound;
            else
                return value;
        }

        public override void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"Mean: {mean}");
            sw.WriteLine($"Sigma: {sigma}");
            sw.WriteLine($"LowerBound: {lowerBound}");
            sw.WriteLine($"UpperBound: {upperBound}");
        }

        public override void ReadFromFile(StreamReader sr)
        {
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            mean = double.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            sigma = double.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            lowerBound = double.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            upperBound = double.Parse(str);
        }

        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double Mean { get => mean; set => mean= value; }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double Sigma { get => sigma; set => sigma = value; }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double LowerBound { get => lowerBound; set => lowerBound = value; }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double UpperBound { get => upperBound; set => upperBound = value; }

        public override string ToString()
        {
            return $"Normal Random Generator ";
        }
    }
}
