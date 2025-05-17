using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[ExecuteInEditMode()]
public class ShadowHealthBar : MonoBehaviour
{
    // private int _max;
    //
    // private int _current;
    //
    // public Image mask;
    public Image healthBar;
    private int _maxHealth;

    private int _health;
    // private int _health = 100;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        _maxHealth = 100;
        _health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        // getCurrentFill();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            takeDamage(5);
        }
    }
    
    public void takeDamage(int damage)
    {
        _health -= damage;
        float fill = (float) _health / (float) _maxHealth;
        healthBar.fillAmount = fill;
    }

}
