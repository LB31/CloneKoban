using System;
using System.Collections.Generic;
using Obstacle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public class Map : MonoBehaviour
    {
        public List<Tilemap> tilemaps = new();
        public int CurrentTilemap = 0;

        public static Map Instance { get; private set; }
        public IObstacle[,] map;
        private Dictionary<IObstacle.Type, TileBase> tileLookup;
        private Dictionary<Player, Vector3Int> playerCoordinates;
        private Dictionary<Goal, Vector3Int> goalCoordinates;

        private void Awake()
        {
            Instance ??= this;

            //PrepareMap(0);
        }

        public void PrepareMap(int index)
        {
            CurrentTilemap = index;

            PlayerMover.Instance.AllPlayers = new List<Player>();
            playerCoordinates = new();
            goalCoordinates = new ();
            tileLookup = new ();
            var bounds = tilemaps[CurrentTilemap].cellBounds;
            int maxX = bounds.min.x < 0 ? bounds.max.x + Math.Abs(bounds.min.x) : bounds.max.x - bounds.min.x;
            int maxY = bounds.min.y < 0 ? bounds.max.y + Math.Abs(bounds.min.y) : bounds.max.y - bounds.min.y;
            map = new IObstacle[maxY, maxX];
            Vector3Int tileMapCoord = bounds.min;
            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++) 
                {
                    var tile = tilemaps[CurrentTilemap].GetTile(tileMapCoord);
                    if (tile == null)
                    {
                        tileMapCoord.x += 1;
                        continue;
                    }

                    var args = tile.name.Split('_');
                    switch (args[0])
                    {
                        case "wall":
                            tileLookup.TryAdd(IObstacle.Type.Wall, tile);
                            map[y, x] = new Wall();
                            break;
                        case "box":
                            tileLookup.TryAdd(IObstacle.Type.Box, tile);
                            map[y, x] = new Box();
                            break;
                        case "goal":
                            tileLookup.TryAdd(IObstacle.Type.Goal, tile);
                            Goal goal = new Goal();
                            map[y, x] = goal;
                            goalCoordinates.Add(goal, new Vector3Int(x, y));
                            break;
                        case "player":
                            tileLookup.TryAdd(IObstacle.Type.Player, tile);
                            Player newPlayer = new Player(args.Length == 3 ? int.Parse(args[2]) + 1 : 1);
                            map[y, x] = newPlayer;
                            playerCoordinates.Add(newPlayer, new Vector3Int(x, y));
                            PlayerMover.Instance.AllPlayers.Add(newPlayer);
                            break;
                    }
                    tileMapCoord.x += 1;
                }
                tileMapCoord.x = bounds.min.x;
                tileMapCoord.y += 1;
            }
        }

        public void MoveAllPlayers()
        {
            
            var temp = new Dictionary<Player, Vector3Int>();
            
            foreach (var playerCoordinate in playerCoordinates)
            {
               temp.Add(playerCoordinate.Key, playerCoordinate.Value);
            }

            foreach (var t in temp)
            {
                MovePlayer(t.Key, t.Value);
            }
        }

        private void MovePlayer(Player player, Vector3Int coordinate)
        {
            Vector3Int goalCoord = CalculateGoalCoordinate(player.NextMove, coordinate);
            var goalTile = map[goalCoord.y, goalCoord.x];
            if (goalCoord != coordinate && (goalTile == null || goalTile.GetType() == IObstacle.Type.Goal))
            {
                MoveObstacle(player, coordinate, goalCoord);
            } else if (goalTile.GetType() != IObstacle.Type.Wall)
            {
                Vector3Int goalCoord2 = CalculateGoalCoordinate(player.NextMove, goalCoord);
                var goalTile2 = map[goalCoord2.y, goalCoord2.x];
                if (goalTile2 != null && goalTile2.GetType() != IObstacle.Type.Goal)
                    MoveRec(new List<IObstacle>(){player}, goalTile, 1);
                MoveObstacle(goalTile, goalCoord, goalCoord2);
                MoveObstacle(player, coordinate, goalCoord);
            }
        }


        public void MoveRec(List<IObstacle> pushList, IObstacle current, int force)
        {
            if (current.GetType() == IObstacle.Type.Player)
            {
                var player = (Player)current;
                Vector3Int goalCoord = CalculateGoalCoordinate(player.NextMove, playerCoordinates[player]);
                var goalTile = map[goalCoord.y, goalCoord.x];
                pushList.Add(current);
                MoveRec(pushList,  goalTile, force + 1);
                return;
            }
            if (current.GetType() == IObstacle.Type.Box)
            {
                var player = (Player)current;
                Vector3Int goalCoord = CalculateGoalCoordinate(player.NextMove, playerCoordinates[player]);
                var goalTile = map[goalCoord.y, goalCoord.x];
                pushList.Add(current);
                MoveRec(pushList,  goalTile, force + 1);
            }
        }

        private Vector3Int CalculateGoalCoordinate(MoveDirection direction, Vector3Int currentCoord)
        {
            switch (direction)
            {
                case MoveDirection.TOP:
                    return new Vector3Int(currentCoord.x, currentCoord.y - 1);
                case MoveDirection.RIGHT:
                    return new Vector3Int(currentCoord.x + 1, currentCoord.y);
                case MoveDirection.DOWN:
                    return new Vector3Int(currentCoord.x, currentCoord.y + 1);
                case MoveDirection.LEFT:
                    return new Vector3Int(currentCoord.x - 1, currentCoord.y);
                case MoveDirection.NONE:
                default:
                    return currentCoord; // Return the original coordinate if no movement is needed
            }
        }


        public void MoveObstacle(IObstacle obstacle, Vector3Int from, Vector3Int to)
        {
            var orig = tilemaps[CurrentTilemap].cellBounds.min;
            map[from.y, from.x] = null;
            map[to.y, to.x] = obstacle;
            tilemaps[CurrentTilemap].SetTile(orig + from, null); 
            tilemaps[CurrentTilemap].SetTile(orig + to, tileLookup[obstacle.GetType()]);
            if (obstacle is Player player)
            {
                playerCoordinates[player] = to;
            }

            if (CheckIfAllGoalsReached())
            {
                FindObjectOfType<ManagerUI>().OnCancel(null);
            }
            else
            {
                RestoreGoals();
            }
        }
        

        private void CheckIfBoxInFront(Vector3Int to)
        {
            if(map[to.y, to.x].Equals(IObstacle.Type.Box) || map[to.y, to.x].Equals(IObstacle.Type.Player))
            {
                //MoveObstacle(map[to.y, to.x], );
            }
        }

        public void RestoreGoals()
        {
            var orig = tilemaps[CurrentTilemap].cellBounds.min;
            foreach (var goal in goalCoordinates)
            {
                var temp = orig + goal.Value;
                if (tilemaps[CurrentTilemap].GetTile(temp) == null)
                {
                    tilemaps[CurrentTilemap].SetTile(temp, tileLookup[IObstacle.Type.Goal]);
                }
            }
        }

        public bool CheckIfAllGoalsReached()
        {
            foreach (Vector3Int item in goalCoordinates.Values)
            {
                if (map[item.y, item.x] == null)
                    return false;
                if (map[item.y, item.x].GetType() != IObstacle.Type.Box)
                    return false;
            }
            Debug.Log("SWEET VICTORY!");
            return true;
        }
    }
}
