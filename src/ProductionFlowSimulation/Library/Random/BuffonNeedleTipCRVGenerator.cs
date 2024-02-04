using System;
using System.ComponentModel;

namespace DiscreteEventSimulationLibrary
{
    class BuffonNeedleTipCRVGenerator : RandomVariateGenerator
    {
        private double length;
        private double gap;

        public BuffonNeedleTipCRVGenerator(double gap,double len)
        {
            this.gap = gap;
            length = len;
        }

        [Browsable(false)]
        public override double GetRandomVariate()
        {
            double angle = -Math.PI / 2+randomizer.NextDouble()*Math.PI;
            double distance = gap * randomizer.NextDouble();
            double total = distance + length * Math.Sin(angle);
            return total;
        }
    }
}
