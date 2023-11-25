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
        
        private void Awake()
        {
            Instance ??= this;
        }

        private void Start()
        {
            tileLookup = new Dictionary<IObstacle.Type, TileBase>();
            var bounds = tilemap.cellBounds;
            var maxX = bounds.min.x < 0 ? bounds.max.x + Math.Abs(bounds.min.x) : bounds.max.x - bounds.min.x;
            var maxY = bounds.min.y < 0 ? bounds.max.y + Math.Abs(bounds.min.y) : bounds.max.y - bounds.min.y;
            map = new IObstacle[maxY, maxX];
            var tileMapCoord = bounds.min;
            for(var y = 0; y < maxY; y++)
            {
                for (var x = 0; x < maxX; x++)
                {
                    var tile = tilemap.GetTile(tileMapCoord);
                    if (tile == null)
                    {
                        tileMapCoord.x += 1;
                        continue;
                    }
                    switch (tile.name)
                    {
                        case "wall":
                            tileLookup.TryAdd(IObstacle.Type.Wall, tile);
                            map[y, x] = new Wall();
                            break;
                        case "box":
                            tileLookup.TryAdd(IObstacle.Type.Box, tile);
                            map[y, x] = new Box();
                            break;
                        case "player":
                            tileLookup.TryAdd(IObstacle.Type.Player, tile);
                            Player newPlayer = new Player();
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
            foreach (var playerCoordinate  in playerCoordinates)
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
    }
}
