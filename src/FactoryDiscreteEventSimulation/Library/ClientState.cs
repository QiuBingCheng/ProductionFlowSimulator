using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
