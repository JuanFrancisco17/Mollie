using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider = null;

    [SerializeField] private TextMeshProUGUI volumeText = null;

    private bool isFullscreen = false;
    public GameObject pauseMenuUI;
    private bool isPaused = false;
    [SerializeField] private bool isMainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private ThirdPersonCamera tpCam;
    [SerializeField] private YawController yController;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audio;
    private void Start()
    {
        LoadValues();
    }

    private void Update()
    {
        if (!isMainMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }
        
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Resume()
    {
        anim.Play("DownMove");
        BasicCharacterStateMachine.instance.enabled = true;
        tpCam.enabled = true;
        yController.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Back()
    {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Settings()
    {
        settingsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        anim.Play("UpMove");
        audio.Play();
        BasicCharacterStateMachine.instance.enabled = false;
        tpCam.enabled = false;
        yController.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0f;
        isPaused = true;
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

    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;

        if (isFullscreen)
        {
            SetFullscreen();
        }
        else
        {
            SetWindowed();
        }
    }

    private void SetFullscreen()
    {
        // Cambiar a pantalla completa
        Screen.fullScreen = true;
    }

    private void SetWindowed()
    {
        // Cambiar a ventana
        Screen.fullScreen = false;
    }
}
