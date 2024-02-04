using ProductionFlowSimulation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Text;

namespace DiscreteEventSimulationLibrary
{
    public class DiscreteEventSimulationModel
    {

        #region property
        private List<ServiceNode> serviceNodes = new List<ServiceNode>();
        private List<Itinerary> itineraries = new List<Itinerary> ();

        List<ClientGenerator> clientGenerators = new List<ClientGenerator>();
        private  DiscreteEventSimulationEngine simulationEngine;

        [Browsable(false)]
        public DiscreteEventSimulationEngine SimulationEngine { get => simulationEngine; set => simulationEngine = value; }

        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public string Name { get => name; set => name = value; }
        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public List<DiscreteEvent> FeatureEventList { get => simulationEngine.FeatureEventList; }

        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public double SimulationClock { get => simulationEngine.SimulationClock; }
        [CategoryAttribute("Model"), DescriptionAttribute("")]
        public double ProcessedEventCount { get => simulationEngine.EventCount; }

        [CategoryAttribute("Collection"), DescriptionAttribute("")]
        [Editor(typeof(DESCollectionElementEditor), typeof(UITypeEditor))]
        public List<ClientGenerator> ClientGenerators { get => clientGenerators; set => clientGenerators = value; }
        [CategoryAttribute("Collection"), DescriptionAttribute("")]
        [Editor(typeof(DESCollectionElementEditor), typeof(UITypeEditor))]
        public List<Itinerary> Itineraries { get => itineraries; set => itineraries = value; }
        [CategoryAttribute("Collection"), DescriptionAttribute("")]
        [Editor(typeof(DESCollectionElementEditor), typeof(UITypeEditor))]
        public List<ServiceNode> ServiceNodes { get => serviceNodes; set => serviceNodes = value; }

        private string name;
        #endregion

        public DiscreteEventSimulationModel(List<ServiceNode> nodes, List<Itinerary> itineraries,double [] probabilities)
        {
            name = "Simple Network";
            this.serviceNodes = nodes;
            this.itineraries = itineraries;
            simulationEngine = new DiscreteEventSimulationEngine();
            Machine.theSimulationModel = this;
        }

        public DiscreteEventSimulationModel()
        {
            name = "Simulation Model";
            simulationEngine = new DiscreteEventSimulationEngine();
            Machine.theSimulationModel = this;
        }
       
        public string GetSimulationResult()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#################### Simulation Results ####################");
            sb.AppendLine($"Simulation End: {SimulationClock:0.000}  Processed Event Count: {ProcessedEventCount}");

            sb.AppendLine($"*** #Client Generators:{clientGenerators.Count}");
            foreach (ClientGenerator cg in clientGenerators)
                cg.GetSimulationResult(sb);

            sb.AppendLine($"");
            sb.AppendLine($"*** #Service Nodes:{serviceNodes.Count}");
            foreach (ServiceNode sn in serviceNodes)
            {
                sn.GetSimulationResult(sb);
                sb.AppendLine($"");
            }

            return sb.ToString();
        }
        public void ResetSimulation()
        {
            simulationEngine.Reset();
            //ProcessedEventCount
            foreach (ServiceNode node in serviceNodes) node.Reset();
            foreach (ClientGenerator clientGenerator in clientGenerators)
            {
                clientGenerator.Reset();
                clientGenerator.UpdateNextClientArrivalEvent(SimulationClock);
            }

            //chart
            int count = 0;
            foreach (ServiceNode serviceNode in serviceNodes)
            {
                for (int i = 0; i < serviceNode.Servers.Count; i++)
                {
                    serviceNode.Servers[i].serverOrder = ++count;
                }
            }
        }

        public List<Server> GetAllServers()
        {
            List<Server> servers = new List<Server> { };
            foreach (ServiceNode sn in serviceNodes)
                foreach (Server server in sn.Servers)
                    servers.Add(server);
            return servers;
        }

        [Browsable(false)]
        public double MaximalCeaseTime
        {
            get
            {
                double largest = double.MinValue;
                foreach (ClientGenerator cg in clientGenerators)
                {
                    if (cg.CeasesTime > largest) largest = cg.CeasesTime;

                }
                return largest;
            }
        }
        public int GetServersCount()
        {
            int count = 0; ;
            foreach (ServiceNode sn in serviceNodes)
            {
                count += sn.Servers.Count;
            }
            return count;
        }
        public bool RunOneEvent()
        {
            return simulationEngine.RunNextEvent();
        }
        public void RunAllEvent()
        {
            simulationEngine.RunToEnd();
        }

        public void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"NumberOfServiceNodes:{serviceNodes.Count}");
            for (int i = 0; i < serviceNodes.Count; i++)
            {
                serviceNodes[i].SaveToFile(sw);
            }

            sw.WriteLine($"NumberOfClientGenerator:{clientGenerators.Count}");
            for (int i = 0; i < clientGenerators.Count; i++)
            {
                clientGenerators[i].SaveToFile(sw);
            }
        }

        public void ReadFromFile(StreamReader sr)
        {
           
            string str;
            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();

            serviceNodes.Clear();
            int number = int.Parse(str);
            for (int i = 0; i < number; i++)
            {
                ServiceNode sn = new ServiceNode();
                sn.ReadFromFile(sr);
                serviceNodes.Add(sn);
            }

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            number = int.Parse(str);
            clientGenerators.Clear();
            itineraries.Clear();

            for (int i = 0; i < number; i++)
            {
                ClientGenerator cg = new ClientGenerator();
                cg.ReadFromFile(sr);
                clientGenerators.Add(cg);

                foreach (Itinerary it in cg.Itineraries)
                {
                    itineraries.Add(it);
                    it.ResumeNodeReference(serviceNodes);
                }
            }
        }

        public void ExportDESElements(List<DESElement> allElements)
        {
            foreach (ServiceNode sn in serviceNodes)
            {
                allElements.Add(sn);

                foreach (Server server in sn.Servers)
                    allElements.Add(server);

                foreach (TimeQueue queue in sn.Queues)
                    allElements.Add(queue);
            }

            foreach (ClientGenerator cg in clientGenerators)
                allElements.Add(cg);
            foreach (Itinerary it in itineraries)
            {
                allElements.Add(it);
                foreach (Visit visit in it.Visits)
                {
                    allElements.Add(visit);
                }
            }
               
        }

        public List<TimeQueue> GetAllQueues()
        {
            List<TimeQueue> queues = new List<TimeQueue> { };
            foreach (ServiceNode sn in serviceNodes)
            {
                foreach (TimeQueue queue in sn.Queues)
                {
                    queues.Add(queue);
                }
            }
            return queues;
        }
    }

}
