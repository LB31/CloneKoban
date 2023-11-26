using Obstacle;
using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMover : Singleton<PlayerMover>
{
    [FormerlySerializedAs("MapReference")] public Map.Maps mapsReference;
    public List<Player> AllPlayers;

    public float MoveDuration = 1;
    [FormerlySerializedAs("PathsSoFar")] public List<MoveDirection> moveHistory = new();

    private void Start()
    {
        mapsReference = Map.Maps.Instance;
    }

    public void Clear()
    {
        AllPlayers?.Clear();
        moveHistory?.Clear();
    }
    
    private void OnMove(InputValue inputValue)
    {
        if (ManagerUI.Instance.IsWon || ManagerUI.Instance.isPaused)
            return;
        var move = inputValue.Get<Vector2>();
        var movement = Vector3.zero;

        if (move.x != 0)
            movement.x = move.x;
        else if (move.y != 0)
            movement.y = move.y;

        var nextMove = MoveDirection.None;
        switch (movement.y)
        {
            case < 0:
                nextMove = MoveDirection.Down;
                break;
            case > 0:
                nextMove = MoveDirection.Up;
                break;
        }

        switch (movement.x)
        {
            case > 0:
                nextMove = MoveDirection.Right;
                break;
            case < 0:
                nextMove = MoveDirection.Left;
                break;
        }

        if (nextMove == MoveDirection.None) return;
        Debug.Log(movement + " " + nextMove);
        
        moveHistory.Add(nextMove);

        foreach (var player in AllPlayers)
        {
            player.CalcNextMove();
        }
        mapsReference.MoveAllPlayers();
        ManagerUI.Instance.UpdateMoveText();
    }

    public void Undo()
    {
        if (ManagerUI.Instance.IsWon)
            return;
        if (moveHistory.Count == 0)
            return;
        moveHistory.RemoveAt(moveHistory.Count - 1);
        Map.Maps.Instance.UndoLastMovements();
        ManagerUI.Instance.UpdateMoveText();
    }
    
    public void UndoAllMoves()
    {
        moveHistory.Clear();
        Maps.Instance.ResetCurrentMap();
        ManagerUI.Instance.UpdateMoveText();
    }
}

public enum MoveDirection
{
    None = -1,
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}