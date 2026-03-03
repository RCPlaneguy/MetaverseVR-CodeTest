using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameTimer;
    [SerializeField] private GameObject completeScreen;
    [SerializeField] private TextMeshProUGUI completeScreenTime;

    private bool _countingDown;
    private float _elapsedTime;

    public static bool GamePaused;
    private const string MainScene = "MainScene";

    private void Start()
    {
        _countingDown = true;
        _elapsedTime = 0f;
        gameTimer.transform.parent.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_countingDown)
            return;
        _elapsedTime += Time.deltaTime;
        gameTimer.text = $"Gametime: {_elapsedTime:0.0}s";
    }

    public void PlayerDocked()
    {
        _countingDown = false;
        gameTimer.transform.parent.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GamePaused = true;
        Time.timeScale = 0f;

        completeScreenTime.text = $"Time: {_elapsedTime:0.0}s";
        completeScreen.SetActive(true);
    }

    public void ResetGame()
    {
        GamePaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainScene);
    }
}
