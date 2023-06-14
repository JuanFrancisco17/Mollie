using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider = null;

    [SerializeField] private TextMeshProUGUI volumeText = null;

    private void Start()
    {
        LoadValues();
    }

    public void VolumeSlider(float volume)
    {
        volumeText.text = volume.ToString("0.0");
    }

    public void SaveButton()
    {
        float volumeValue = volumeSlider.value;
        PlayerPrefs.SetFloat("VolumeVal", volumeValue);
        LoadValues();
    }

    void LoadValues()
    {
        float volumeValue = PlayerPrefs.GetFloat("VolumeVal");
        volumeSlider.value = volumeValue;
        AudioListener.volume = volumeValue;
    }
}
