using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    [SerializeField]
    AudioSource audioSourcePrefab;
    [SerializeField]
    int poolSize;
    [SerializeField]
    float poolRefillPeriod;

    List<AudioSource> freeSources;
    List<AudioSource> usedSources;

    void Awake () {
        // if an audio manager already exists in the scene, self destroy
        if(Instance != null) {
            Destroy(this.gameObject);
            return;
        }
        // Initialize singleton
        Instance = this;
        // Initialise lists
        freeSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        // Dont destroy on load
        DontDestroyOnLoad(gameObject);
        // Start pool maintenance routine
        StartCoroutine(MaintainPool());
    }

    private IEnumerator MaintainPool() {
        while(true) {
            if(freeSources.Count < poolSize / 2.0f) {
                RefillPool();
            }
            yield return new WaitForSeconds(poolRefillPeriod);
        }
    }
    public AudioSource Play(AudioClip clip, System.Action<AudioSource> modifier, bool canRepeat = true) {
        // Dont do anything if the clip is null
        if (clip == null) {
            return null;
        }
        // Get an audio source
        AudioSource audioSource = GetAudioSource();
        // Set default params
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.pitch = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.panStereo = 0f;
        audioSource.time = 0f;
        audioSource.volume = 0.95f;
        audioSource.outputAudioMixerGroup = audioSourcePrefab.outputAudioMixerGroup;
        audioSource.priority = audioSourcePrefab.priority;
        // Apply user-specified modifiers
        modifier(audioSource);
        // Play sound
        audioSource.Play();
        return audioSource;
    }

    private AudioSource GetAudioSource() {
        AudioSource source;
        if(freeSources.Count == 0) {
            RefillPool();
        }
        source = freeSources[freeSources.Count - 1];
        freeSources.RemoveAt(freeSources.Count - 1);
        usedSources.Add(source);
        source.gameObject.SetActive(true);
        return source;
    }

    private void RefillPool() {
        int missing = Mathf.Max(0, poolSize - freeSources.Count);
        for(int i = 0; i < missing; ++i) {
            AudioSource source = Instantiate(audioSourcePrefab);
            source.transform.SetParent(transform);
            freeSources.Add(source);
        }
    }

    public void Stop(AudioSource source) {
        if(source == null) {
            return;
        }
        // Stop source
        source.Stop();
        // Search for source
        int index = usedSources.IndexOf(source);
        // If found, return to free sources list
        if(index >= 0) {
            usedSources.RemoveAt(index);
            freeSources.Add(source);
            source.gameObject.SetActive(false);
        }
    }


}
