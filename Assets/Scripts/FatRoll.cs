using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatRoll : MonoBehaviour
{
    public static FatRoll instance;
    public Animator anim;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.init(800);
    }

    public void MoveGordon()
    {
        StartCoroutine(CRT_RollingStones());
    }

    IEnumerator CRT_RollingStones()
    {
        yield return new WaitForSeconds(1f);
        anim.Play("RollinStones");

    }
}
