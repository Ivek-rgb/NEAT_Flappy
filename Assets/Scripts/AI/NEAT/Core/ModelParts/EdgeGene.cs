using Utils;

namespace AI.NEAT.Core.ModelParts
{
    public class EdgeGene
    {

        public int FromNode { get; set; }
        public int ToNode { get; set; }
        public float Weight { get; set; }
        public bool Enabled { get; set; }
        public int InnovationNumber { get; set; }

        public EdgeGene(int fromNode, int toNode, float weight, bool enabled, int innovationNumber)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
            this.Weight = weight;
            this.Enabled = enabled;
            this.InnovationNumber = innovationNumber;
        }

        public EdgeGene(int fromNode, int toNode, bool enabled, int innovationNumber)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
            this.Weight = RandomUtils.RandomFloatRange(-1f, 1f);
            this.Enabled = enabled;
            this.InnovationNumber = innovationNumber;
        }

        public EdgeGene Clone() => new(FromNode, ToNode, Weight, Enabled, InnovationNumber);

    }
}
