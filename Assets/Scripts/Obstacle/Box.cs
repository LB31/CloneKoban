using UnityEngine;

namespace Obstacle
{
    public class Box : MonoBehaviour, IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Box;
        }
    }
}
