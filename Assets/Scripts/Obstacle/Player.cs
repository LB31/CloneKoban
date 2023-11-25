using UnityEngine;

namespace Obstacle
{
    public class Player : MonoBehaviour, IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Player;
        }
    }
}
