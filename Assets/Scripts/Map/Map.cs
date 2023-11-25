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
        
        private void Awake()
        {
            Instance ??= this;
        }

        private void Start()
        {
            tileLookup = new Dictionary<IObstacle.Type, TileBase>();
            var bounds = tilemap.cellBounds;
            map = new IObstacle[bounds.max.y, bounds.max.x];
            for(var x = bounds.min.x; x < bounds.max.x; x++) {
                for(var y= bounds.min.y; y < bounds.max.y; y++)
                {
                    var tile = tilemap.GetTile(new Vector3Int(x, y));
                    switch (tile.name)
                    {
                        case "Wall":
                            tileLookup.TryAdd(IObstacle.Type.Wall, tile);
                            map[y, x] = new Wall();
                            break;
                        case "Box":
                            tileLookup.TryAdd(IObstacle.Type.Box, tile);
                            map[y, x] = new Box();
                            break;
                        case "Player":
                            tileLookup.TryAdd(IObstacle.Type.Player, tile);
                            map[y, x] = new Player();
                            break;
                    }
                }
            }
        }

        public void MoveObstacle(IObstacle obstacle, Vector3Int from, Vector3Int to)
        {
            tilemap.SetTile(from, null);
            tilemap.SetTile(to, tileLookup[obstacle.GetType()]);
        }
    }
}
