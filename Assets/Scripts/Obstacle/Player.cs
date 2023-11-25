using UnityEngine;

namespace Obstacle
{
    public class Player : IObstacle
    {
        public int delay = 1;
        public MoveDirection NextMove;

        public Player(int delay)
        {
            this.delay = delay;
        }
        
        public void CalcNextMove()
        {
            var path = PlayerMover.Instance.PathsSoFar;
            if (path.Count <= delay)
                return;
            NextMove = path[^delay];
        }
        
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Player;
        }

    }
}
