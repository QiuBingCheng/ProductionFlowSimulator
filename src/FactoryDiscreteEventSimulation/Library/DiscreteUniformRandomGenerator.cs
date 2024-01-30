using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class DiscreteUniformRandomGenerator: DiscreteRandomVariateGenerator
    {
        // data fileds
        int lowerBound;
        int upperBound;

        public DiscreteUniformRandomGenerator(int lowerBound,int upperBound)
        {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public int UpperBounds { get => upperBound; set => upperBound = value; }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public int LowerBounds { get => lowerBound; set => lowerBound = value; }

        public override int GetAIntegerRandomNumber()
        {

            return randomizer.Next(lowerBound, upperBound + 1);
        }
    }
}
