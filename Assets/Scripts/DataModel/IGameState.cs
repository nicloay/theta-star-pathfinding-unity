using MapGenerator.MapData;
using Utils;

namespace DataModel
{
    /// <summary>
    /// Main Game states used by the scene
    ///   GameStateNan - default state
    ///   GameStateMapGeneration - when game generate new map, also contains the progress property
    ///   GameStateMapReady - when map generation finish this state is active, contains raw map data
    /// </summary>
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