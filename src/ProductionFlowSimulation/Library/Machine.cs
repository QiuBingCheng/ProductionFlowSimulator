using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;


namespace DiscreteEventSimulationLibrary
{
    public class Machine:Server
    {
        bool enabledBreakDownAndRepairEvents = false;

        BreakDownEvent breakDownEvent;
        ContinuousRandomGeneratorType breakDownTimeGeneratorType = ContinuousRandomGeneratorType.None;
        RandomVariateGenerator breakDownTimeGenerator;

        RepairEvent repairEvent;
        ContinuousRandomGeneratorType repairTimeGeneratorType = ContinuousRandomGeneratorType.None;
        RandomVariateGenerator repairTimeGenerator;

        public static DiscreteEventSimulationModel theSimulationModel;
        private static int instanceCount;

        #region column
        [CategoryAttribute("Model"), DescriptionAttribute(""), DisplayName("1.EnabledBreakDownAndRepairEvents")]
        public bool EnabledBreakDownAndRepairEvents
        {
            get => enabledBreakDownAndRepairEvents;
            set
            {
                if (enabledBreakDownAndRepairEvents == value) return;

                enabledBreakDownAndRepairEvents = value;

                if (enabledBreakDownAndRepairEvents)
                {
                    if (breakDownEvent == null)
                        breakDownEvent = new BreakDownEvent(this, double.NaN);

                    BreakDownTimeGeneratorType = ContinuousRandomGeneratorType.Uniform;
                    RepairTimeGeneratorType = ContinuousRandomGeneratorType.Uniform;
                }
                else
                {
                    breakDownTimeGeneratorType = ContinuousRandomGeneratorType.None;
                    repairTimeGeneratorType = ContinuousRandomGeneratorType.None;
                    breakDownTimeGenerator = null;
                    repairTimeGenerator = null;
                }
            }
        }

        [CategoryAttribute("Model"), DescriptionAttribute(""), DisplayName("2.BreakDownTimeGeneratorType")]
        public ContinuousRandomGeneratorType BreakDownTimeGeneratorType
        {
            get => breakDownTimeGeneratorType;
            set
            {
                if (!enabledBreakDownAndRepairEvents) return;

                if (breakDownTimeGeneratorType != value && value != ContinuousRandomGeneratorType.None)
                {
                    breakDownTimeGeneratorType = value;
                    breakDownTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(breakDownTimeGeneratorType);
                }

            }
        }
        [TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("3.BreakDownTimeGenerator")]
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public RandomVariateGenerator BreakDownTimeGenerator { get => breakDownTimeGenerator; set => breakDownTimeGenerator = value; }


        [CategoryAttribute("Model"), DescriptionAttribute(""), DisplayName("4.RepairTimeGeneratorType")]
        public ContinuousRandomGeneratorType RepairTimeGeneratorType
        {
            get => repairTimeGeneratorType;
            set
            {
                if (!enabledBreakDownAndRepairEvents) return;
                if (repairTimeGeneratorType != value && value!=ContinuousRandomGeneratorType.None)
                {
                    repairTimeGeneratorType = value;
                    repairTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(repairTimeGeneratorType);
                }
            }
        }

        [TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("5.RepairTimeGenerator")]
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public RandomVariateGenerator RepairTimeGenerator { get => repairTimeGenerator; set => repairTimeGenerator = value; }

        internal BreakDownEvent BreakDownEvent { get => breakDownEvent; set => breakDownEvent = value; }
        internal RepairEvent RepaiEvent { get => repairEvent; set => repairEvent = value; }
      
        #endregion

        internal void ReturnFromRepair(double eventTime)
        {
            UpdateSeries(ServerState.Breakdown, lastEventTime, eventTime);
            UpdateStatistics(ServerState.Breakdown, lastEventTime, eventTime);

            //schedule next break event
            if (clientUnderService == null)
                currentState = ServerState.Free;

            else if (clientUnderService.CurrentState == ClientState.BreakdownDwell)
                currentState = ServerState.Busy;

            else if(clientUnderService.CurrentState == ClientState.BlockedDwell)
            {
                UpdateSeries(ServerState.Blocked, lastEventTime, eventTime);
                currentState = ServerState.Blocked;
            }
            
            double breakTime = breakDownTimeGenerator.GetRandomVariate();
            breakDownEvent.EventTime = eventTime + breakTime;
            if (breakDownEvent.EventTime < theSimulationModel.MaximalCeaseTime)
                breakDownEvent.AddToSimulationEngine();

        }

        internal void TurnToBreakDown(double eventTime)
        {
            //free -> breakdown,busy->breakdown,blocked->breakdown
            UpdateSeries(currentState, lastEventTime, eventTime);
            UpdateStatistics(currentState, lastEventTime, eventTime);

            lastEventTime = eventTime;

            //repair event
            double repairTime = repairTimeGenerator.GetRandomVariate();
            repairEvent.EventTime = eventTime + repairTime;
            repairEvent.AddToSimulationEngine();

            //update statistics
            if (currentState == ServerState.Busy)
            {
                clientUnderService.CurrentState = ClientState.BreakdownDwell;
                serviceCompleteEvent.RemoveFromSimulationEngine();
                double remainTime = serviceCompleteEvent.EventTime - eventTime;
                serviceCompleteEvent.EventTime = repairEvent.EventTime+ remainTime;
                serviceCompleteEvent.AddToSimulationEngine();
            }
            currentState = ServerState.Breakdown;
        }
        public override void Draw(Graphics g)
        {
            Brush brush = new SolidBrush(BackColor);
            if (currentState == ServerState.Free)
                brush = new SolidBrush(Color.Lime);
            else if (currentState == ServerState.Busy)
                brush = new SolidBrush(Color.Red);
            else if (currentState == ServerState.Breakdown)
                brush = new SolidBrush(Color.Purple);
            else if (currentState == ServerState.Blocked)
                brush = new SolidBrush(Color.Yellow);

            PointF[] points = GraphicUtil.DrawRegularPoly(GetCenterPoint(), Bound.Width / 2, 6);

            g.FillPolygon(new SolidBrush(Color.White), points);
            g.FillPolygon(brush, points);
            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)(Bound.X + 0.5 * (Bound.Width - size.Width)), Bound.Bottom));
            DrawClientInService(g);
        }
     
        public Machine()
        {
            Name = $"Machine{++instanceCount}";
            //breakDownTimeGeneratorType = ContinuousRandomGeneratorType.None;
            //repairTimeGeneratorType = ContinuousRandomGeneratorType.None;
            queues = new List<TimeQueue>();
            ganttStates = new Series(Name);
            pieStates = new Series(Name);
            BackColor = Color.Green;
        }
        public override void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);
            string str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            enabledBreakDownAndRepairEvents = bool.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            breakDownTimeGeneratorType = (ContinuousRandomGeneratorType)(int.Parse(str));
            if (breakDownTimeGeneratorType != ContinuousRandomGeneratorType.None)
            {
                breakDownTimeGenerator = RandomVariateGenerator.CreateASpecificDistributionGenerator(breakDownTimeGeneratorType);
                breakDownTimeGenerator.ReadFromFile(sr);
            }

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(':') + 1).Trim();
            repairTimeGeneratorType = (ContinuousRandomGeneratorType)(int.Parse(str));
            if (repairTimeGeneratorType != ContinuousRandomGeneratorType.None)
            {
                repairTimeGenerator= RandomVariateGenerator.CreateASpecificDistributionGenerator(repairTimeGeneratorType);
                repairTimeGenerator.ReadFromFile(sr);
            }

        }
        public override void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"EnableBreakAndRepair: {enabledBreakDownAndRepairEvents}");
            sw.WriteLine($"BreakDownTimeGeneratorType: {(int)breakDownTimeGeneratorType}");
            if (breakDownTimeGeneratorType != ContinuousRandomGeneratorType.None)
                breakDownTimeGenerator.SaveToFile(sw);

            sw.WriteLine($"RepairTimeGeneratorType: {(int)repairTimeGeneratorType}");
            if (repairTimeGeneratorType != ContinuousRandomGeneratorType.None)
                repairTimeGenerator.SaveToFile(sw);

        }
        public override void Reset()
        {
            base.Reset();

            if (enabledBreakDownAndRepairEvents)
            {
                breakDownEvent = new BreakDownEvent(this, breakDownTimeGenerator.GetRandomVariate());
                breakDownEvent.AddToSimulationEngine();

                repairEvent = new RepairEvent(this, double.NaN);
            }

        } 
    }
}
