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
    [Range(0f,1f)] public float volume = 0.5f;
    public float crossfadeTime = 1f;

    AudioSource musicSource; // para música
    AudioSource sfxSource;   // para SFX de “nivel completado”
    int songIndex = 0;
    List<AudioClip> activePlaylist;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = volume;

        // 👇 este source vive entre escenas, así el SFX no se corta al cargar la siguiente
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = volume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        SetPlaylistForScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPlaylistForScene(scene.buildIndex);
    }

    void SetPlaylistForScene(int index)
    {
        if (musicSource.isPlaying) musicSource.Stop();
        songIndex = 0;

        if (index == 1) activePlaylist = level1Songs;
        else if (index == 2) activePlaylist = level2Songs;
        else if (index == 3) activePlaylist = level3Songs;
        else activePlaylist = null;

        if (activePlaylist != null && activePlaylist.Count > 0)
            StartCoroutine(PlayPlaylist());
    }

    IEnumerator PlayPlaylist()
    {
        while (true)
        {
            if (activePlaylist == null || activePlaylist.Count == 0) yield break;

            var clip = activePlaylist[songIndex];
            if (clip)
            {
                musicSource.clip = clip;
                musicSource.Play();
                yield return new WaitForSeconds(clip.length);
            }

            songIndex = (songIndex + 1) % activePlaylist.Count;
        }
    }

    // 👉 Reproduce el SFX y devuelve su duración (por si quieres esperar)
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
            return clip.length;
        }
        return 0f;
    }
}
