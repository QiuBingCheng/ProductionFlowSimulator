using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class ClientArrivalEvent:DiscreteEvent
    {
        //static DiscreteEventType EventType = DiscreteEventType.ClientArrival;
        Client theClient;
        ClientGenerator theGenerator;
        public ClientArrivalEvent(ClientGenerator  generator, Client client,double time)
        {
            theGenerator = generator;
            this.theClient = client;    
            EventTime = time;
            EventType = DiscreteEventType.ClientArrival;
        }

        public Client TheClient { get => theClient; set => theClient = value; }

        public override void ProcessEvent()
        {
            theClient.EnterServiceNode(EventTime);
            theGenerator.UpdateNextClientArrivalEvent(EventTime);
        }

        public override string ToString()
        {
            return $"Client Arrival Event at {theClient.birthTime}";
        }
    }
}
