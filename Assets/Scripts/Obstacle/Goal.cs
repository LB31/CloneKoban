using UnityEngine;

namespace Obstacle
{
    public class Goal : IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Goal;
        }
    }
}
