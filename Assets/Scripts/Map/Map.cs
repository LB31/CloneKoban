using System;
using System.Collections.Generic;
using System.Linq;
using Obstacle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Map
{
    public class Map : MonoBehaviour
    {
        public List<Tilemap> tilemaps = new();
        [FormerlySerializedAs("CurrentTilemap")] public int currentTilemap = 0;

        public static Map Instance { get; private set; }
        private IObstacle[,] map;
        private Dictionary<string, TileBase> tileLookup;
        private Dictionary<Player, Vector3Int> playerCoordinates;
        private Dictionary<Goal, Vector3Int> goalCoordinates;
        private readonly List<List<Vector3Int>> boxCoordinateHistory = new ();
        private int mapWidth;
        private int mapHeight;
        
        private void Awake()
        {
            Instance ??= this;
        }

        public void Reset()
        {
            PlayerMover.Instance.UndoAllMoves();
        }
        public void PrepareMap(int index)
        {
            currentTilemap = index;
            
            PlayerMover.Instance.AllPlayers = new List<Player>();
            playerCoordinates = new Dictionary<Player, Vector3Int>();
            goalCoordinates = new Dictionary<Goal, Vector3Int>();
            tileLookup = new Dictionary<string, TileBase>();
            var bounds = tilemaps[currentTilemap].cellBounds;
            mapWidth = bounds.min.x < 0 ? bounds.max.x + Math.Abs(bounds.min.x) : bounds.max.x - bounds.min.x;
            mapHeight= bounds.min.y < 0 ? bounds.max.y + Math.Abs(bounds.min.y) : bounds.max.y - bounds.min.y;
            map = new IObstacle[mapHeight, mapWidth];
            Vector3Int tileMapCoord = bounds.min;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++) 
                {
                    var tile = tilemaps[currentTilemap].GetTile(tileMapCoord);
                    if (tile == null)
                    {
                        tileMapCoord.x += 1;
                        continue;
                    }

                    var args = tile.name.Split('_');
                    switch (args[0])
                    {
                        case "wall":
                            tileLookup.TryAdd("wall", tile);
                            map[y, x] = new Wall();
                            break;
                        case "box":
                            tileLookup.TryAdd("box", tile);
                            map[y, x] = new Box();
                            break;
                        case "goal":
                            tileLookup.TryAdd("goal", tile);
                            Goal goal = new Goal();
                            map[y, x] = goal;
                            goalCoordinates.Add(goal, new Vector3Int(x, y));
                            break;
                        case "player":
                            Player newPlayer = new Player(args.Length == 3 ? int.Parse(args[2]) + 1 : 1);
                            map[y, x] = newPlayer;
                            playerCoordinates.Add(newPlayer, new Vector3Int(x, y));
                            PlayerMover.Instance.AllPlayers.Add(newPlayer);
                            tileLookup.TryAdd(newPlayer.Get_Name(), tile);
                            break;
                    }
                    tileMapCoord.x += 1;
                }
                tileMapCoord.x = bounds.min.x;
                tileMapCoord.y += 1;
            }
            boxCoordinateHistory.Clear();
            UpdateBoxHistory();
        }

        public void MoveAllPlayers()
        {
            foreach (var pair in playerCoordinates.
                         Where(x => x.Key.NextMove != MoveDirection.None).
                         OrderBy(x => x.Key.delay))
            {
                Debug.Log($"Moving Player {pair.Key.Get_Name()} in Direction {pair.Key.NextMove}");
                MovePlayer(pair.Key, pair.Value);
            }
        }

        private void MovePlayer(Player player, Vector3Int coordinate)
        {
            Vector3Int goalCoord = CalculateGoalCoordinate(player.NextMove, coordinate);
            var goalTile = map[goalCoord.y, goalCoord.x];
            if (coordinate == goalCoord)
                return;
            if (goalTile == null || goalTile.GetType() == IObstacle.Type.Goal)
            {
                MoveObstacle(player, coordinate, goalCoord);
                Debug.Log($"Successfully moved from {coordinate} to {goalCoord}.");
            } 
            else if (goalTile.GetType() != IObstacle.Type.Wall)
            {
                Vector3Int goalCoord2 = CalculateGoalCoordinate(player.NextMove, goalCoord);
                var goalTile2 = map[goalCoord2.y, goalCoord2.x];
                if (goalTile2 != null && goalTile2.GetType() != IObstacle.Type.Goal)
                    return;//MoveRec(new List<IObstacle>(){player}, goalTile, 1);
                MoveObstacle(goalTile, goalCoord, goalCoord2);
                MoveObstacle(player, coordinate, goalCoord);
                Debug.Log($"Successfully moved box from {goalCoord} to {goalCoord2}.");
                Debug.Log($"Successfully moved from {coordinate} to {goalCoord}.");
            }
            else
            {
                return;
            }
            UpdateBoxHistory();
            if (ManagerUI.Instance.IsWon)
                return;
            if (CheckIfAllGoalsReached())
            {
              ManagerUI.Instance.OnWin();
            }
            else
            {
                RestoreGoals();
            }
        }
        
        public void MoveAllPlayersReverse()
        {
            foreach (var pair in playerCoordinates.
                         Where(x => x.Key.NextMove != MoveDirection.None).
                         OrderByDescending(x => x.Key.delay))
            {
                Debug.Log($"Reverse Moving Player {pair.Key.Get_Name()} in Direction {pair.Key.ReverseMove}");
                MovePlayerReverse(pair.Key, pair.Value);
            }
        }
        
        private void MovePlayerReverse(Player player, Vector3Int coordinate)
        {
            Vector3Int goalCoord = CalculateGoalCoordinate(player.ReverseMove, coordinate);
            var goalTile = map[goalCoord.y, goalCoord.x];
           
            
            if (goalTile == null || goalTile.GetType() == IObstacle.Type.Goal)
            {
                Vector3Int prevCoord = CalculateGoalCoordinate(player.NextMove, coordinate);
                var prevTile = map[prevCoord.y, prevCoord.x];
                if (prevTile?.GetType() == IObstacle.Type.Wall)
                    return;
                MoveObstacle(player, coordinate, goalCoord);
                Debug.Log($"Successfully moved from {coordinate} to {goalCoord}.");
            }
            else
            {
                return;
            }
            RestoreGoals();
            UndoLastBoxPlacements();
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
                case MoveDirection.Up:
                    return new Vector3Int(currentCoord.x, currentCoord.y + 1);
                case MoveDirection.Right:
                    return new Vector3Int(currentCoord.x + 1, currentCoord.y);
                case MoveDirection.Down:
                    return new Vector3Int(currentCoord.x, currentCoord.y - 1);
                case MoveDirection.Left:
                    return new Vector3Int(currentCoord.x - 1, currentCoord.y);
                case MoveDirection.None:
                default:
                    return currentCoord; // Return the original coordinate if no movement is needed
            }
        }


        public void MoveObstacle(IObstacle obstacle, Vector3Int from, Vector3Int to)
        {
            var orig = tilemaps[currentTilemap].cellBounds.min;
            map[from.y, from.x] = null;
            map[to.y, to.x] = obstacle;
            tilemaps[currentTilemap].SetTile(orig + from, null); 
            tilemaps[currentTilemap].SetTile(orig + to, tileLookup[obstacle.Get_Name()]);
            if (obstacle is Player player)
            {
                playerCoordinates[player] = to;
            }
        }

        private void UpdateBoxHistory()
        {
            boxCoordinateHistory.Add(new List<Vector3Int>());
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (map[y, x]?.GetType() != IObstacle.Type.Box)
                        continue;
                    boxCoordinateHistory[^1].Add(new Vector3Int(x, y));
                }
            }
        }

        private void UndoLastBoxPlacements()
        {
            var orig = tilemaps[currentTilemap].cellBounds.min;
            foreach (var coordinate in boxCoordinateHistory[^1])
            {
                if (map[coordinate.y, coordinate.x]?.GetType() == IObstacle.Type.Box)
                {
                    tilemaps[currentTilemap].SetTile(orig + coordinate, null);
                    map[coordinate.y, coordinate.x] = null;
                }
                else
                    Debug.LogError($"Faulty Box history detected! There is no box at {coordinate}!");
            }
            foreach (var coordinate in boxCoordinateHistory[^2])
            {
                if (map[coordinate.y, coordinate.x] == null)
                {
                    tilemaps[currentTilemap].SetTile(orig + coordinate, tileLookup["box"]);
                    map[coordinate.y, coordinate.x] = new Box();
                }
                else
                    Debug.LogError($"Can't undo box placement! Tile at {coordinate} is not empty.");
            }
            boxCoordinateHistory.RemoveAt(boxCoordinateHistory.Count - 1);
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
            var orig = tilemaps[currentTilemap].cellBounds.min;
            foreach (var goal in goalCoordinates)
            {
                var temp = orig + goal.Value;
                if (tilemaps[currentTilemap].GetTile(temp) == null)
                {
                    tilemaps[currentTilemap].SetTile(temp, tileLookup["goal"]);
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
