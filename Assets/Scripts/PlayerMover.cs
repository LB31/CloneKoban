using Obstacle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMover : Singleton<PlayerMover>
{
    public Map.Map MapReference;
    public List<Player> AllPlayers;

    public float MoveDuration = 1;
    [FormerlySerializedAs("PathsSoFar")] public List<MoveDirection> moveHistory = new();
    public List<MoveDirection> reverseMoveHistory = new ();

    private bool moving;

    private void Start()
    {
        MapReference = Map.Map.Instance;
    }

    public void Clear()
    {
        AllPlayers?.Clear();
        moveHistory?.Clear();
    }
    
    private void OnMove(InputValue inputValue)
    {
        if (ManagerUI.Instance.IsWon)
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
                reverseMoveHistory.Add(MoveDirection.Up);
                break;
            case > 0:
                nextMove = MoveDirection.Up;
                reverseMoveHistory.Add(MoveDirection.Down);
                break;
        }

        switch (movement.x)
        {
            case > 0:
                nextMove = MoveDirection.Right;
                reverseMoveHistory.Add(MoveDirection.Left);
                break;
            case < 0:
                nextMove = MoveDirection.Left;
                reverseMoveHistory.Add(MoveDirection.Right);
                break;
        }

        if (nextMove == MoveDirection.None) return;
        Debug.Log(movement + " " + nextMove);
        
        moveHistory.Add(nextMove);

        foreach (var player in AllPlayers)
        {
            player.CalcNextMove();
        }
        MapReference.MoveAllPlayers();
    }

    public void OnUndo()
    {
        Debug.Log("Undo triggered!");
        if (ManagerUI.Instance.IsWon)
            return;
        UndoLastMove();
    }
    
    public bool UndoLastMove()
    {
        if (moveHistory.Count == 0)
            return false;
        foreach (var player in AllPlayers)
        {
            player.CalcNextMove();
        }
        MapReference.MoveAllPlayersReverse();
        reverseMoveHistory.RemoveAt(reverseMoveHistory.Count - 1);
        moveHistory.RemoveAt(moveHistory.Count - 1);
        return true;
    }

    public void UndoAllMoves()
    {
        while (UndoLastMove()) {}
    }
    
    IEnumerator MovePlayer(Vector3 movement)
    {
        moving = true;
        float elapsed = 0;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + movement;

        while (elapsed < MoveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / MoveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;

        moving = false;
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