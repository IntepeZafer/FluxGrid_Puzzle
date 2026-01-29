using UnityEngine;

namespace PolarityGrid.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        [Header("Audio Clips")]
        [SerializeField] private AudioClip swipeSound;
        [SerializeField] private AudioClip snapSound;
        [SerializeField] private AudioClip winSound;

        private AudioSource _audioSource;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        public void PlaySwipe() => _audioSource.PlayOneShot(swipeSound);
        public void PlaySnap() => _audioSource.PlayOneShot(snapSound);
        public void PlayWin() => _audioSource.PlayOneShot(winSound);
    }
}