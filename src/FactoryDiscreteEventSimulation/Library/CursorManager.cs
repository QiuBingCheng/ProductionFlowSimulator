using System.Collections.Generic;

namespace DiscreteEventSimulationLibrary
{
    public enum CursorType
    {
        Module,
        Server,
        Machine,
        Queue,
        Itinerary,
        Distribution,
        Select,
        Release,
        Link
    }

    internal class CursorManager
    {
        internal static Dictionary<CursorType, string> cursorPaths = new Dictionary<CursorType, string>
        {
            { CursorType.Module,"cursorModule.cur" },
            { CursorType.Server,"cursorServer.cur"},
            { CursorType.Machine,"cursorMachine.cur"},
            { CursorType.Queue , "cursorQueue.cur"},
            { CursorType.Itinerary,"cursorItinerary.cur"},
            {CursorType.Select,"cursorSelect.cur" },
            {CursorType.Release,"cursorReleaser.cur" },
            {CursorType.Link,"cursorLink.cur" },
            {CursorType.Distribution,"cursorDistribution.cur" }
         };

        internal static CursorType CurrentCursorType { get; set; }
    }
}
