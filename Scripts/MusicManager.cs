using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Canciones por nivel (3 por cada uno)")]
    public List<AudioClip> level0Songs;
    public List<AudioClip> level1Songs;
    public List<AudioClip> level2Songs;
    public List<AudioClip> level3Songs;

    [Header("Sonidos al completar nivel")]
    public AudioClip level0Complete;
    public AudioClip level1Complete;
    public AudioClip level2Complete;
    public AudioClip level3Complete;

    [Header("Ajustes generales")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public float crossfadeTime = 1f;

    [Header("Debug")]
    public bool verboseLogs = true;

    AudioSource musicSource;
    AudioSource sfxSource;
    int songIndex = 0;

    List<AudioClip> activePlaylist;
    List<AudioClip> lastPlaylist;
    Coroutine playlistRoutine;
    int lastSceneIndex = -1;

    void Awake()
    {
        if (Instance != null)
        {
            if (verboseLogs) Debug.Log("[MusicManager] Duplicado detectado → destroy.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.name = "MusicManager(Singleton)";
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = volume;
        musicSource.ignoreListenerPause = true;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = volume;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (verboseLogs) Debug.Log($"[MusicManager] Start → escena {lastSceneIndex}");
        ApplyPlaylistForScene(lastSceneIndex);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == lastSceneIndex)
        {
            if (verboseLogs) Debug.Log($"[MusicManager] OnSceneLoaded ignorado (misma escena {scene.buildIndex})");
            return;
        }
        lastSceneIndex = scene.buildIndex;
        if (verboseLogs) Debug.Log($"[MusicManager] OnSceneLoaded → escena {lastSceneIndex}");
        ApplyPlaylistForScene(scene.buildIndex);
    }

    void ApplyPlaylistForScene(int index)
    {
        var resolved = ResolvePlaylist(index);
        if (resolved == null || resolved.Count == 0)
        {
            if (verboseLogs) Debug.Log("[MusicManager] Sin playlist para esta escena → stop.");
            StopPlaylistRoutine();
            musicSource.Stop();
            lastPlaylist = null;
            activePlaylist = null;
            return;
        }

        if (PlaylistsEqualByContent(resolved, lastPlaylist))
        {
            if (verboseLogs) Debug.Log("[MusicManager] Playlist igual por contenido → no reiniciar.");
            musicSource.volume = volume;
            return;
        }

        StopPlaylistRoutine();

        activePlaylist = new List<AudioClip>(resolved);
        lastPlaylist = new List<AudioClip>(resolved);
        songIndex = 0;

        if (verboseLogs)
        {
            Debug.Log($"[MusicManager] Nueva playlist aplicada (escena {index}). " +
                      $"Clips: {activePlaylist.Count} | Vol={volume}");
        }

        playlistRoutine = StartCoroutine(PlayPlaylist());
    }

    List<AudioClip> ResolvePlaylist(int index)
    {
        if (index == 0) return level0Songs;
        if (index == 1) return level1Songs;
        if (index == 2) return level2Songs;
        if (index == 3) return level3Songs;
        return null;
    }

    bool PlaylistsEqualByContent(List<AudioClip> a, List<AudioClip> b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    void StopPlaylistRoutine()
    {
        if (playlistRoutine != null)
        {
            if (verboseLogs) Debug.Log("[MusicManager] StopCoroutine(playlistRoutine)");
            StopCoroutine(playlistRoutine);
            playlistRoutine = null;
        }
    }

    IEnumerator PlayPlaylist()
    {
        while (activePlaylist != null && activePlaylist.Count > 0)
        {
            var clip = activePlaylist[songIndex];
            if (clip != null)
            {
                musicSource.clip = clip;
                musicSource.volume = volume;
                musicSource.Play();

                if (verboseLogs)
                    Debug.Log($"[MusicManager] ▶️ Play: {clip.name} (idx {songIndex})");

                while (musicSource != null && musicSource.isPlaying)
                {
                    yield return null;
                }

                if (verboseLogs)
                    Debug.Log($"[MusicManager] ⏹️ Fin clip: {(clip != null ? clip.name : "null")}");
            }

            songIndex = (songIndex + 1) % activePlaylist.Count;
            yield return null;
        }

        if (verboseLogs) Debug.Log("[MusicManager] Playlist terminó / quedó vacía.");
        playlistRoutine = null;
    }

    public float PlayLevelCompleteSound()
    {
        AudioClip clip = null;
        int index = SceneManager.GetActiveScene().buildIndex;

        if (index == 0) clip = level0Complete;
        else if (index == 1) clip = level1Complete;
        else if (index == 2) clip = level2Complete;
        else if (index == 3) clip = level3Complete;

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
            if (verboseLogs) Debug.Log($"[MusicManager] 🔔 LevelComplete SFX: {clip.name}");
            return clip.length;
        }
        if (verboseLogs) Debug.Log("[MusicManager] (Sin SFX de LevelComplete asignado)");
        return 0f;
    }
}
