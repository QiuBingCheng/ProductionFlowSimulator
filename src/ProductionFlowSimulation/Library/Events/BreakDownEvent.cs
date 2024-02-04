namespace DiscreteEventSimulationLibrary
{
    class BreakDownEvent:DiscreteEvent
    {
        Machine theMachine;
        public BreakDownEvent(Machine machine ,double time)
        {
            theMachine = machine;
            EventTime = time;
            EventType = DiscreteEventType.ServerBreakDown;
        }
        public override void ProcessEvent()
        {
            //the machine demonstration
            theMachine.TurnToBreakDown(EventTime);
        }
        public override string ToString()
        {
            return $"BreakDown Event at {EventTime}";
        }
    }
}
