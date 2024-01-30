using FactoryDiscreteEventSimulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class ClientGenerator:DESElement
    {
        ContinuousRandomGeneratorType interarrivalType;
        RandomVariateGenerator interarrivalTimeGenerator;
        List<Itinerary> itineraries;
        List<int> generationWeights;
        double totalProbability;
        private int totalCount;
        private double totalInterarrivalTime;
        private double squareSum;
        private int ceasesTime = 480;
        Random randomizer = new Random();
        ClientArrivalEvent clientArrivalEvent;
        int maximalClientCount = 10000;
        static int instanceCount;
        public int DropCount;
        #region Properties
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public ContinuousRandomGeneratorType InterarrivalType { get => interarrivalType;
            set {

                if (value != interarrivalType) {
                    interarrivalType = value;
                    interarrivalTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(interarrivalType);
                    }
                }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public RandomVariateGenerator InterarrivalTimeGenerator { get => interarrivalTimeGenerator; set => interarrivalTimeGenerator = value; }
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public int CeasesTime { get => ceasesTime; set => ceasesTime = value; }

        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int TotalCount { get => totalCount; }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double TotalInterarrivalTime { get => totalInterarrivalTime;}
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double AverageInterarrivalTime { 
            get
            {
                if(totalCount>0)
                    return totalInterarrivalTime / totalCount;

                return 0;
            }
        }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double InterarrivalTimeSTD
        {
            get
            {
                if (totalCount > 0)
                    return squareSum / totalCount- AverageInterarrivalTime*AverageInterarrivalTime;

                return 0;
            }
        }
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        [Editor(typeof(DESCollectionElementEditor), typeof(UITypeEditor))]
        public List<Itinerary> Itineraries
        {
            get => itineraries; set
            { 
                itineraries = value; 
            }
        }

        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public List<int> GenerationWeights { get => generationWeights; set => generationWeights = value; }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int DroppedCount { get => DropCount; }

        #endregion

        #region public interfacing functions
        public override void Draw(Graphics g)
        {
            // Create graphics path object and add ellipse.
            GraphicsPath graphPath = new GraphicsPath();

            PointF point1 = new PointF(Bound.X + Bound.Width / 2, Bound.Y);
            PointF point2 = new PointF(Bound.X, Bound.Y + Bound.Height / 2);
            PointF point3 = new PointF(Bound.Right, Bound.Y + Bound.Height / 2);
            PointF point4 = new PointF(Bound.X, Bound.Bottom);
            PointF point5 = new PointF(Bound.Right, Bound.Bottom); ;
            PointF[] ps = { point1, point3, point5, point4, point2 };
            graphPath.AddLines(ps);
            // Fill graphics path to screen.
            g.FillPath(new SolidBrush(BackColor), graphPath);

            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)(Bound.X + 0.5 * (Bound.Width - size.Width)), Bound.Bottom));
           
           // Rectangle circleBound = new Rectangle(GetCenterPoint(), new Size((int)(Bound.Width / 1.5), (int)(Bound.Height / 2)));
            Rectangle circleBound = new Rectangle(Bound.X+ (int)(Bound.Width / 3.5), Bound.Y + (int)(Bound.Height / 2), Bound.Width / 7 * 3, Bound.Height / 10 * 3);
            if (interarrivalType == ContinuousRandomGeneratorType.None)
                g.FillEllipse(Brushes.White, circleBound);
            else
                g.FillEllipse(Brushes.Brown, circleBound);

        }
        public ClientGenerator(List<Itinerary> itineraries, List<int> relativeProbability)
        {
            this.generationWeights = relativeProbability;
            foreach (int prob in relativeProbability)
                totalProbability += prob;

            this.itineraries = itineraries;
            InterarrivalType = ContinuousRandomGeneratorType.Exponential;
            clientArrivalEvent = new ClientArrivalEvent(this, null, double.NaN);
        }

        public ClientGenerator()
        {
            clientArrivalEvent = new ClientArrivalEvent(this, null, double.NaN);

            Name = $"Releaser{++instanceCount}";
            BackColor = Color.Tomato;
            itineraries = new List<Itinerary>();
            GenerationWeights = new List<int>();
        }

        public void UpdateNextClientArrivalEvent(double currentTime)
        {
            if (currentTime > ceasesTime) return;

            totalCount++;

            double intervalTime = interarrivalTimeGenerator.GetRandomVariate();
            if (intervalTime + currentTime > ceasesTime) return;

            totalInterarrivalTime += intervalTime;
            squareSum += intervalTime * intervalTime;

            //choose an itinerary
            double target = randomizer.NextDouble() * totalProbability;
            int itineraryId;
            double sum = 0;
            for (itineraryId = 0; itineraryId < itineraries.Count; itineraryId++)
            {
                sum += generationWeights[itineraryId];
                if (sum > target)
                    break;
            }

            Client client = new Client(currentTime + intervalTime, itineraries[itineraryId]);
            clientArrivalEvent.EventTime = currentTime + intervalTime;
            clientArrivalEvent.TheClient = client;
            clientArrivalEvent.AddToSimulationEngine();

        }
        #endregion


        #region Helping functions
        internal void Reset()
        {
            DropCount = 0;
            totalCount = 0;
            squareSum = 0;
            totalInterarrivalTime = 0;
            totalProbability = 0;

            //product shape
            for (int i = 0; i < itineraries.Count; i++)
            {
                itineraries[i].TheClientGenerator = this;
                itineraries[i].Shape = (GraphicUtil.Shape)i;
                totalProbability += generationWeights[i];
            }

        }

        internal void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"ClientArrivalTimeGeneratorType: {(int)interarrivalType}");
            if (interarrivalType != ContinuousRandomGeneratorType.None)
                interarrivalTimeGenerator.SaveToFile(sw);

            sw.WriteLine($"NumberOfItineraries: {itineraries.Count}");
            sw.WriteLine($"GenerationWeights: {String.Join<int>(",", generationWeights)}");
            foreach (Itinerary it in itineraries)
            {
                it.SaveToFile(sw);
            }
        }

        internal void GetSimulationResult(StringBuilder sb)
        {
            sb.AppendLine($"[{Name}] Cease Time:{ceasesTime:0.000} Clients Generated Count:{TotalCount} Drop Count:{DropCount}");
            sb.AppendLine($"Interarrival Time Average:{AverageInterarrivalTime:0.00} STD:{InterarrivalTimeSTD:0.00}");
        }

        internal void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);

            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            interarrivalType = (ContinuousRandomGeneratorType)Enum.Parse(typeof(ContinuousRandomGeneratorType), str, true);
            if (interarrivalType != ContinuousRandomGeneratorType.None)
            {
                interarrivalTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(interarrivalType);
                interarrivalTimeGenerator.ReadFromFile(sr);
            }

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            int itinerarryCount = int.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            string [] weights = str.Split(',');
            foreach (string weight in weights)
            {
                generationWeights.Add(int.Parse(weight));
            }

            for (int i = 0; i < itinerarryCount; i++)
            {
                Itinerary it = new Itinerary();
                it.ReadFromFile(sr);
                itineraries.Add(it);
            }
        }
        #endregion
    }
}
