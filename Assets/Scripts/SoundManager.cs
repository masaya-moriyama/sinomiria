using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip battle;
    public AudioClip battle2;
    public AudioClip battle3;

    public AudioSource audioSourceBgm;

    private int loopCount = 1;

    public static SoundManager instance { get; private set; }

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSourceBgm = GetComponent<AudioSource>();
            StartMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (!audioSourceBgm.isPlaying)
        {
            StartMusic();
        }
    }

    public void StartMusic()
    {
        switch (loopCount)
        {
            case 1:
                this.audioSourceBgm.clip = battle;
                this.audioSourceBgm.loop = false;
                loopCount++;
                break;
            case 2:
                this.audioSourceBgm.clip = battle2;
                this.audioSourceBgm.loop = false;
                loopCount++;
                break;
            case 3:
                this.audioSourceBgm.clip = battle3;
                this.audioSourceBgm.loop = true;
                break;
        }

        audioSourceBgm.Play();
    }

    public void SetVolume(float value)
    {
        SoundManager.instance.audioSourceBgm.volume = value;
    }
}
