using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Canciones por nivel (3 por cada uno)")]
    public List<AudioClip> level1Songs;
    public List<AudioClip> level2Songs;
    public List<AudioClip> level3Songs;

    [Header("Sonidos al completar nivel")]
    public AudioClip level1Complete;
    public AudioClip level2Complete;
    public AudioClip level3Complete;

    [Header("Ajustes generales")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public float crossfadeTime = 1f;

    [Header("Debug")]
    public bool verboseLogs = true;

    AudioSource musicSource;   // Música
    AudioSource sfxSource;     // SFX “nivel completado”
    int songIndex = 0;

    List<AudioClip> activePlaylist; // la que se está usando
    List<AudioClip> lastPlaylist;   // snapshot de la última aplicada
    Coroutine playlistRoutine;
    int lastSceneIndex = -1;

    void Awake()
    {
        // Singleton estricto
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

        // No suscribimos aquí para evitar dobles
        // SceneManager.sceneLoaded += OnSceneLoaded;
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
        var resolved = ResolvePlaylist(index); // obtiene lista candidata (puede ser null)
        if (resolved == null || resolved.Count == 0)
        {
            if (verboseLogs) Debug.Log("[MusicManager] Sin playlist para esta escena → stop.");
            StopPlaylistRoutine();
            musicSource.Stop();
            lastPlaylist = null;
            activePlaylist = null;
            return;
        }

        // Si el contenido es igual a lo que ya estaba, no reiniciar ni tocar songIndex
        if (PlaylistsEqualByContent(resolved, lastPlaylist))
        {
            if (verboseLogs) Debug.Log("[MusicManager] Playlist igual por contenido → no reiniciar.");
            // Asegura que el source siga con volumen correcto
            musicSource.volume = volume;
            return;
        }

        // Cambió la playlist: detén corrutina anterior, aplica nueva y reinicia desde 0
        StopPlaylistRoutine();

        // Clona la lista para que no cambie por referencia luego
        activePlaylist = new List<AudioClip>(resolved);
        lastPlaylist   = new List<AudioClip>(resolved);
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
            // compara por referencia de clip (lo más confiable)
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

                // Espera real a que termine. Si alguien para el audio, salimos del while interno
                while (musicSource != null && musicSource.isPlaying)
                {
                    yield return null;
                }

                if (verboseLogs)
                    Debug.Log($"[MusicManager] ⏹️ Fin clip: {(clip != null ? clip.name : "null")}");
            }

            // Avanza al siguiente
            songIndex = (songIndex + 1) % activePlaylist.Count;
            yield return null;
        }

        if (verboseLogs) Debug.Log("[MusicManager] Playlist terminó / quedó vacía.");
        playlistRoutine = null;
    }

    // SFX nivel completado
    public float PlayLevelCompleteSound()
    {
        AudioClip clip = null;
        int index = SceneManager.GetActiveScene().buildIndex;

        if (index == 1) clip = level1Complete;
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
