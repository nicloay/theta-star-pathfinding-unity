using UnityEngine;

namespace Controllers
{
    
    /// <summary>
    /// When map is ready, these states used as State machine:
    /// values: Idle, FirstPointSet, BothPointsSet
    /// some states encapsulate Vector2Int positions
    /// </summary>
    public interface IInputState
    {
    }


    public class InputIdle : IInputState
    {
    }

    public class InputFirstPointSet : IInputState
    {
        public readonly Vector2Int FirstPoint;

        public InputFirstPointSet(Vector2Int firstPoint)
        {
            FirstPoint = firstPoint;
        }
    }

    public class InputBothPointsSet : InputFirstPointSet
    {
        public readonly Vector2Int SecondPoint;

        public InputBothPointsSet(InputFirstPointSet previousState, Vector2Int secondPoint) : base(previousState
            .FirstPoint)
        {
            SecondPoint = secondPoint;
        }
    }
}