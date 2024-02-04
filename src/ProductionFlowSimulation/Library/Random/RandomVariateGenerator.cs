using System;
using System.ComponentModel;
using System.IO;

namespace DiscreteEventSimulationLibrary
{
    public class RandomVariateGenerator
    {
        protected Random randomizer;
        private int seed;
        protected double theoreticMean;
        protected double theoreticStandardDeviation;
        public RandomVariateGenerator()
        {
            this.seed = -1;
            randomizer = new Random();
            
        }
        public static RandomVariateGenerator CreateASpecificDistributionGenerator(ContinuousRandomGeneratorType type)
        {
            switch (type)
            {
                case ContinuousRandomGeneratorType.Exponential:
                    return new ExponentialRandomGenerator();
                case ContinuousRandomGeneratorType.Uniform:
                    return new ContinuousUniformRandomGenerator();
                case ContinuousRandomGeneratorType.Normal:
                    return new NormalRandomGenerator();
                case ContinuousRandomGeneratorType.Gamma:
                    return new GammaRandomGenerator();
                case ContinuousRandomGeneratorType.Chisquare:
                    return new ChiSquareGenerator();
            }
            return null;
        }
        public virtual void SaveToFile(StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        public virtual void ReadFromFile(StreamReader sr)
        {
            throw new NotImplementedException();
        }

        [CategoryAttribute("Status"), DescriptionAttribute("If seed is -1 that means new random state.")]
        public int Seed { get => seed; set => seed = value; }

        //Display(Order=2)]

        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double TheoreticStandardDeviation { get => theoreticStandardDeviation; }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double TheoreticMean { get => theoreticMean;}

        public void CreateRandomizer()
        {
            if(seed == -1)
                randomizer = new Random();
            else
            {
                randomizer = new Random(seed);
            }
        }


        /// <summary>
        /// Generate a random variable
        /// </summary>
        public virtual Histogram GenerateRandomVariateGetHistogram(int numberOfInstances)
        {
            throw new NotImplementedException();
        }

        public virtual double GetRandomVariate()
        {
            throw new NotImplementedException();
        }
        public virtual int GetRandomVariate(bool flag = true)
        {
            throw new NotImplementedException();
        }
      
    }
}