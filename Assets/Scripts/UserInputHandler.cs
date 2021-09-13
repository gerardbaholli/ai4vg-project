using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UserInputHandler : MonoBehaviour
{
    RaceStatus raceStatus;

    [SerializeField] Button buttonRunAndStop;
    [SerializeField] Button buttonSafetyCar;
    [SerializeField] Button buttonReset;
    [SerializeField] TextMeshProUGUI textRaceAndStop;
    [SerializeField] TextMeshProUGUI textSafetyCar;


    private void Start()
    {
        raceStatus = FindObjectOfType<RaceStatus>();
        textRaceAndStop.SetText("Race");
        textSafetyCar.SetText("Safety Car");

        buttonSafetyCar.interactable = false;
        buttonReset.interactable = false;
    }

    private void FixedUpdate()
    {
        
    }

    public void SwitchRaceAndStop() {
        raceStatus.start = !raceStatus.start;
        buttonSafetyCar.interactable = true;
        buttonReset.interactable = true;
        buttonSafetyCar.enabled = true;

        if (raceStatus.start == true)
        {
            textRaceAndStop.SetText("Stop");
        }
        else
        {
            textRaceAndStop.SetText("Race");
        }
    }

    public void SwitchSafetyCar()
    {
        raceStatus.saferyCar = !raceStatus.saferyCar;

        if (raceStatus.saferyCar == true)
        {
            ChangeButtonColor(buttonSafetyCar, Color.yellow);
        }
        else
        {
            ChangeButtonColor(buttonSafetyCar, Color.white);
        }
    }

    private void ChangeButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        button.colors = colors;
    }

    public void ReloadScene(float delayOnSceneLoad)
    {
        StartCoroutine(WaitForSceneLoad(SceneManager.GetActiveScene().buildIndex, delayOnSceneLoad));
    }

    private IEnumerator WaitForSceneLoad(int sceneIndex, float delayOnSceneLoad)
    {
        yield return new WaitForSeconds(delayOnSceneLoad);
        SceneManager.LoadScene(sceneIndex);
    }

}
