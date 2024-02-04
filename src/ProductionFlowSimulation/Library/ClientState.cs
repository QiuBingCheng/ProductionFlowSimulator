namespace DiscreteEventSimulationLibrary
{
    public enum ClientState
    {
        None,
        WaitForService,
        BeingServer,
        BlockedDwell, //machine is blocked or breakdown
        BreakdownDwell
    }
}
