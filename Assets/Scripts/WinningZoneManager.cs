using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningZoneManager : MonoBehaviour
{
    [SerializeField] GameObject winningPanel;
    public AudioClip winningMusic;

    private void Start()
    {
        winningPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Win());
        }
    }

    IEnumerator Win()
    {
        SoundManager.instance.PlayOneshot(0, winningMusic);
        winningPanel.SetActive(true);
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(0);
    }
}
