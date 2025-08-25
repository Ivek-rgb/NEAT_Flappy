using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Core.Serialization.Data;

namespace AI.NEAT.Core.Serialization.Mappers
{
    public class InnovationTrackerMapper
    {
        public static SerializableInnovationTracker ToSerializable(InnovationTracker tracker)
        {
            var dict = tracker.GetInnovationsCopy();
            return new SerializableInnovationTracker
            {
                keys = dict.Keys.ToList(),
                values = dict.Values.ToList(),
                currentInnovation = tracker.CurrentInnovation
            };
        }
        
        public static InnovationTracker FromSerializable(SerializableInnovationTracker dto)
        {
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < dto.keys.Count; i++) 
                  dict[dto.keys[i]] = dto.values[i];
            return new InnovationTracker(dto.currentInnovation, dict);
        }
        
    }
}