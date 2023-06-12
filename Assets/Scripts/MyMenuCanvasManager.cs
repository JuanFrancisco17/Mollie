using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyMenuCanvasManager : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject settingsCanvas;
    public AudioClip uisound;

    public Animation anim;
    public GameObject fundido;

    public bool isMainMenu;


    // Start is called before the first frame update
    void Start()
    {
        fundido.SetActive(false);
        if (isMainMenu == true)
        {
            mainMenuCanvas.SetActive(true);
            settingsCanvas.SetActive(false);
        }
        else
        {
            mainMenuCanvas.SetActive(false);
            settingsCanvas.SetActive(false);
        }
    }

    public void MainMenuEnable()
    {
        // SoundManager.instance.Play(uisound);
        try
        {
            mainMenuCanvas.SetActive(true);
            settingsCanvas.SetActive(false);
        }
        catch
        {

        }
    }

    public void SettingsEnable()
    {
        // SoundManager.instance.Play(uisound);
        try
        {
            mainMenuCanvas.SetActive(false);
            settingsCanvas.SetActive(true);
        }
        catch
        {

        }

    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    public void OnClickPlay()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        fundido.SetActive(true);
        anim.Play();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }

}
