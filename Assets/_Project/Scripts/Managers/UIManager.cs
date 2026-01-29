using UnityEngine;
using TMPro; // TextMeshPro kullandığın için gerekli
using UnityEngine.UI; // Butonlar için
using PolarityGrid.Core;
using UnityEngine.SceneManagement;

namespace PolarityGrid.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TextMeshProUGUI winMoveCountText;

        private void Awake()
        {
            // Singleton: Her yerden kolay erişim için
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            // GameManager'daki "LevelWon" olayına abone oluyoruz
            GameManager.OnLevelWon += HandleLevelWon;
        }

        private void OnDisable()
        {
            GameManager.OnLevelWon -= HandleLevelWon;
        }

        private void Start()
        {
            // Oyun başında paneli kapat
            winPanel.SetActive(false);
            UpdateMoveCount(0);
        }

        // Hamle sayısını güncelleyen metod
        public void UpdateMoveCount(int moves)
        {
            moveCountText.text = "Hamle: " + moves;
        }

        // Bölüm kazanıldığında çalışacak metod
        private void HandleLevelWon()
        {
            winPanel.SetActive(true);
            // Kazanma panelindeki text'e final hamle sayısını yazdırabiliriz
        }

        // --- BUTON FONKSİYONLARI ---

        public void OnRestartButtonClicked()
        {
            // Mevcut sahneyi yeniden yükle
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnNextLevelButtonClicked()
        {
            // LevelManager'a "sonraki bölüme geç" diyeceğiz (Birazdan kuracağız)
            Debug.Log("Sonraki Bölüm Yükleniyor...");
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.NextLevel();
            }
        }
    }
}