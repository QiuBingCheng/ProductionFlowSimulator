using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DiscreteEventSimulationLibrary
{
    public class ContinuousUniformRandomGenerator : RandomVariateGenerator
    {
        // data fileds
        double lowerBound;
        double upperBound;
        /// <summary>
        ///  
        /// </summary>
        /// <param name="lowerBounds">The lower bounds of variate</param>
        /// <param name="upperBounds">The upper bounds of variate</param>
        public ContinuousUniformRandomGenerator(double lowerBounds=0,double upperBounds=1)
        {
            this.lowerBound = lowerBounds;
            this.upperBound = upperBounds;
        }

        [CategoryAttribute("Parameter"), DescriptionAttribute(""), Display(Order=2)]
        public double UpperBound { get => upperBound; set => upperBound = value; }
        [CategoryAttribute("Parameter"), DescriptionAttribute(""),Display(Order =1)]
        public double LowerBound { get => lowerBound; set => lowerBound = value; }

        public override double GetRandomVariate()
        {
            return lowerBound + randomizer.NextDouble() * (upperBound - lowerBound);
        }

        public override void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"Lower Bound: {lowerBound}");
            sw.WriteLine($"Upper Bound: {upperBound}");
        }

        public override void ReadFromFile(StreamReader sr)
        {
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            lowerBound = double.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            upperBound = double.Parse(str);
        }
        public override string ToString()
        {
            return $"Uniform Random Generator ";
        }
    }
}
