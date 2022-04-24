using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.Load(SceneLoader.Scene.GameScene);
    }
}
