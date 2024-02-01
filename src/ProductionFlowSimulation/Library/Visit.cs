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
    public class Visit:DESElement
    {
        private ServiceNode theNode;
        public int theNodeHashCode;
        private RandomVariateGenerator serviceTimeGenerator;
        private ContinuousRandomGeneratorType serviceTimeGeneratorType;
        public PointF[] polygonPoints;
        static int count;
        public Visit()
        {
            Name = $"Generator{++count}";
            ServiceTimeGeneratorType = ContinuousRandomGeneratorType.Normal;
            BackColor = Color.LightBlue;
        }
        #region
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public ServiceNode TheNode { get => theNode; set => theNode = value; }

        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public ContinuousRandomGeneratorType ServiceTimeGeneratorType
        {
            get => serviceTimeGeneratorType;
            set
            {

                if (value != serviceTimeGeneratorType)
                {
                    serviceTimeGeneratorType = value;
                    serviceTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(serviceTimeGeneratorType);
                }
            }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public RandomVariateGenerator ServiceTimeGenerator { get => serviceTimeGenerator; set => serviceTimeGenerator = value; }

        #endregion
        public override void Draw(Graphics g)
        {
            g.FillPolygon(new SolidBrush(BackColor), polygonPoints);
            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)Bound.X , (int)polygonPoints[2].Y));

            if (ServiceTimeGeneratorType == ContinuousRandomGeneratorType.None)
                g.FillEllipse(Brushes.White, Bound);
            else
                g.FillEllipse(Brushes.Brown, Bound);
        }

        public override void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"TheNodeHashCode:{TheNode.GetHashCode()}");
            sw.WriteLine($"ServiceTimeGeneratorType: {(int)serviceTimeGeneratorType}");
            if (serviceTimeGeneratorType != ContinuousRandomGeneratorType.None)
            {
                serviceTimeGenerator.SaveToFile(sw);
            }
        }

        public override void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);
           
            string str;
            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            theNodeHashCode = int.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            ServiceTimeGeneratorType = (ContinuousRandomGeneratorType)Enum.Parse(typeof(ContinuousRandomGeneratorType), str, true);
            if (serviceTimeGeneratorType != ContinuousRandomGeneratorType.None)
            {
                serviceTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(ServiceTimeGeneratorType);
                serviceTimeGenerator.ReadFromFile(sr);
            }
        }
    }
}
