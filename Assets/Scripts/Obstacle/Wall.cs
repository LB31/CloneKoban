using UnityEngine;

namespace Obstacle
{
    public class Wall : IObstacle
    {
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Wall;
        }

        public string Get_Name()
        {
            return "wall";
        }
    }
}
