using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _introMusic;
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            Debug.LogError("MusicManager: Asigna un AudioSource.");
    }

    public void PlayIntroMusic()
    {
        if (_audioSource == null || _introMusic == null) return;
        if (_audioSource.clip == _introMusic && _audioSource.isPlaying) return;
        _audioSource.clip = _introMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void PlayMenuMusic()
    {
        if (_audioSource == null || _menuMusic == null) return;
        if (_audioSource.clip == _menuMusic && _audioSource.isPlaying) return;
        _audioSource.clip = _menuMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void PlayGameMusic()
    {
        if (_audioSource == null || _gameMusic == null) return;
        if (_audioSource.clip == _gameMusic && _audioSource.isPlaying) return;
        _audioSource.clip = _gameMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

        public void StopMusic()
    {
        if (_audioSource != null)
            _audioSource.Stop();
    }
}
