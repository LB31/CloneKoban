using UnityEngine;

namespace Obstacle
{
    public class Player : IObstacle
    {
        public MoveDirection NextMove;

        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Player;
        }

    }
}
