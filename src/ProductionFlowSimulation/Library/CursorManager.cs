using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            { CursorType.Module,Path.Combine("Resources", "cursors","cursorModule.cur")},
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

        // Initialize custom cursor manager
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        internal static Cursor SetCursor(CursorType cursorType)
        {
            IntPtr ptr = LoadCursorFromFile(cursorPaths[cursorType]);
            CurrentCursorType = CursorType.Module;
            return new Cursor(ptr);
        }

    }
}
