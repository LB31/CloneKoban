using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    public CharacterController CharacterController;
    public float MoveDuration = 1;
    public Vector2 PathSoFar;

    private bool moving;

    private void OnMove(InputValue inputValue)
    {
        if (moving) return;

        Vector2 move = inputValue.Get<Vector2>();
        Vector3 movement = Vector3.zero;

        if (move.x != 0)
            movement.x = move.x;
        else if (move.y != 0)
            movement.z = move.y;

        PathSoFar += new Vector2(move.x, move.y);

        StartCoroutine(MovePlayer(movement));
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
