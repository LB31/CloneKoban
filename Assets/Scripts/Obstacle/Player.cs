using UnityEngine;

namespace Obstacle
{
    public class Player : IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Player;
        }
    }
}
