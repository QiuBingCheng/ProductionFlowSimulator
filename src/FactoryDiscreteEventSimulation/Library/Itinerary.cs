using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class Itinerary:DESElement
    {
        List<Visit> visits = new List<Visit>();
        private double priorityWeight;
        static int instanceCount;
        private GraphicUtil.Shape shape;
        private ClientGenerator clientGenerator;

        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public GraphicUtil.Shape Shape { get => shape; set => shape = value; }

        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public double PriorityWeight { get => priorityWeight; set => priorityWeight = value; }
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public List<Visit> Visits { get => visits; set => visits = value; }
        public ClientGenerator TheClientGenerator { get => clientGenerator; set => clientGenerator = value; }

        public Itinerary()
        {
            List<ServiceNode> serviceNodes = new List<ServiceNode>();
            //List<RandomVariateGenerator> timeGerarator = new List<RandomVariateGenerator>();
            BackColor = Color.LightBlue;
            Name = $"Itinerary{++instanceCount}";
        }
        public void ResumeNodeReference(List<ServiceNode>allNodes)
        {
            foreach (Visit visit in visits)
            {
                foreach (ServiceNode sn in allNodes)
                {
                    if (sn.hashCode == visit.theNodeHashCode)
                    {
                        visit.TheNode = sn;
                        break;
                    }
                }
            }
        }
        public override void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"Priority Weight:{priorityWeight}");
            sw.WriteLine($"NumberofVisits: {visits.Count}");
            foreach (Visit visit in Visits)
            {
                visit.SaveToFile(sw);
            }

        }
        public override void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);

            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            priorityWeight = int.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            int numberOfVisits = int.Parse(str);

            for (int i = 0; i < numberOfVisits; i++)
            {
                Visit visit = new Visit();
                visit.ReadFromFile(sr);
                visits.Add(visit);
            }   
        }

        public override void Draw(Graphics g)
        {
            SolidBrush sb = new SolidBrush(BackColor);
            PointF point1 = new PointF(Bound.X+ Bound.Width/ 4, Bound.Y);
            PointF point2 = new PointF(Bound.Right, Bound.Y);
            PointF point3 = new PointF(Bound.X, Bound.Y+Bound.Height/2);
            PointF point4 = new PointF(Bound.X + Bound.Width / 4, Bound.Bottom);
            PointF point5 = new PointF(Bound.Right, Bound.Bottom);
            PointF[] curvePoints = { point1, point2, point5, point4, point3 };
            g.FillPolygon(sb, curvePoints);

            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)(Bound.X + 0.5 * (Bound.Width - size.Width)), Bound.Bottom));

        }
      
    }
}
