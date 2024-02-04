using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public class TimeQueue:DESElement
    {
        List<Client> theQueue = new List<Client>();
        int capacity = int.MaxValue;
        static int instanceCount;
        Series seriesClients;
        int maximalCount = 0;
        int servedClientCount = 0;
        private double delayTimeSquare;
        private double totalDelayTime;
        bool isPrioritized = false;
        
        [Browsable(false)]
        public Series SeriesClients { get => seriesClients; set => seriesClients = value; }

        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public bool IsPrioritized { get => isPrioritized; set => isPrioritized = value; }
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public int Capacity { get => capacity; set => capacity = value; }

        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int ServedClientCount { get => servedClientCount;}
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int CurrentClientCount { get => theQueue.Count; }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int MaximalCount { get => maximalCount; }

        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double DelayTimeAverage { 
            get
            {
                if(servedClientCount>0)
                    return totalDelayTime / servedClientCount;
                return 0;
            } 
        }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public double DelayTimeSTD { 
            get
            {
                return Math.Sqrt(delayTimeSquare / servedClientCount - DelayTimeAverage * DelayTimeAverage);
            } 
        }

        public TimeQueue()
        {
            Name = $"Queue{++instanceCount}";
            seriesClients = new Series(Name);
            BackColor = Color.White;
        }
        public override void Draw(Graphics g)
        {
            //queue
            Brush b = new SolidBrush(BackColor);
            g.FillRectangle(b, Bound);
            g.DrawRectangle(Pens.Black, Bound);
            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)(Bound.X + 0.5 * (Bound.Width - size.Width)), Bound.Bottom));

            DrawClientsInQueue(g);
        }

        public void DrawClientsInQueue(Graphics g)
        {
            //client in the queue
            Rectangle rectangle;
            int row = 0;
            int col = 0;
            bool flag = false;

            for (int i = 0; i < theQueue.Count; i++)
            {
                while (true)
                {
                    rectangle = new Rectangle(Bound.X + col * theQueue[i].Bound.Width, Bound.Y + row * theQueue[i].Bound.Height, theQueue[i].Bound.Width, theQueue[i].Bound.Height);

                    if (rectangle.Right > Bound.Right)
                    {
                        col = 0;
                        row++;
                    }
                    else if (rectangle.Bottom > Bound.Bottom)
                    {
                        flag = true;
                        break;
                    }
                    else
                    {
                        GraphicUtil.DrawSpecificType(theQueue[i].TheItinerary.Shape, rectangle, theQueue[i].BackColor, g);
                        col++;
                        break;
                    }
                }

                if (flag)
                    break;
            }
        }
        public void Reset()
        {
            maximalCount = 0;
            servedClientCount = 0;
            delayTimeSquare = 0;
            totalDelayTime = 0;

            theQueue.Clear();
            seriesClients.Points.Clear();
            seriesClients.Points.AddXY(0, 0);
            seriesClients.ChartType = SeriesChartType.StepLine;
            seriesClients.Color = BackColor;
            seriesClients.Name = Name;
            seriesClients.LegendText = Name;
            seriesClients.BorderWidth = 3;
        }
       
        public bool AddClient(double time,Client theClient)
        {
            if (theQueue.Count >= capacity) return false;

            theClient.QueueTime = time;
            int pos = theQueue.Count;

            double thePriorityWeight = theClient.TheItinerary.PriorityWeight;
            if (isPrioritized)
            {
                //determine
                for (int i = 0; i < theQueue.Count; i++)
                {
                    if (thePriorityWeight < theQueue[i].TheItinerary.PriorityWeight)
                    {
                        pos = i;
                        break;
                    }
                }
            }
            theQueue.Insert(pos, theClient);
            servedClientCount++;
            if (theQueue.Count > maximalCount) maximalCount = theQueue.Count;
            UpdateQueueLengthSeries(time);
            return true;
        }

        public virtual Client EscortAClient(double time)
        {
            if (theQueue.Count == 0) return null;

            Client target = theQueue[0];
            theQueue.RemoveAt(0);
            double delayTime = time - target.QueueTime;
            totalDelayTime += delayTime;
            delayTimeSquare += delayTime * delayTime;
            UpdateQueueLengthSeries(time);
            return target;
        }

        public void UpdateQueueLengthSeries(double time)
        {
            seriesClients.Points.AddXY(time, theQueue.Count);
        }

        public override void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            capacity = int.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            IsPrioritized = bool.Parse(str);
        }

        public override void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"Capacity: {capacity}");
            sw.WriteLine($"IsPrioritized: {isPrioritized}");
        }

        internal void GetSimulationResult(StringBuilder sb)
        {
            sb.AppendLine($"[{Name}] Passed Client Count:{ServedClientCount} Maximal Count:{maximalCount} Time-average Length:{DelayTimeAverage:0.00}");
        }
    }
}