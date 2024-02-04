using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscreteEventSimulationLibrary
{
    public class DiscreteEvent
    {
       

        static DiscreteEventSimulationEngine theEngine;
        public DiscreteEventType EventType { get; internal set; }
        public double EventTime { get; internal set; }

        public virtual void ProcessEvent()
        {
            throw new Exception("No implememtation");
        }

        internal static void SetEngine(DiscreteEventSimulationEngine engine)
        {
            theEngine = engine;
        }

        internal void AddToSimulationEngine()
        {
            theEngine.InsertAnEvent(this);
        }

        public bool RemoveFromSimulationEngine()
        {
            return theEngine.RemoveAnEvent(this);
        }

    }
}
