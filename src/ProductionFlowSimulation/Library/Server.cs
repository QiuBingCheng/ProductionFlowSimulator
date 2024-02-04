using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public class Server:DESElement
    {
        RandomVariateGenerator serviceTimeGenerator;
        protected ContinuousRandomGeneratorType serviceTimeGeneratorType;
        protected Client clientUnderService;
        protected List<TimeQueue> queues;
        protected List<int> queueHashCodes = new List<int> ();
        protected int clientServedCount;
        private static int instanceCount;
        protected Series ganttStates;
        protected Series pieStates;
        protected double busyTime;
        protected double freeTime;
        protected double squareSum = 0;
        protected double lastEventTime;
        protected ServerState currentState;
        protected ServiceCompleteEvent serviceCompleteEvent;
        protected static Dictionary<ServerState, Color> ganttColors;
        protected Rectangle clientBound;
        private double blockedTime;
        public int serverOrder;
        private double breakTime;

        static Server()
        {
            ganttColors = new Dictionary<ServerState, Color>
            {
                { ServerState.Free, Color.Lime },
                { ServerState.Busy, Color.Red },
                { ServerState.Blocked, Color.Yellow },
                { ServerState.Breakdown, Color.Purple }
            };
        }
      
        public Server()
        {
            ServiceTimeGeneratorType = ContinuousRandomGeneratorType.Uniform;
            Name = $"Server{++instanceCount}";
            queues = new List<TimeQueue>();
            ganttStates = new Series(Name);
            pieStates = new Series(Name);
            BackColor = Color.Green;
        }

        #region Property
        [Category("Statistics"), Description("")]
        public int ClientServedCount { get => clientServedCount; }
        [Category("Statistics"), Description("")]
        public double BusyTime { get => busyTime; }
        [Category("Statistics"), Description("")]
        public double FreeTime { get => freeTime; }
        [Category("Statistics"), Description("")]
        public double BlockedTime { get => blockedTime; }
        [Category("Statistics"), Description("")]
        public double ServiceTimeAverage
        {
            get
            {
                if (clientServedCount > 0)
                    return busyTime / clientServedCount;

                return 0;
            }
        }
        [Category("Statistics"), Description("")]
        public double ServiceTimeSTD
        {
            get
            {
                if (clientServedCount > 0)
                    return Math.Sqrt((squareSum* clientServedCount - busyTime*busyTime) / (clientServedCount* (clientServedCount-1)));
                return 0;
            }
        }


        [Category("Display"), Description("")]
        public ServerState CurrentState { get => currentState; }

        [Category("Collections"), Description("")]
        public List<TimeQueue> Queues { get => queues; set => queues = value; }

        [Browsable(false)]
        public double LastEventTime { get => lastEventTime; set => lastEventTime = value; }

        [Category("Model"), Description("")]
        public Client ClientUnderService { get => clientUnderService; set => clientUnderService = value; }

        [Category("Model"), Description(""), DisplayName("6.ServiceTimeGeneratorType")]
        public ContinuousRandomGeneratorType ServiceTimeGeneratorType
        {
            get => serviceTimeGeneratorType;
            set
            {
                if (serviceTimeGeneratorType != value)
                {
                    serviceTimeGeneratorType = value;
                    serviceTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(serviceTimeGeneratorType);
                }
            }
        }
    
        [TypeConverter(typeof(ExpandableObjectConverter)),DisplayName("7.ServiceTimeGenerator")]
        [Category("Model"), Description("")]
        public RandomVariateGenerator ServiceTimeGenerator { get => serviceTimeGenerator; set => serviceTimeGenerator = value; }

        // collection
        [Browsable(false)]
        public ServiceNode ParentNode { get; set; }
        [Browsable(false)]
        public Series GanttStates { get => ganttStates; set => ganttStates = value; }
        [Browsable(false)]
        public Series PieStates { get => pieStates; set => pieStates = value; }
        #endregion

        public virtual void Reset()
        {
            clientUnderService = null;
            busyTime = 0;
            squareSum = 0;
            freeTime = 0;
            clientServedCount = 0;
            ganttStates.Points.Clear();

            //chart 
            ganttStates.ChartType = SeriesChartType.RangeBar;
            pieStates.ChartType = SeriesChartType.Pie;
            ganttStates.LegendText = Name;
            pieStates.LegendText = Name;
            ganttStates.Name = Name;
            pieStates.Name = Name;
            pieStates.Points.Clear();
            pieStates.Points.AddXY("Free", 0.0); //Point[0] -->free
            pieStates.Points[0].Color = Color.FromArgb(160, Color.Lime);
            pieStates.Points.AddXY("Busy", 0.0);//Point[0] --> busy
            pieStates.Points[1].Color = Color.FromArgb(160, Color.Red);
            pieStates.Points.AddXY("Blocked", 0.0);//Point[0] --> busy
            pieStates.Points[2].Color = Color.FromArgb(160, Color.Yellow);
            pieStates.Points.AddXY("BreakDown", 0.0);//Point[0] --> busy
            pieStates.Points[3].Color = Color.FromArgb(160, Color.Purple);

            lastEventTime = 0;
            currentState = ServerState.Free;
            serviceCompleteEvent = new ServiceCompleteEvent(this, double.NaN);

        }
        public override void Draw(Graphics g)
        {
            Brush b = new SolidBrush(BackColor);
            if (currentState == ServerState.Free)
                b = new SolidBrush(Color.Lime);
            else if (currentState == ServerState.Busy)
                b = new SolidBrush(Color.Red);
            else if (currentState == ServerState.Breakdown)
                b = new SolidBrush(Color.Purple);
            else if (currentState == ServerState.Blocked)
                b = new SolidBrush(Color.Yellow);

            g.FillEllipse(new SolidBrush(Color.White), Bound);
            g.FillEllipse(b, Bound);
            g.DrawEllipse(Pens.Black, Bound);
            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)(Bound.X + 0.5 * (Bound.Width - size.Width)), Bound.Bottom));
            DrawClientInService(g);
            
        }
        
        internal override void DrawLineToCollections(Graphics g)
        {
            foreach (TimeQueue queue in Queues)
                g.DrawLine(Pens.Black, GetCenterPoint(),queue.GetCenterPoint());
        }

        protected void DrawClientInService(Graphics g)
        {
            if (clientUnderService != null)
            {
                Point cp = GetCenterPoint();
                clientBound = new Rectangle(cp.X - clientUnderService.Bound.Width / 2, cp.Y - clientUnderService.Bound.Height / 2, clientUnderService.Bound.Width, clientUnderService.Bound.Height);
                GraphicUtil.DrawSpecificType(clientUnderService.TheItinerary.Shape, clientBound, clientUnderService.BackColor, g);
            }
        }
        protected void UpdateSeries(ServerState state, double lastEventTime, double currentTime)
        {
            DataPoint dp = new DataPoint
            {
                XValue = serverOrder,
                YValues = new double[] { lastEventTime, currentTime },
                Color = ganttColors[state]
            };
            ganttStates.Points.Add(dp);

            pieStates.Points[(int)state].YValues[0] += (currentTime - lastEventTime);
        }
        protected void UpdateStatistics(ServerState state,double lastEventTime,double currentTime)
        {
            double duration = currentTime - lastEventTime;
            switch (state)
            {
                
                case ServerState.Free:
                    freeTime += duration;
                    break;
                case ServerState.Busy:
                    busyTime += duration;
                    squareSum += duration * duration;
                    break;
                case ServerState.Breakdown:
                    breakTime += duration;
                    break;
                case ServerState.Blocked:
                    blockedTime += duration;
                    break;
            }
            
        }

        public void StartServingAClient(double time,Client client)
        {
            UpdateSeries(currentState, lastEventTime, time);
            UpdateStatistics(currentState, lastEventTime, time);

            lastEventTime = time;
            currentState = ServerState.Busy;

            //set client
            clientUnderService = client;
            client.ServerInService = this;

            double serviceTime;
            if (clientUnderService.CurrentVisit.ServiceTimeGeneratorType == ContinuousRandomGeneratorType.None)
                serviceTime = serviceTimeGenerator.GetRandomVariate();
            else
                serviceTime = clientUnderService.CurrentVisit.ServiceTimeGenerator.GetRandomVariate();

            serviceCompleteEvent.EventTime = time + serviceTime;
            serviceCompleteEvent.AddToSimulationEngine();
        }

        public void CompleteCurrentService(double time)
        {
            UpdateSeries(currentState, lastEventTime, time);
            UpdateStatistics(currentState, lastEventTime, time);

            clientServedCount++;
            lastEventTime = time;
            currentState = ServerState.Free;

            //cannot enter new service node
            // servers are all busy and no queue has vacancies
            //1. 
            if (clientUnderService.HasNextItinerary())
            {
                bool doesEnterServiceNode = clientUnderService.EnterServiceNode(time);

                Console.WriteLine("HasNextItinerary: Yes");
                if (!doesEnterServiceNode)
                {
                    clientUnderService.TurnToBlockedDwell(time);
                    currentState = ServerState.Blocked;
                    return;
                }
            }
  
            ParentNode.ExitAClient(time, clientUnderService);

            clientUnderService = null;
            TimeQueue target = queues[0];
            Client head = target.EscortAClient(time);
            ParentNode.updateClientSeries(time);
            if (head != null) 
                StartServingAClient(time, head);

            //checke blocked server in last node 
            if (ParentNode.DeferredServerQueue.Count > 0)
                ReEnterTheNode(time);

        }

        public void ReEnterTheNode(double time)
        {
            Server deferredServer = ParentNode.DeferredServerQueue.Dequeue();
            UpdateSeries(ServerState.Blocked, deferredServer.lastEventTime, time);
            ParentNode.ReceiveAClient(time, deferredServer.clientUnderService);

            if (deferredServer.currentState == ServerState.Blocked)
            {
                deferredServer.currentState = ServerState.Free;
                TimeQueue target = deferredServer.queues[0];
                Client head = target.EscortAClient(time);
                deferredServer.ParentNode.updateClientSeries(time);
                if (head != null)
                    deferredServer.StartServingAClient(time, head);

            }
        }

        internal void GetSimulationResult(StringBuilder sb)
        {
            sb.AppendLine($"[{Name}] Client Served:{clientServedCount} Service Time Average:{ServiceTimeAverage:0.00} STD:{ServiceTimeSTD:0.00}");
            sb.AppendLine($"Busy Ratio:{busyTime / lastEventTime:0.00} Free Ratio:{freeTime / lastEventTime:0.00} Blocked Ratio:{BlockedTime / lastEventTime:0.00}");
        }

        public override void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"ServiceTimeGeneratorType: {(int)serviceTimeGeneratorType}");
            if (serviceTimeGeneratorType != ContinuousRandomGeneratorType.None)
                serviceTimeGenerator.SaveToFile(sw);

            sw.WriteLine($"NumberofServerQueues: {queues.Count}");
            for (int i = 0; i < queues.Count; i++)
            {
                queues[i].SaveToFile(sw);
            }
        }

        public override void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);

            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            serviceTimeGeneratorType = (ContinuousRandomGeneratorType)(int.Parse(str));
            if (serviceTimeGeneratorType != ContinuousRandomGeneratorType.None)
            {
                serviceTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(serviceTimeGeneratorType);
                serviceTimeGenerator.ReadFromFile(sr);
            }
          
            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            int number = int.Parse(str);
            for (int i = 0; i < number; i++)
            {
                TimeQueue queue = new TimeQueue();
                queue.ReadFromFile(sr);
                bool flag = false;
                for (int j = 0; j < ParentNode.Queues.Count; j++)
                {
                    if (ParentNode.Queues[j].Name == queue.Name)
                    {
                        queues.Add(ParentNode.Queues[j]);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    queues.Add(queue);
                    ParentNode.Queues.Add(queue);
                }
            }
        }
    }
}
