using Unity.VisualScripting;

namespace Obstacle
{
    public interface IObstacle
    {
        enum Type
        {
            Wall,
            Box,
            Player,
            Goal
        }

        public abstract Type GetType();
    }
}
