using System.Collections.Generic;
using UnityEngine;

namespace AI.NEAT.Core
{
    public class InnovationTracker
    {
        
        private readonly Dictionary<string, int> _innovations = new();
        private int _currentInnovation = 0;
        public int CurrentInnovation => _currentInnovation;
        
        public InnovationTracker() { }

        public InnovationTracker(int currentInnovation, Dictionary<string, int> innovations)
        {
            _currentInnovation = currentInnovation;
            _innovations = new Dictionary<string, int>(innovations); 
        }

        public Dictionary<string, int> GetInnovationsCopy()
        {
            return new Dictionary<string, int>(_innovations); 
        }

        public int GetInnovationNumber(int from, int to)
        {
            string key = $"{from}:{to}" ;
            if (_innovations.TryGetValue(key, out int innovation))
                return innovation;

            _innovations[key] = _currentInnovation;
            return _currentInnovation++;
        }

        public InnovationTracker Copy() => new(_currentInnovation, _innovations);

    }
}
