using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Collections;

public class LevelSelector : MonoBehaviour
{
    public static LevelSelector instance;

    public int selectedWorld = 0;
    internal bool changing = false;

    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public CubeWorldGenerator[] worlds;

    [Header("Enemies waves setup")]
    public Wave[][] enemies;

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
    }

    private void Start()
    {
        ThemeInfo startTheme = worlds[0].GetComponent<ThemeSelector>().GetThemeInfo();
        RenderSettings.skybox.SetColor("_Tint", startTheme.backGroundColor);
        FindObjectOfType<Light>().color = startTheme.lightColor;

        if (!GameManager.instance.initiated)
            CreateWorldList();
    }
    public void SelectLevel(int levelId)
    {
        SceneController.instance.LoadScene(levelId);
    }

    public void NextWorld()
    {
        if (!changing && selectedWorld < worlds.Length - 1)
        {
            changing = true;
            GoTo(selectedWorld + 1);
        }
    }
    public void PreviousWorld()
    {
        if (!changing && selectedWorld > 0)
        {
            changing = true;
            GoTo(selectedWorld - 1);
        }
    }

    public void CreateWorldList()
    {
        for (int i = 0; i < worlds.Length; i++)
        {
            //insert the level world settings in the list containing the different levels
            WorldInfo worldInfo = new WorldInfo();
            worldInfo.nPaths = worlds[i].nPaths;
            worldInfo.wallDensity = worlds[i].wallDensity;
            worldInfo.rocksVisualReduction = worlds[i].rocksVisualReduction;
            worldInfo.rockSize = worlds[i].rockSize;
            worldInfo.numberOfMidpoints = worlds[i].numberOfMidpoints;
            worldInfo.themeInfo = worlds[i].GetComponent<ThemeSelector>().GetThemeInfo();
            worldInfo.waves = worlds[i].GetComponent<WaveInfo>().waves;
            GameManager.instance.worldList.Add(worldInfo);
        }
        GameManager.instance.initiated = true;
    }

    public void SelectWorld()
    {
        //Loads game scene with selected world
        GameManager.instance.currentWorldId = selectedWorld;
        SceneController.instance.LoadScene(1);
    }

    public void switchBetweenPanels(int panelId)
    {

        switch (panelId)
        {
            case 0:
                levelSelectPanel.SetActive(false);
                mainMenuPanel.SetActive(true);
                //MainMenuCamera.instance.MoveRight();
                break;
            case 1:
                mainMenuPanel.SetActive(false);
                levelSelectPanel.SetActive(true);
                //MainMenuCamera.instance.MoveLeft();
                break;
            default:
                break;
        }
    }

    public void GoTo(int nextIdx)
    {
        changing = true;
        StartCoroutine(GoToCube(nextIdx));
    }

    IEnumerator GoToCube(int nextIdx)
    {
        GameObject cameraObj = MainMenuCamera.instance.gameObject;
        MainMenuCamera camera = MainMenuCamera.instance;

        Light light = FindObjectOfType<Light>();
        
        Color lightColor;
        Color backGroundColor; ;

        ThemeInfo theme1 = worlds[nextIdx].GetComponent<ThemeSelector>().GetThemeInfo();
        ThemeInfo theme2 = worlds[selectedWorld].GetComponent<ThemeSelector>().GetThemeInfo();

        float waitTime = 1f;
        float doneTime = Time.time + waitTime;
        float delta;
        Vector3 position;

        while (Time.time < doneTime)
        {
            delta = ((doneTime - Time.time) / waitTime);
            position = Vector3.Lerp(worlds[nextIdx].center.position, worlds[selectedWorld].center.position, delta);

            cameraObj.transform.position = camera.offset + position;
            cameraObj.transform.LookAt(position + (Vector3.right * camera.offset.x), Vector3.up);

            lightColor = Color.Lerp(theme1.lightColor, theme2.lightColor, delta);
            backGroundColor = Color.Lerp(theme1.backGroundColor, theme2.backGroundColor, delta);

            light.color = lightColor;
            RenderSettings.skybox.SetColor("_Tint", backGroundColor);
            yield return null;
        }
        camera.idx = nextIdx;
        selectedWorld = nextIdx;
        changing = false;
        yield return null;
    }

}