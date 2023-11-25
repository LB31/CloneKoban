using UnityEngine;

namespace Obstacle
{
    public class Box : IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Box;
        }
    }
}
