using System;
using System.Collections.Generic;
using System.Text;

namespace DiscreteEventSimulationLibrary
{
    public class DiscreteEventSimulationEngine
    {
        public int EventCount { get; set; }
        List<DiscreteEvent> featureEventList = new List<DiscreteEvent>();
        double simulationClock;
        public string FeatureEventListString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (DiscreteEvent de in featureEventList)
                {
                    sb.Append($"{de.EventTime:0.00} {de.GetType().Name} -> ");
                }
                return sb.ToString();
            }
        }
        public DiscreteEventSimulationEngine()
        {
            DiscreteEvent.SetEngine(this);
        }

        public double SimulationClock { get => simulationClock; }
        public List<DiscreteEvent> FeatureEventList { get => featureEventList; set => featureEventList = value; }

        public bool RemoveAnEvent(DiscreteEvent discreteEvent)
        {
            return featureEventList.Remove(discreteEvent);
        }

        public void Reset()
        {
            EventCount = 0;
            featureEventList.Clear();
            simulationClock = 0;
            EventCount = 0;
        }

        public bool RunNextEvent()
        {
            if (featureEventList.Count == 0) return false;
            DiscreteEvent headEvent = featureEventList[0];
            featureEventList.RemoveAt(0);
            simulationClock = headEvent.EventTime;
            EventCount++;
            headEvent.ProcessEvent();
            return true;
        }

        internal void InsertAnEvent(DiscreteEvent anEvent)
        {
            
            for (int i = 0; i < featureEventList.Count; i++)
            {
                if (anEvent.EventTime <= featureEventList[i].EventTime)
                {
                    featureEventList.Insert(i, anEvent);
                    return;
                }
            }
      
            featureEventList.Add(anEvent);
        }

        internal void RunToEnd()
        {
            do
            {
                RunNextEvent();
            } while (featureEventList.Count != 0);
        }
    }
}