using System.Collections.Generic;
using UnityEngine;

namespace Obstacle
{
    public class Player : IObstacle
    {
        public int delay = 1;
        public MoveDirection NextMove = MoveDirection.None;
        public MoveDirection ReverseMove = MoveDirection.None;
        private string name = "player";

        public Player(int delay)
        {
            this.delay = delay;
            if (delay == 0)
                return;
            name += "_delay_" + delay;
        }
        
        public void CalcNextMove()
        {
            if (PlayerMover.Instance.moveHistory.Count < delay)
                return;
            NextMove = PlayerMover.Instance.moveHistory[^delay];
            ReverseMove = PlayerMover.Instance.reverseMoveHistory[^delay];
        }
        
        public new IObstacle.Type GetType()
        {
            return IObstacle.Type.Player;
        }

        public string Get_Name()
        {
            return name;
        }
    }
}
