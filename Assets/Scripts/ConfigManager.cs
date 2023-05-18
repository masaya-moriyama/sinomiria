using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigManager : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider.value = SoundManager.instance.audioSourceBgm.volume;

        slider.onValueChanged.AddListener(
            value => SoundManager.instance.SetVolume(value)
        );
    }

    public void SwitchFullHD()
    {
        Screen.SetResolution(1920, 1080, false);
    }

    public void SwitchHD()
    {
        Screen.SetResolution(1280, 720, false);
    }

    public void SwitchWQHD()
    {
        Screen.SetResolution(2560, 1440, false);
    }

    public void SwitchFullScreenMode()
    {
        Screen.fullScreen = true;
    }

    public void SwitchWindowMode()
    {
        Screen.fullScreen = false;
    }

    public void SwitchSceneToTitle()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }
}
