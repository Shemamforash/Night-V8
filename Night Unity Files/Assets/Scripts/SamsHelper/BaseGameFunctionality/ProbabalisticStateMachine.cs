using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality
{
    public class ProbabalisticStateMachine : StateMachine
    {
        //Fallback states are not permitted using strict sampling
        private bool _useStrictSampling;

        //Fallback states are not permitted, but all must be visited, therefore unreachable, unvisited states are permitted.
        private bool _useAllStates;

        public void UseStrictSampling()
        {
            _useStrictSampling = true;
        }

        public void VisitAllStates()
        {
            _useAllStates = true;
        }

        public bool StrictSampling()
        {
            return _useStrictSampling;
        }

        public bool AllStates()
        {
            return _useAllStates;
        }

        protected void LoadProbabilities(string fileName)
        {
            List<string> lines = Helper.ReadLinesFromFile(fileName);
            string[] headings = lines[0].Split(',');
            for (int i = 1; i < lines.Count; ++i)
            {
                string[] probabilities = lines[i].Split(',');
                ProbabalisticState state = (ProbabalisticState) _states[probabilities[0]];
                for (int j = 1; j < probabilities.Length; ++j)
                {
                    state.AddProbability(headings[j], float.Parse(probabilities[j]));
                }
            }
        }
    }
}