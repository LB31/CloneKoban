using System;
using System.Collections.Generic;
using System.Linq;
using Obstacle;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Map
{
    public class Maps : MonoBehaviour
    {
        [HideInInspector] public List<Tilemap> tilemaps = new();
        [FormerlySerializedAs("CurrentTilemap")] public int currentTilemap = 0;

        public static Maps Instance { get; private set; }
        private IObstacle[,] map;
        private Dictionary<string, TileBase> tileLookup;
        private Dictionary<Player, Vector3Int> playerCoordinates;
        private Dictionary<Goal, Vector3Int> goalCoordinates;
        private readonly List<IObstacle[,]> history = new ();
        private int mapWidth;
        private int mapHeight;

        private void Awake()
        {
            Instance ??= this;
            for (var i = 0; i < transform.childCount; i ++)
            {
                tilemaps.Add(transform.GetChild(i).GetChild(0).GetComponent<Tilemap>());    
            }
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
            UpdateHistory();
        }

        public void MoveAllPlayers()
        {
            foreach (var player in playerCoordinates.
                         Where(x => x.Key.NextMove != MoveDirection.None).
                         OrderBy(x => x.Key.delay).
                         Select(x => x.Key))
            {
                Debug.Log($"Moving Player {player.Get_Name()} in Direction {player.NextMove}");
                MovePlayer(player, playerCoordinates[player]);
            }
            UpdateHistory();
        }

        private bool CanMove(Player player, Vector3Int coordinate, MoveDirection moveDirection)
        {
            Vector3Int goalCoord = CalculateGoalCoordinate(moveDirection, coordinate);
            var goalTile = map[goalCoord.y, goalCoord.x];
            if (goalTile == null || goalTile.GetType() == IObstacle.Type.Goal)
            {
                return true;
            }

            if (goalTile.GetType() == IObstacle.Type.Wall) return false;
            Vector3Int goalCoord2 = CalculateGoalCoordinate(player.NextMove, goalCoord);
            var goalTile2 = map[goalCoord2.y, goalCoord2.x];
            return goalTile2 == null || goalTile2.GetType() == IObstacle.Type.Goal;
        }
        
        private void MovePlayer(Player player, Vector3Int coordinate)
        {
            Vector3Int goalCoord = CalculateGoalCoordinate(player.NextMove, coordinate);
            var goalTile = map[goalCoord.y, goalCoord.x];
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
                {
                    MoveRec(
                        new List<(IObstacle, Vector3Int)>() { (player, coordinate) }, (goalTile, goalCoord), 1, 0, player.NextMove);
                    return;
                }
                MoveObstacle(goalTile, goalCoord, goalCoord2);
                MoveObstacle(player, coordinate, goalCoord);
                Debug.Log($"Successfully moved box from {goalCoord} to {goalCoord2}.");
                Debug.Log($"Successfully moved from {coordinate} to {goalCoord}.");
            }
            else
            {
                return;
            }
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

        private void MoveRec(List<(IObstacle, Vector3Int)> pushList, (IObstacle, Vector3Int) current, int force, int boxCount, MoveDirection direction)
        {
            switch (current.Item1?.GetType())
            {
                case IObstacle.Type.Player:
                    {
                        Vector3Int goalCoord = CalculateGoalCoordinate(direction, current.Item2);
                        var goalTile = map[goalCoord.y, goalCoord.x];
                        if (goalTile is Player player && player.NextMove != direction) 
                            return;
                        pushList.Add(current);
                        MoveRec(pushList, (goalTile, goalCoord), force + 1, boxCount, direction);
                    }
                    break;
                case IObstacle.Type.Box:
                    {
                        Vector3Int goalCoord = CalculateGoalCoordinate(direction, current.Item2);
                        var goalTile = map[goalCoord.y, goalCoord.x];
                        if (goalTile != null && goalTile.GetType() == IObstacle.Type.Player)
                            return;
                        pushList.Add(current);
                        MoveRec(pushList, (goalTile, goalCoord), force, boxCount + 1, direction);
                    }
                    break;
                case IObstacle.Type.Goal:
                case null:
                    pushList.Add(current);
                    HandleMultiPush(pushList, force, boxCount);
                    break;
                case IObstacle.Type.Wall:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleMultiPush(List<(IObstacle, Vector3Int)> pushList, int force, int boxCount)
        {
            if (force < boxCount)
                return;
            for (var i = 1; i < pushList.Count; i++)
            {
                MoveObstacle(pushList[^(i+1)].Item1, pushList[^(i+1)].Item2, pushList[^i].Item2);
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

        private IObstacle[,] GetCopy(IObstacle[,] src)
        {
            var copy = new IObstacle[mapHeight,mapWidth];

            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    switch (src[y, x]?.GetType())
                    {
                        case IObstacle.Type.Wall:
                            copy[y, x] = new Wall();
                            break;
                        case IObstacle.Type.Box:
                            copy[y, x] = new Box();
                            break;
                        case IObstacle.Type.Player:
                            copy[y, x] = new Player(((Player) map[y, x]).delay);
                            break;
                        case IObstacle.Type.Goal:
                            copy[y, x] = new Goal();
                            break;
                        case null:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            return copy;
        }
        
        private void UpdateHistory()
        {
            history.Add(GetCopy(map));
        }


        public void ResetCurrentMap()
        {
            map = history[0];
            playerCoordinates.Clear();
            history.Clear();
            history[0] = map;
            var orig = tilemaps[currentTilemap].cellBounds.min;
            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    if (map[y, x] is Player player)
                    {
                        playerCoordinates[player] = new Vector3Int(x, y);
                    }
                    tilemaps[currentTilemap].SetTile(orig + new Vector3Int(x, y), 
                        map[y, x] == null ? null : tileLookup[map[y, x].Get_Name()]);
                }
            }
            PlayerMover.Instance.AllPlayers.Clear();
            foreach (var player in playerCoordinates.Keys)
            {
                PlayerMover.Instance.AllPlayers.Add(player);
            }
        }
        
        public bool UndoLastMovements()
        {
            if (ManagerUI.Instance.IsWon || history.Count < 2)
                return false;
            map = history[^2];
            playerCoordinates.Clear();
            history.RemoveAt(history.Count - 1);
            var orig = tilemaps[currentTilemap].cellBounds.min;
            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    if (map[y, x] is Player player)
                    {
                        playerCoordinates[player] = new Vector3Int(x, y);
                    }
                    tilemaps[currentTilemap].SetTile(orig + new Vector3Int(x, y), 
                        map[y, x] == null ? null : tileLookup[map[y, x].Get_Name()]);
                }
            }
            PlayerMover.Instance.AllPlayers.Clear();
            foreach (var player in playerCoordinates.Keys)
            {
                PlayerMover.Instance.AllPlayers.Add(player);
            }
            return true;
        }
    }
}
