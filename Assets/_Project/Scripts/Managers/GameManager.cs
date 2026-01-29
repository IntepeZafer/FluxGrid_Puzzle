using UnityEngine;
using System;
using PolarityGrid.Core;

namespace PolarityGrid.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }
        public static event Action OnLevelWon;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            ChangeState(GameState.Playing);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"<color=green>Oyun Durumu: {newState}</color>");

            if (newState == GameState.LevelWon)
            {
                OnLevelWon?.Invoke();
            }
        }

        public void CheckWinCondition(bool isAllPaired)
        {
            if (CurrentState == GameState.Playing && isAllPaired)
            {
                ChangeState(GameState.LevelWon);
            }
        }
    }
}