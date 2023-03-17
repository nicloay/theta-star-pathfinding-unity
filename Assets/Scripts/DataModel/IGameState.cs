using MapGenerator.MapData;
using Utils;

namespace DataModel
{
    public interface IGameState
    {
    }

    public class GameStateNan : IGameState
    {
    }

    public class GameStateMapGeneration : IGameState
    {
        public readonly LevelGenerationProgress Progress = new();
    }

    public class GameStateMapReady : IGameState
    {
        public readonly RawMapData RawMapData;

        public GameStateMapReady(RawMapData rawMapData)
        {
            RawMapData = rawMapData;
        }
    }
}