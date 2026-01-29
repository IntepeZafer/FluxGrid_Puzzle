using UnityEngine;
using System.Collections.Generic;
using PolarityGrid.Core;
using UnityEngine.SceneManagement;

namespace PolarityGrid.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private List<LevelData> levels; // Seviye listesi
        private int _currentLevelIndex = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Sahneler arası silinmesin
            }
            else Destroy(gameObject);

            // Oyuncunun kaldığı seviyeyi hatırla
            _currentLevelIndex = PlayerPrefs.GetInt("SavedLevel", 0);
        }

        public LevelData GetCurrentLevelData()
        {
            // Liste dışına çıkmamak için kontrol
            if (_currentLevelIndex >= levels.Count) _currentLevelIndex = 0;
            return levels[_currentLevelIndex];
        }

        public void NextLevel()
        {
            _currentLevelIndex++;
            PlayerPrefs.SetInt("SavedLevel", _currentLevelIndex);
            
            // Sahneyi yeniden yükle (GridManager start'ta yeni leveli çekecek)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}