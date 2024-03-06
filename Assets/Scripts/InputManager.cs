using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public enum Direction { Left, Up, Right, Down, None }

    private Direction direction;
    private Vector2 startPos, endPos;
    public float swipeThreshold = 100f;
    private bool draggingStarted;

    public event Action<Direction> OnSwipe;

    public void Awake()
    {
        draggingStarted = false;
        direction = Direction.None;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingStarted = true;
        startPos = eventData.pressPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingStarted)
        {
            endPos = eventData.position;

            Vector2 difference = endPos - startPos;

            if (difference.magnitude > swipeThreshold)
            {
                if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
                {
                    direction = difference.x > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    direction = difference.y > 0 ? Direction.Up : Direction.Down;
                }
            }
            else
            {
                direction = Direction.None;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingStarted && direction != Direction.None)
        {
            OnSwipe?.Invoke(direction);
        }
        startPos = Vector2.zero;
        endPos = Vector2.zero;
        draggingStarted = false;
    }
}
