using ProductionFlowSimulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace DiscreteEventSimulationLibrary
{
    public class ServiceNode:DESElement
    {
        
        private List<Server> servers;
        private List<TimeQueue> queues;
        private int currentClientCount;
        private Series clientInNodeSeries;
        static private int instanceCount;
        private int visitedClientCount;
        private int maxClientCount;
        private double totalWaitTime;
        private Queue<Server> deferredServerQueue;
        private int dropCount;

        #region Column
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        [Editor(typeof(DESCollectionElementEditor), typeof(UITypeEditor))]
        public List<TimeQueue> Queues { get => queues; set => queues = value; }
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        [Editor(typeof(DESCollectionElementEditor), typeof(UITypeEditor))]
        public List<Server> Servers { get => servers; set => servers = value; }

        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int CurrentClientCount { get => currentClientCount; }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int VisitedClientCount { get => visitedClientCount; }
        [CategoryAttribute("Statistics"), DescriptionAttribute("")]
        public int MaxClientCount { get => maxClientCount; }

        int itineraryCont;
        [Browsable(false)]
        public Series ClientCountSeries { get => clientInNodeSeries; set => clientInNodeSeries = value; }

        [Browsable(false)]
        public int ItineraryCont { get => itineraryCont; set => itineraryCont = value; }
        [Browsable(false)]
        public Queue<Server> DeferredServerQueue { get => deferredServerQueue; set => deferredServerQueue = value; }
        [Browsable(false)]
        public int DropCount { get => dropCount; set => dropCount = value; }
        #endregion


        public override void Draw(Graphics g)
        {
            Brush b = new SolidBrush(BackColor);
            g.FillRectangle(b, Bound);
            SizeF size = g.MeasureString(Name, ElementFont);
            g.DrawString(Name, ElementFont, Brushes.Black, new Point((int)(Bound.X + 0.5 * (Bound.Width - size.Width)),Bound.Bottom));
        }

        public Series GetServerGantt(int serverIndex)
        {
            return servers[serverIndex].GanttStates;
        }
        public Series GetServerGantt(string serverName)
        {
            foreach (Server server in servers)
            {
                if(server.Name == serverName)
                    return server.GanttStates;
            }
            return null;
        }

        public ServiceNode(List<Server> listOfServers, List<TimeQueue> listOfQueues)
        {
            this.servers = listOfServers;
            this.queues = listOfQueues;
            Name = $"ServiceNode{++instanceCount}";
            clientInNodeSeries = new Series();
            clientInNodeSeries.ChartType = SeriesChartType.StepLine;
            clientInNodeSeries.Color = Color.Red;
            clientInNodeSeries.BorderWidth = 3;
            clientInNodeSeries.Name = "Count In Node";
        }
        public ServiceNode()
        {
            servers = new List<Server> { };
            queues = new List<TimeQueue> { };
            deferredServerQueue = new Queue<Server> { };
            Name = $"ServiceNode{++instanceCount}";
            clientInNodeSeries = new Series();
            clientInNodeSeries.ChartType = SeriesChartType.StepLine;
            clientInNodeSeries.Color = Color.Red;
            clientInNodeSeries.BorderWidth = 3;
            clientInNodeSeries.Name = "Count In Node";
            BackColor = Color.LightYellow;
        }

        internal void Reset()
        {
            dropCount = 0;
            clientInNodeSeries.Points.Clear();
            deferredServerQueue.Clear();
            clientInNodeSeries.Points.AddXY(0, 0);

            foreach (Server s in servers) s.Reset();
            foreach (TimeQueue q in queues) q.Reset();

            currentClientCount = 0;
            visitedClientCount = 0;
            maxClientCount = 0;

        }
        public bool ReceiveAClient(double time,Client client)
        {
            //if no server free and queue is full , then reject the client
            //update statistic statics
            currentClientCount++;
            updateClientSeries(time);

            clientInNodeSeries.Points.AddXY(time, currentClientCount);
            if (maxClientCount < currentClientCount)
                maxClientCount = currentClientCount;

            Server freeServer = ServiceStrategy.RandomlySelect(servers);
            if (freeServer != null)
            {
                client.CurrentState = ClientState.BeingServer;
                freeServer.StartServingAClient(time, client);
                Console.WriteLine(freeServer.Name," starts serveing ",client.Name);
                return true;
            }

            //Find shortest queue to let the client queued
            //target queue
            TimeQueue shortageQueue = queues[0];
            for (int i = 1; i < queues.Count; i++)
            {
                if (queues[i].CurrentClientCount < shortageQueue.CurrentClientCount)
                    shortageQueue = queues[i];
            }

            bool isOK = shortageQueue.AddClient(time, client);
           
            if (isOK) {
                client.CurrentState = ClientState.WaitForService;
                updateClientSeries(time);
                Console.WriteLine(client.Name, " is waiting in ", shortageQueue.Name);
                return true;
            }

            if(client.CurrentItineraryIndex > 0)
            {
                deferredServerQueue.Enqueue(client.ServerInService);
                client.TurnToBlockedDwell(time);
                Console.WriteLine(client.Name, " is waiting in ", shortageQueue.Name);
            }
            return false;
        }

        public void ExitAClient(double time, Client client)
        {
            Console.WriteLine("Exit A Client: ", client.Name);

            currentClientCount--;
            visitedClientCount++;
            //update queue length series by service node
            updateClientSeries(time);
            //update statistic
            totalWaitTime += (time - client.birthTime);
        }

        internal void GetSimulationResult(StringBuilder sb)
        {
            sb.AppendLine($"<<<Service Node:{Name}>>>");
            sb.AppendLine($"Visited Client Count:{visitedClientCount}");
            sb.AppendLine($"#Queues:{queues.Count}");
            foreach (TimeQueue queue in queues)
            {
                queue.GetSimulationResult(sb);                                                           
            }

            sb.AppendLine($"#Servers:{servers.Count}");
            foreach (Server server in servers)
            {
                server.GetSimulationResult(sb);
            }     

        }

        public void updateClientSeries(double time)
        {
            //因為同步 所以一起處理
            clientInNodeSeries.Points.AddXY(time, currentClientCount);
            foreach (TimeQueue queue in Queues)
                queue.UpdateQueueLengthSeries(time);
        }

        public override void SaveToFile(StreamWriter sw)
        {
            base.SaveToFile(sw);
            sw.WriteLine($"NumberofServers:{servers.Count}");
            sw.WriteLine($"NumberofQueues:{queues.Count}");

            for (int i = 0; i < servers.Count; i++)
            {
                Type t = servers[i].GetType();
                sw.WriteLine($"Type Information:{t.FullName},{t.Assembly.GetName().Name}");
                servers[i].SaveToFile(sw);
            }
        }

        public override void ReadFromFile(StreamReader sr)
        {
            base.ReadFromFile(sr);

            string str;

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            int numberOfServers = int.Parse(str);

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            int numberOfQueues = int.Parse(str);

            servers.Clear();
            string serverType;
            for (int i = 0; i < numberOfServers; i++)
            {
                str = sr.ReadLine();
                str = str.Substring(str.IndexOf(":") + 1).Trim();
                serverType = str.Split(',')[0];

                Type t = Type.GetType(serverType);
                Server server = (Server)Activator.CreateInstance(t);
                server.ParentNode = this;
                server.ReadFromFile(sr);
                servers.Add(server);
            }
        }

       
    }
}