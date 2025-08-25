using System;
using System.IO;
using AI.NEAT.Core.ModelParts;
using AI.NEAT.Core.PopulationManagement;
using AI.NEAT.Core.Serialization.Data;
using AI.NEAT.Core.Serialization.Mappers;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.NEAT.Core.Serialization
{
    public static class NeatSerializer
    {

       
        // simple private logger for dual message generation
        private static class ConsoleLogger
        {
            public static void Error(string message)
            {
#if UNITY_EDITOR
                Debug.LogError(message);
#else
                Console.Error.Write(message);
#endif
            }

            public static void Info(string message)
            {
#if UNITY_EDITOR
                Debug.Log(message);
#else
                Console.Write(message);
#endif
            }

        }

        public static void SavePopulation(PopulationManager population, string filePath)
        {
            try
            {
                var serializablePop = PopulationMapper.ToSerializable(population);
                var json = JsonUtility.ToJson(serializablePop, true);
                
                EnsureDirectory(filePath);
                File.WriteAllText(filePath, json);
                ConsoleLogger.Info($"[NEATSerializer] Population saved to: {filePath}");
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error($"[NEATSerializer] Failed to save population: {ex}");
            }
        }
        
        
        public static PopulationManager LoadPopulation(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    
                    ConsoleLogger.Error($"[NEATSerializer] File not found: {filePath}");
                    return null;
                }

                var json = File.ReadAllText(filePath);
                var serializablePop = JsonUtility.FromJson<SerializablePopulation>(json);

                var manager = PopulationMapper.FromSerializable(serializablePop);
                
                ConsoleLogger.Info($"[NEATSerializer] Population loaded from: {filePath}");
                
                return manager;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error($"[NEATSerializer] Failed to load population: {ex}");
                return null;
            }
        }
        
        
        public static void SaveGenome(Genome genome, string filePath)
        {
            try
            {
                var serializableGenome = GenomeMapper.ToSerializable(genome);
                var json = JsonUtility.ToJson(serializableGenome, true);

                EnsureDirectory(filePath);
                File.WriteAllText(filePath, json);

                ConsoleLogger.Info($"[NEATSerializer] Genome saved to: {filePath}");
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error($"[NEATSerializer] Failed to save genome: {ex}");
            }
        }
        
        public static Genome LoadGenome(string filePath, InnovationTracker tracker = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    ConsoleLogger.Error($"[NEATSerializer] File not found: {filePath}");
                    return null;
                }

                var json = File.ReadAllText(filePath);
                var serializableGenome = JsonUtility.FromJson<SerializableGenome>(json);

                var genome = GenomeMapper.FromSerializable(serializableGenome);
                genome.Tracker = tracker; 
                ConsoleLogger.Info($"[NEATSerializer] Genome loaded from: {filePath}");
                return genome;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error($"[NEATSerializer] Failed to load genome: {ex}");
                return null;
            }
        }
        
        private static void EnsureDirectory(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        
    }
}