using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFxPlayer : MonoBehaviour
{

    //[RequireComponent(typeof(AudioSource))]

    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip wallSlide;
    [SerializeField] private AudioClip coin;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip victory;

    private AudioSource aSource;
    Dictionary<string, AudioClip> fxs = new Dictionary<string, AudioClip>();

    void Start()
    {
        aSource = GetComponent<AudioSource>();
        fxs.Add("Jump", jump);
        fxs.Add("Land", land);
        fxs.Add("WallSlide", wallSlide);
        fxs.Add("Coin", coin);
        fxs.Add("Death", death);
        fxs.Add("GameOver", gameOver);
        fxs.Add("Victory", victory);
    }

    public void PlayFxByName(string fxName)
    {
        Debug.Log("play sound fx: " + fxName);
        if (fxs.ContainsKey(fxName) && fxs[fxName])
        {
            aSource.PlayOneShot(fxs[fxName]);
        }
    }
}
