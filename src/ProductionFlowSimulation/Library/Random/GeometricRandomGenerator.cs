using System;
using System.ComponentModel;

namespace DiscreteEventSimulationLibrary
{
    // data fileds

    class GeometricRandomGenerator : DiscreteRandomVariateGenerator
    {
        double mean;
     
        public GeometricRandomGenerator(double mean)
        {
            this.mean = mean;
        }

        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double Mean { get => mean; set => mean = value; }

        public override double GetRandomVariate()
        {

            return Math.Floor(-mean * Math.Log(1 - mean));
        }
    }
}
