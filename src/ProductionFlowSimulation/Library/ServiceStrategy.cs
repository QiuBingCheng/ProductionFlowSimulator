using System;
using System.Collections;
using System.Collections.Generic;

namespace DiscreteEventSimulationLibrary
{
    class ServiceStrategy
    {
        private static Random random = new Random();
        private static ArrayList tmp = new ArrayList();

        public static Server RandomlySelect(List<Server> listOfServers)
        {
            for (int i = 0; i < listOfServers.Count; i++)
            {
                if (listOfServers[i].CurrentState == ServerState.Free)
                    tmp.Add(i);
            }
            if (tmp.Count == 0) return null;
            
            int index = Convert.ToInt32(tmp[random.Next(tmp.Count)]);
            tmp.Clear();
            return listOfServers[index];
        }

        public static Server SelectLowestBusyTime(List<Server> listOfServers)
        {
            double lowestBusyTime = double.NaN;
            int target = -1;

            for (int i = 1; i < listOfServers.Count; i++)
            {
                if (listOfServers[i].CurrentState == ServerState.Free && listOfServers[i].BusyTime < lowestBusyTime)
                {
                    lowestBusyTime = listOfServers[i].BusyTime;
                    target = i;
                }
            }
            return target==-1?null:listOfServers[target];
        }
    }

}
