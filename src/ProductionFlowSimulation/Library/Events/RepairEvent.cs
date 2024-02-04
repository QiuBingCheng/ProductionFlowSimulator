using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    class RepairEvent:DiscreteEvent
    {
        Machine theMachine;
        public RepairEvent(Machine machine, double time)
        {
            theMachine = machine;
            EventTime = time;
            EventType = DiscreteEventType.ServerRepair;
        }
        public override void ProcessEvent()
        {
            //the machine demonstration
            theMachine.ReturnFromRepair(EventTime);

        }
        public override string ToString()
        {
            return $"Repair Event at {EventTime}";
        }
    }
}
