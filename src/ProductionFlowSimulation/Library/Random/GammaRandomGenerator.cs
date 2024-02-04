using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    class GammaRandomGenerator : ContinuousRandomVariateGenerator
    {
        private double shape;
        private double scale;
        private NormalRandomGenerator normalGenerator = new NormalRandomGenerator(0,1);
        private ContinuousUniformRandomGenerator UniformGenerator = new ContinuousUniformRandomGenerator();

        public GammaRandomGenerator(double shape=2,double scale=2)
        {
            this.shape = shape;
            this.scale = scale;
        }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double Shape { get => shape; set => shape = value; }
        [CategoryAttribute("Parameter"), DescriptionAttribute("")]
        public double Scale { get => scale; set => scale = value; }

        [Browsable(false)]
        public override double GetRandomVariate()
        {
            // Implementation based on "A Simple Method for Generating Gamma Variables"
            // by George Marsaglia and Wai Wan Tsang.  ACM Transactions on Mathematical Software
            // Vol 26, No 3, September 2000, pages 363-372.

            double d, c, x, xsquared, v, u;

            if (shape >= 1.0)
            {
                d = shape - 1.0 / 3.0;
                c = 1.0 / Math.Sqrt(9.0 * d);
                for (; ; )
                {
                    do
                    {
                        x = normalGenerator.GetRandomVariate();
                        v = 1.0 + c * x;
                    }
                    while (v <= 0.0);
                    v = v * v * v;
                    u = UniformGenerator.GetRandomVariate();
                    xsquared = x * x;
                    if (u < 1.0 - .0331 * xsquared * xsquared || Math.Log(u) < 0.5 * xsquared + d * (1.0 - v + Math.Log(v)))
                        return scale * d * v;
                }
            }
            
            string msg = string.Format("Shape must be greater than 0. Received {0}.", shape);
            throw new ArgumentOutOfRangeException(msg);
        }

        public override void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"Shape: {shape}");
            sw.WriteLine($"Scale: {scale}");
        }

        public override void ReadFromFile(StreamReader sr)
        {
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            shape = double.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            scale = double.Parse(str);
        }
    }
}
