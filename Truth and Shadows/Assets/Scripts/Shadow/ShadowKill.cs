using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
[ExecuteInEditMode()]
public class ShadowKill : MonoBehaviour, ILightHittable
{
    // private int _max;
    //
    // private int _current;
    //
    // public Image mask;
    // public Image healthBar;
    // private int _maxHealth;
    //
    // private int _health;
    // private int _health = 100;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AAAAA");
        // _maxHealth = 100;
        // _health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        // getCurrentFill();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switchToCharacter();
        }
    }
    // Called when the character enters the light
    public void OnLightEnter(Light lightSource)
    {
        Debug.Log("OnLightEnter");
        switchToCharacter();
    }

    // Called when the character exits the light
    public void OnLightExit(Light lightSource)
    {
        
    }

    // Called while the character remains in the light
    public void OnLightStay(Light lightSource)
    {
        Debug.Log("OnLightStay");
        switchToCharacter();
    }

    public void switchToCharacter()
    {
        SceneManager.LoadScene("DavidLevel");
    }
    // public void takeDamage(int damage)
    // {
    //     if (_health - damage >= 0)
    //     {
    //         _health -= damage;
    //         float fill = (float) _health / (float) _maxHealth;
    //         healthBar.fillAmount = fill;
    //     }
    //     
    // }
    //
    // public void heal(int heal)
    // {
    //     if (_health + heal <= _maxHealth)
    //     {
    //         _health += heal;
    //         float fill = (float) _health / (float) _maxHealth;
    //         healthBar.fillAmount = fill;
    //     }
    //     
    // }

}