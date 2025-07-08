using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideWallOnKeyPress : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Transform triggerZone;
    public float triggerRadius = 2f;
    public KeyCode interactionKey = KeyCode.F;

    [Header("Movement Settings")]
    public Vector3 localMoveDirection = Vector3.right;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    private bool isOpening = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool hasMoved = false;

    [Header("Sound Settings")]
    public AudioSource switchSoundSource;
    public AudioSource wallSoundSource;
    private float soundTime;
    public float soundTimer;

    public bool IsPlayerNearby { get; private set; } = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + transform.TransformDirection(localMoveDirection.normalized) * moveDistance;
    }

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, triggerZone.position);
        IsPlayerNearby = (distance < triggerRadius);

        if (!hasMoved && IsPlayerNearby)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                isOpening = true;
                hasMoved = true;
                soundTime = 0f;
                soundPlay();
            }
        }

        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, moveSpeed * Time.deltaTime);

            soundTime += Time.deltaTime;

            if (soundTime >= soundTimer)
            {
                wallSoundSource.Stop();
                switchSoundSource.Stop();
            }

        }
    }

    void soundPlay()
    {
        if (wallSoundSource != null)
        {
            wallSoundSource.Play();
        }

        if (switchSoundSource != null)
        {
            switchSoundSource.Play();
        }
    }
}