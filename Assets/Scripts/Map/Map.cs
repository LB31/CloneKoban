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
        public Tilemap tilemap;
        public static Map Instance { get; private set; }
        public IObstacle[,] map;
        private Dictionary<IObstacle.Type, TileBase> tileLookup;
        private Dictionary<Player, Vector3Int> playerCoordinates;
        private Dictionary<Goal, Vector3Int> goalCoordinates;

        private void Awake()
        {
            Instance ??= this;
        }

        private void Start()
        {
            PlayerMover.Instance.AllPlayers = new List<Player>();
            playerCoordinates = new();
            goalCoordinates = new ();
            tileLookup = new ();
            var bounds = tilemap.cellBounds;
            int maxX = bounds.min.x < 0 ? bounds.max.x + Math.Abs(bounds.min.x) : bounds.max.x - bounds.min.x;
            int maxY = bounds.min.y < 0 ? bounds.max.y + Math.Abs(bounds.min.y) : bounds.max.y - bounds.min.y;
            map = new IObstacle[maxY, maxX];
            Vector3Int tileMapCoord = bounds.min;
            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++) 
                {
                    var tile = tilemap.GetTile(tileMapCoord);
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
            switch (player.NextMove)
            {
                case MoveDirection.NONE:
                    break;
                case MoveDirection.TOP:
                    MovePlayerUp(player, coordinate);
                    break;
                case MoveDirection.RIGHT:
                    MovePlayerRight(player, coordinate);
                    break;
                case MoveDirection.DOWN:
                    MovePlayerDown(player, coordinate);
                    break;
                case MoveDirection.LEFT:
                    MovePlayerLeft(player, coordinate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MovePlayerUp(Player player, Vector3Int coordinate)
        {
            var goalCoord = new Vector3Int(coordinate.y - 1, coordinate.x);
            if (map[goalCoord.y, goalCoord.x] == null)
            {
                MoveObstacle(player, coordinate, goalCoord);
            }
        }

        private void MovePlayerDown(Player player, Vector3Int coordinate)
        {
            var goalCoord = new Vector3Int(coordinate.y + 1, coordinate.x);
            if (map[goalCoord.y, goalCoord.x] == null)
            {
                MoveObstacle(player, coordinate, goalCoord);
            }
        }
        private void MovePlayerLeft(Player player, Vector3Int coordinate)
        {
            var goalCoord = new Vector3Int(coordinate.y, coordinate.x - 1);
            if (map[goalCoord.y, goalCoord.x] == null)
            {
                MoveObstacle(player, coordinate, goalCoord);
            }
        }
        private void MovePlayerRight(Player player, Vector3Int coordinate)
        {
            var goalCoord = new Vector3Int(coordinate.y, coordinate.x + 1);
            if (map[goalCoord.y, goalCoord.x] == null)
            {
                MoveObstacle(player, coordinate, goalCoord);
            }
        }

        public void MoveObstacle(IObstacle obstacle, Vector3Int from, Vector3Int to)
        {
            tilemap.SetTile(from, null);
            tilemap.SetTile(to, tileLookup[obstacle.GetType()]);
            if (obstacle is Player player)
            {
                playerCoordinates[player] = to;
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
