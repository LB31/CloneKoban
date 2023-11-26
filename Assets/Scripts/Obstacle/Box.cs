using UnityEngine;

namespace Obstacle
{
    public class Box : IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Box;
        }

        public string Get_Name()
        {
            return "box";
        }
    }
}
