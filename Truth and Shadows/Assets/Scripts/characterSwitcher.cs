using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterSwitcher : MonoBehaviour
{
    public GameObject mainForm;        // Drag in MainForm child (Shadow)
    public GameObject altForm;         // Drag in AltForm child (Squid)

    private bool isInAltForm = false;

    void Start()
    {
        // Main is active at start
        mainForm.SetActive(true);
        altForm.SetActive(false);

        mainForm.tag = "Player";       // Tag the active one
        altForm.tag = "Untagged";      // Inactive one stays untagged
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleForm();
        }
    }

    void ToggleForm()
    {
        isInAltForm = !isInAltForm;

        if (isInAltForm)
        {
            // Move Squid to Shadow's position
            altForm.transform.position = mainForm.transform.position;
            altForm.transform.rotation = mainForm.transform.rotation;

            mainForm.SetActive(false);
            altForm.SetActive(true);

            // Tag switch
            mainForm.tag = "Untagged";
            altForm.tag = "Player";
        }
        else
        {
            // Move Shadow to Squid's position
            mainForm.transform.position = altForm.transform.position;
            mainForm.transform.rotation = altForm.transform.rotation;

            altForm.SetActive(false);
            mainForm.SetActive(true);

            // Tag switch
            altForm.tag = "Untagged";
            mainForm.tag = "Player";
        }
    }
}
