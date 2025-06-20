using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseBridge : MonoBehaviour
{
    public Transform player;
    public Transform bridge; // The bridge to move
    public float triggerRadius = 2f;
    public KeyCode interactionKey = KeyCode.F;
    public AudioSource switchAudioSource;
    public AudioSource bridgeAudioSource;
    public float raiseAmount = 3f; // How high the bridge moves
    public float moveSpeed = 2f;

    private Vector3 startBridgePos;
    private Vector3 targetBridgePos;
    private bool isRaising = false;
    private bool activated = false;
    private float time;
    public float timeLimit;

    void Start()
    {
        if (bridge != null)
        {
            startBridgePos = bridge.position;
            targetBridgePos = startBridgePos + Vector3.up * raiseAmount;
        }
    }

    void Update()
{
    if (player == null || bridge == null)
        return;

    float distance = Vector3.Distance(player.position, transform.position);

    if (!activated && distance < triggerRadius && Input.GetKeyDown(interactionKey))
    {
        activated = true;
        isRaising = true;
        time = 0f;
        soundPlay();
    }

    if (isRaising)
    {
        bridge.position = Vector3.MoveTowards(bridge.position, targetBridgePos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(bridge.position, targetBridgePos) < 0.01f)
        {
            bridge.position = targetBridgePos;
            isRaising = false;
        }

        time += Time.deltaTime;

        if (time >= timeLimit)
        {
            if (bridgeAudioSource.isPlaying)
                bridgeAudioSource.Stop();
            if (switchAudioSource.isPlaying)
                switchAudioSource.Stop();
        }
    }
}

    void soundPlay()
    {
        if (bridgeAudioSource != null)
        {
            bridgeAudioSource.Play();
        }
        if (switchAudioSource != null)
        {
            switchAudioSource.Play();
        }

    }
}