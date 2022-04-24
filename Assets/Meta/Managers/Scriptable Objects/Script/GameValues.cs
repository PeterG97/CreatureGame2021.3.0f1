using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Values", menuName = "Global Values/Game Values")]
public class GameValues : ScriptableObject
{
    //Only changeable in editor
    [SerializeField] private float gameCellSize = 1;
    public float GameCellSize { get => gameCellSize; private set { } }

    //Changeable at runtime
    [SerializeField] private int targetFPS;
    public int TargetFPS { get => targetFPS; set => targetFPS = value; }
}
