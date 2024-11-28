using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    private CinemachineVirtualCamera virtualCamera;
    private Transform playerTransform;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);


        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera component is missing!");
        }
    }

    private void Start()
    {
        FindPlayerAndSetFollow();
    }

    private void Update()
    {
     
        if (playerTransform == null)
        {
            FindPlayerAndSetFollow();
        }
    }

    private void FindPlayerAndSetFollow()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            SetFollowAndLookAt(playerTransform);
        }
        else
        {
            Debug.LogWarning("Player GameObject not found. Ensure the player has the 'Player' tag.");
        }
    }



    private void SetFollowAndLookAt(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
        else
        {
            Debug.LogWarning("Virtual Camera is not assigned.");
        }
    }
}