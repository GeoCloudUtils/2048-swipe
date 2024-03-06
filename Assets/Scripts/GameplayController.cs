using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private Button newGameButton;

    [SerializeField] private Board board;

    private int score = 0;

    public void Start()
    {
        scoreText.SetText("score\n" + score);
        recordText.SetText("record\n" + PlayerPrefs.GetInt("RECORD", 0).ToString());

        board.OnCombine += OnCombine;

        newGameButton.onClick.AddListener(StartNewGame);
    }

    private void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void OnCombine(int value)
    {
        score += value;
        scoreText.SetText("score\n" + score.ToString());
        int record = PlayerPrefs.GetInt("RECORD", 0);
        if (record <= score)
        {
            recordText.SetText("record\n" + score.ToString());
            PlayerPrefs.SetInt("RECORD", score);
        }
    }
}
