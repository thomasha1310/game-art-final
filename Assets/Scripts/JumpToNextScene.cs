using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpToNextScene : MonoBehaviour
{

    [SerializeField, Tooltip("The name of the scene to load when the user presses the jump button.")]
    private String nextScene;



    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Jump"))
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}
