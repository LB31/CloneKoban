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
                            Player newPlayer = new Player(args.Length == 2 ? int.Parse(args[1]) : 1);
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
            foreach (var playerCoordinate in playerCoordinates)
            {
                MovePlayer(playerCoordinate.Key, playerCoordinate.Value);
            }
        }

        private void MovePlayer(Player player, Vector3Int coordinate)
        {
            Vector3Int goalCoord = CalculateGoalCoordinate(player.NextMove, coordinate);

            if (goalCoord != coordinate && (map[goalCoord.y, goalCoord.x] == null || map[goalCoord.y, goalCoord.x].GetType() == IObstacle.Type.Goal))
            {
                MoveObstacle(player, coordinate, goalCoord);
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
            tilemaps[CurrentTilemap].SetTile(from, null);
            tilemaps[CurrentTilemap].SetTile(to, tileLookup[obstacle.GetType()]);
            if (obstacle is Player player)
            {
                playerCoordinates[player] = to;
            }
        }

        private void CheckIfBoxInFront(Vector3Int to)
        {
            if(map[to.y, to.x].Equals(IObstacle.Type.Box) || map[to.y, to.x].Equals(IObstacle.Type.Player))
            {
                //MoveObstacle(map[to.y, to.x], );
            }
        }

        public bool CheckIfAllGoalsReached()
        {
            bool win = false;

            foreach (Vector3Int item in goalCoordinates.Values)
            {
                win = map[item.x, item.y].Equals(IObstacle.Type.Box);
            }

            return win;
        }
    }
}
