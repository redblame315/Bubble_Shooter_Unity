/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/03/2023 ------------
 ##########################################################
 ##########################################################
*/

using UnityEngine;
using System.Collections;

public class GameSceneController : MonoBehaviour
{
    public static GameSceneController instance;
    GameplayController gamePlayController;

    public UnityEngine.UI.Text txtWinScore, txtWinHighScore;


    public static bool needShowFade = false;

    private void Awake()
    {
        if (instance == null )
        {
            instance = this;
        }
    }
    void Start()
    {
        GlobalData.WHITE_COUNT = 6;

        needShowFade = false;
        gamePlayController = GameObject.FindObjectOfType<GameplayController>();

    }

    void Update()
    {
    }

    // Scene Controlling
    // Menu Popup Area
    public GameObject WinMenu;
    
    public void ShowWinMenu()
    {
        if (gamePlayController.CurrentScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", gamePlayController.CurrentScore);
        }
        txtWinScore.text = gamePlayController.CurrentScore.ToString();
        txtWinHighScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        Debug.Log("game ended...");
        WinMenu.SetActive(true);
    }

    public void ReplayInWinMenu()
    {
        WinMenu.SetActive(false);
        txtWinScore.text = "0";
        txtWinHighScore.text = "0";
        Application.LoadLevel("GameScene");
        gamePlayController.gameEnded = false;
    }
    public void RestartLevel()
    {
        Application.LoadLevel("GameScene");
    }


}
