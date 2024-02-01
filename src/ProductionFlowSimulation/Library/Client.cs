using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public class Client:DESElement
    {
        static int instanceCount;
        public double birthTime;
        Series gantt;
        ClientState currentState = ClientState.None;
        private int currentItineraryIndex = -1;
        Itinerary itinerary;
        private double queueTime = -1;
        private Random rnd = new Random();
        private Server serverInService = null;
        public Client(double birthTime, Itinerary it)
        {
            Name = $"Client{instanceCount}";
            this.birthTime = birthTime;
            this.itinerary = it;
            gantt = new Series();
            gantt.ChartType = SeriesChartType.RangeBar;
            BackColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            Bound = new Rectangle(0, 0, 15, 15);
        }

        public double QueueTime { get => queueTime; set => queueTime = value; }
        public Itinerary TheItinerary { get => itinerary; set => itinerary = value; }
        public Visit CurrentVisit { get => itinerary.Visits[currentItineraryIndex]; }
        public ClientState CurrentState { get => currentState; set => currentState = value; }
        public int CurrentItineraryIndex { get => currentItineraryIndex; set => currentItineraryIndex = value; }

        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public Server ServerInService { get => serverInService; set => serverInService = value; }

        public virtual bool EnterServiceNode(double eventTime)
        {
            //update gantt

            currentItineraryIndex++;
            bool isOk = itinerary.Visits[currentItineraryIndex].TheNode.ReceiveAClient(eventTime,this);
          
            if (!isOk && currentItineraryIndex == 0)
                itinerary.TheClientGenerator.DropCount++;

            Console.WriteLine("currentItineraryIndex: ", currentItineraryIndex);
            Console.WriteLine("does enter service node? : ", isOk);

            return isOk;
        }

        internal bool HasNextItinerary()
        {
            return currentItineraryIndex < itinerary.Visits.Count - 1;
        }
        public void TurnToBlockedDwell(double time)
        {
            //
            currentState = ClientState.BlockedDwell;
        }
        public void TurnToBreakdownDwell(double time)
        {
            //
            currentState = ClientState.BreakdownDwell;
        }

    }
}
