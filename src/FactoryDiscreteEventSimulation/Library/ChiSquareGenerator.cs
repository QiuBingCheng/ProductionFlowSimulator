using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    class ChiSquareGenerator : ContinuousRandomVariateGenerator
    {
        private GammaRandomGenerator gammaGenerator = new GammaRandomGenerator(1,2);
        private int degreesOfFreedom;
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public int DegreesOfFreedom { get => degreesOfFreedom; set => degreesOfFreedom = value; }

        public ChiSquareGenerator(int degreesOfFreedom=2)
        {
            this.degreesOfFreedom = degreesOfFreedom;
        }

        [Browsable(false)]
        public override double GetRandomVariate()
        {
            gammaGenerator.Shape = 0.5 * degreesOfFreedom;
            return gammaGenerator.GetRandomVariate();
        }

        public override void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"DegreesOfFreedom: {degreesOfFreedom}");
        }

        public override void ReadFromFile(StreamReader sr)
        {
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            degreesOfFreedom = int.Parse(str);
        }
    }
}
