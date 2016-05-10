﻿using UnityEngine;
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
        // Initialize singleton
        Instance = this;
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
        return source;
    }

    private void RefillPool() {
        int missing = Mathf.Max(0, poolSize - freeSources.Count);
        for(int i = 0; i < missing; ++i) {
            freeSources.Add(Instantiate(audioSourcePrefab));
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
        }
    }


}
