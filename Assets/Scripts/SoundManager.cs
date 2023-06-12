using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // Audio players components.
    public AudioSource[] effectsSource; //0= all player sfx, 1= all hannah sfx, 2= gordon roll, 3=player steps
    public AudioSource musicSource;

    public Slider musicSlider;
    public Slider sFXSlider;

    public static SoundManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //DontDestroyOnLoad permite que al cambiar escena no se destruya el gameobject
        DontDestroyOnLoad(gameObject);
    }

    // Sonido SFX por el audiosource para efectos de sonido
    public void Play(int source, AudioClip clip)
    {
        effectsSource[source].clip = clip;
        effectsSource[source].Play();
    }

    public void PlayOneshot(int source, AudioClip clip)
    {
        effectsSource[source].clip = clip;
        effectsSource[source].PlayOneShot(clip);
    }

    // Musica por el audiosource para musica
    public void PlayGordon()
    {
        effectsSource[2].Play();
    }


    void Update()
    {
        try
        {
            if (musicSlider == null)
            {
                musicSlider = GameObject.FindGameObjectWithTag("musicSlider").GetComponent<Slider>();
            }

            if (sFXSlider == null)
            {
                sFXSlider = GameObject.FindGameObjectWithTag("sfxSlider").GetComponent<Slider>();
            }
            musicSource.volume = musicSlider.value;
            effectsSource[0].volume = sFXSlider.value;
            effectsSource[1].volume = sFXSlider.value;
        }
        catch
        {
            //no hay sliders en esta escena
        }
    }


}