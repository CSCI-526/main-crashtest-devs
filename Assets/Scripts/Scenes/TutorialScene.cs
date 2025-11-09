using System.Collections;
using UnityEngine;

public class TutorialScene : MonoBehaviour
{

    public GameObject WASDTutorial;
    public GameObject DriftTutorial;
    public GameObject PowerUpTutorial;
    public GameObject PowerUpTutorial2;
    public GameObject EndTutorial;

    private bool finishedWASDTutoral = false;
    private bool finishedDriftTutoral = false;

    void FixedUpdate()
    {

        if (!finishedWASDTutoral && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            finishedWASDTutoral = true;
            StartCoroutine(HideWASDTutorial());
        }

        //edit this to change how players learn to drift
        if(finishedWASDTutoral && DriftTutorial.activeInHierarchy && !finishedDriftTutoral && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift)))
        {
            finishedDriftTutoral = true;
            StartCoroutine(HideDriftTutorial());
        }

        if(finishedDriftTutoral && PowerUpTutorial2.activeInHierarchy && (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3)))
        {
            StartCoroutine(HidePowerUpTutorial());
        }
    }

    IEnumerator HideWASDTutorial()
    {
        yield return new WaitForSeconds(2f);   // wait 2 seconds
        WASDTutorial.SetActive(false);        // then hide the tutorial

        StartCoroutine(ShowDriftTutorial());
    }

    IEnumerator ShowDriftTutorial()
    {
        yield return new WaitForSeconds(5f);

        DriftTutorial.SetActive(true);
    }

    IEnumerator HideDriftTutorial()
    {
        yield return new WaitForSeconds(2f);
        DriftTutorial.SetActive(false);

        StartCoroutine(ShowPowerUpTutorial());
    }

    IEnumerator ShowPowerUpTutorial()
    {
        yield return new WaitForSeconds(2f);
        PowerUpTutorial.SetActive(true);

        yield return new WaitForSeconds(5f);
        PowerUpTutorial.SetActive(false);
        PowerUpTutorial2.SetActive(true);
    }

    IEnumerator HidePowerUpTutorial()
    {
        PowerUpTutorial2.SetActive(false);
        yield return new WaitForSeconds(5f);

        EndTutorial.SetActive(true);
    }
}
