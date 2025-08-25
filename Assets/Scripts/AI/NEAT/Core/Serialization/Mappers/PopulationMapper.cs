using System.Linq;
using AI.NEAT.Core.PopulationManagement;
using AI.NEAT.Core.Serialization.Data;

namespace AI.NEAT.Core.Serialization.Mappers
{
    public class PopulationMapper
    {
        
        public static SerializablePopulation ToSerializable(PopulationManager manager)
        {
            return new SerializablePopulation
            {
                generation = manager.Generation,
                populationSize = manager.PopulationSize,
                inputSize = manager.InputSize,
                outputSize = manager.OutputSize,
                deltaThreshold = manager.DeltaThreshold,
                innovationTracker = InnovationTrackerMapper.ToSerializable(manager.InnovationTracker),
                population = GenomeMapper.ToSerializableList(manager.Population)
            };
        }
        
        public static PopulationManager FromSerializable(SerializablePopulation dto)
        {
            
            var tracker = InnovationTrackerMapper.FromSerializable(dto.innovationTracker);
            var genomes = GenomeMapper.FromSerializableList(dto.population);

            var manager = PopulationManager.LoadUpPopulation(
                dto.populationSize,
                dto.inputSize,
                dto.outputSize,
                dto.deltaThreshold,
                genomes,
                tracker
            );

            manager.Generation = dto.generation;
            return manager;
        }
        
        
    }
}