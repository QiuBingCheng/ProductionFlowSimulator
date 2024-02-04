using System;
using System.Collections.Generic;
using System.IO;
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
        static string BaseDir = Path.Combine("..", "..", "Resources", "cursors");

        internal static Dictionary<CursorType, string> cursorPaths = new Dictionary<CursorType, string>
        {
            { CursorType.Module,Path.Combine(BaseDir,"cursorModule.cur")},
            { CursorType.Server,Path.Combine(BaseDir,"cursorServer.cur")},
            { CursorType.Machine,Path.Combine(BaseDir,"cursorMachine.cur")},
            { CursorType.Queue , Path.Combine(BaseDir, "cursorQueue.cur")},
            { CursorType.Itinerary,Path.Combine(BaseDir, "cursorItinerary.cur")},
            {CursorType.Select,Path.Combine(BaseDir, "cursorSelect.cur") },
            {CursorType.Release,Path.Combine(BaseDir, "cursorReleaser.cur")},
            {CursorType.Link,Path.Combine(BaseDir, "cursorLink.cur" )},
            {CursorType.Distribution,Path.Combine(BaseDir, "cursorDistribution.cur")}
         };

        internal static CursorType CurrentCursorType { get; set; }

        // Initialize custom cursor manager
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        internal static Cursor SetCursor(CursorType cursorType)
        {
            IntPtr ptr = LoadCursorFromFile(cursorPaths[cursorType]);
            CurrentCursorType = cursorType;
            return new Cursor(ptr);
        }

    }
}
