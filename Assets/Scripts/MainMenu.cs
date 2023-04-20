using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField seedInputField;

    public void StartGame()
    {
        int.TryParse(seedInputField.text, out Galaxy.seed);
        LevelLoader.LoadScene(1);
    }
}
