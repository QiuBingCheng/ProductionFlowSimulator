using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class ServiceCompleteEvent:DiscreteEvent
    {
        Server theServer;
        public ServiceCompleteEvent(Server server,double time)
        {
            theServer = server;
            EventTime = time;
        }

        public override void ProcessEvent()
        {
            theServer.CompleteCurrentService(EventTime);
        }
        public override string ToString()
        {
            return $"ServiceComplete Event at {EventTime}";
        }
    }
}
