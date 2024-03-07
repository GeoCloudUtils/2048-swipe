using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text recordText;

    [SerializeField] private Button reloadButton;
    [SerializeField] private Button tryAgainButton;

    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private Board board;

    private int score = 0;

    public void Start()
    {
        scoreText.SetText("SCORE\n" + score);
        recordText.SetText("HIGH SCORE\n" + PlayerPrefs.GetInt("RECORD", 0).ToString());

        board.OnCombine += OnCombine;
        board.OnGameOverAction += GameOver;

        reloadButton.onClick.AddListener(StartNewGame);
        tryAgainButton.onClick.AddListener(StartNewGame);
    }

    private void GameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void OnCombine(int value)
    {
        score += value;
        scoreText.SetText("SCORE\n" + score.ToString());
        int record = PlayerPrefs.GetInt("RECORD", 0);
        if (record <= score)
        {
            recordText.SetText("SCORE\n" + score.ToString());
            PlayerPrefs.SetInt("HIGH SCORE", score);
        }
    }
}
