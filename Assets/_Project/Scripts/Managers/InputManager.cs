using UnityEngine;
using System;
using PolarityGrid.Core;

namespace PolarityGrid.Managers
{
    public class InputManager : MonoBehaviour
    {
        public static event Action<Direction> OnSwipe;

        private Vector2 _fingerDownPosition;
        private Vector2 _fingerUpPosition;
        [SerializeField] private float minDistanceForSwipe = 50f;

        void Update()
        {
            // Temiz Kod: Sadece oyun oynanırken girişi kabul et
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            if (Input.GetMouseButtonDown(0)) _fingerDownPosition = Input.mousePosition;
            if (Input.GetMouseButtonUp(0))
            {
                _fingerUpPosition = Input.mousePosition;
                DetectSwipe();
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) _fingerDownPosition = touch.position;
                if (touch.phase == TouchPhase.Ended)
                {
                    _fingerUpPosition = touch.position;
                    DetectSwipe();
                }
            }
        }

        private void DetectSwipe()
        {
            float distance = Vector2.Distance(_fingerDownPosition, _fingerUpPosition);
            if (distance > minDistanceForSwipe)
            {
                Vector2 directionVector = _fingerUpPosition - _fingerDownPosition;
                if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
                    SendSwipe(directionVector.x > 0 ? Direction.Right : Direction.Left);
                else
                    SendSwipe(directionVector.y > 0 ? Direction.Up : Direction.Down);
            }
        }

        private void SendSwipe(Direction dir) => OnSwipe?.Invoke(dir);
    }
}