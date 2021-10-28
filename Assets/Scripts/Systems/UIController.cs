using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Menus")]
    public GameObject upgradeMenu;
    public GameObject shopMenu;
    public GameObject pauseMenu;
    public GameObject endgameMenu;

    private int levelToRestart;
    public enum Menus
    {
        UpgradeMenu,
        ShopMenu,
        PauseMenu,
        EndgameMenu
    }

    public Menus menus;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        levelToRestart = SceneManager.GetActiveScene().buildIndex;
    }

    public void EnableUpdateMenu()
    {
        upgradeMenu.SetActive(true);
    }

    public void DisableUpdateMenu()
    {
        upgradeMenu.SetActive(false);
    }

    public void EnablePauseMenu()
    {
        upgradeMenu.SetActive(false);
        shopMenu.SetActive(false);
        pauseMenu.SetActive(true);

    }

    public void DisablePauseMenu()
    {
        pauseMenu.SetActive(false);
        shopMenu.SetActive(true);
    }

    public void EnableEndgameMenu()
    {
        upgradeMenu.SetActive(false);
        shopMenu.SetActive(false);
        Toggle();
    }


    public void SetMenuActive()
    {
        switch (menus)
        {
            case Menus.UpgradeMenu:
                upgradeMenu.SetActive(true);

                break;
            case Menus.ShopMenu:
                shopMenu.SetActive(true);
                break;
            case Menus.PauseMenu:
                pauseMenu.SetActive(true);
                break;

            case Menus.EndgameMenu:
                EnableEndgameMenu();
                break;
            default:
                break;
        }
    }

    public void SetMenuInactive()
    {
        switch (menus)
        {
            case Menus.UpgradeMenu:
                upgradeMenu.SetActive(false);
                break;
            case Menus.ShopMenu:
                shopMenu.SetActive(false);
                break;
            case Menus.PauseMenu:
                pauseMenu.SetActive(false);
                break;
            case Menus.EndgameMenu:
                Toggle();
                break;
            default:
                break;
        }
    }

    public void Toggle()
    {
        if (endgameMenu.activeSelf==false)
        {
            endgameMenu.SetActive(true);
            GameObject.Find("FinalScoreText").GetComponent<UnityEngine.UI.Text>().text = "Score: " + LevelStats.instance.currentScore;
            Time.timeScale = 0;      
        }
        else
        {
            endgameMenu.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void Retry()
    {
        Toggle();
        if (SceneController.instance)
        {
            SceneController.instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Exit()
    {
        Toggle();
        if (SceneController.instance)
        {
            SceneController.instance.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void GoToNextLevel()
    {
        Toggle();
        if (SceneController.instance)
        {
            SceneController.instance.LoadScene(levelToRestart);
        }
        else
        if (GameManager.instance.actualLevel < GameManager.instance.worldList.Count - 1)
        {
            GameManager.instance.SetNextLevelWorld();
            SceneManager.LoadScene(levelToRestart);
        }
    }
    public void SlowGame()
    {
        //TODO try to slow to stop game when gameover
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UIController))]
public class UIControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UIController uIcontroller = (UIController) target;
        DrawDefaultInspector();
        if (GUILayout.Button("Enable"))
        {
            uIcontroller.SetMenuActive();
        }

        if (GUILayout.Button("Disable"))
        {
            uIcontroller.SetMenuInactive();
        }
    }
}
#endif

