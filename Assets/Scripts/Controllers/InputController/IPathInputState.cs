using UnityEngine;

namespace Controllers
{
    public interface IPathInputState
    {
    }


    public class InputIdle : IPathInputState
    {
    }

    public class InputFirstPointSet : IPathInputState
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