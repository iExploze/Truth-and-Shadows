using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowHealth : MonoBehaviour
{
    // Start is called before the first frame update
    // private int _health = 100;
    private ShadowHealthBar _healthBar;
    private bool _damaged;
    void Start()
    {
        _damaged = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TakeDamage()
    {
        if (_damaged)
        {
            // _health -= 5;
            // call health bar update
            _healthBar.takeDamage(5);
        }
        return;
    }
}
