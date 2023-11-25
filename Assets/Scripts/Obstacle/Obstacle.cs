using Unity.VisualScripting;

namespace Obstacle
{
    public interface IObstacle
    {
        enum Type
        {
            Wall,
            Box,
            Player
        }

        public abstract Type GetType();
    }
}
