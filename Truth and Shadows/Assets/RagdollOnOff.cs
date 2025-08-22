using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollOnOff : MonoBehaviour
{
    public CapsuleCollider mainCollider;
    public CapsuleCollider lightCollider;
    public GameObject SesomRig;
    public Animator SesomAnimator;
    public lightCharacterDetection lightDetection;
    public AudioSource deathSound;

    // Start is called before the first frame update
    void Start()
    {
        GetRagdollBits();
        RagdollModeOff();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.gameObject.tag == "EnemyTag")
    //    {
    //        RagdollModeOn();
    //    }
    //}

    Collider[] ragDollColliders;
    Rigidbody[] limbsRigidbodies;

    void GetRagdollBits()
    {
        ragDollColliders = SesomRig.GetComponentsInChildren<Collider>();
        limbsRigidbodies = SesomRig.GetComponentsInChildren<Rigidbody>();
    }

    public void RagdollModeOn()
    {
        SesomAnimator.enabled = false;

        foreach (Collider col in ragDollColliders) { col.enabled = true; }

        foreach (Rigidbody rig in limbsRigidbodies) { rig.isKinematic = false; }

        mainCollider.enabled = false;
        lightCollider.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        lightDetection.enabled = false;

        deathSound.Play();

        Debug.Log("on");
    }

    public void RagdollModeOff()
    {
        foreach (Collider col in ragDollColliders) { col.enabled = false; }

        foreach (Rigidbody rig in limbsRigidbodies) { rig.isKinematic = true; }

        SesomAnimator.enabled = true;
        mainCollider.enabled = true;
        lightCollider.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;

        lightDetection.enabled = true;

        Debug.Log("off");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
