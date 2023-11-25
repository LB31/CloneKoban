using UnityEngine;

namespace Obstacle
{
    public class Wall : MonoBehaviour, IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Wall;
        }
    }
}
