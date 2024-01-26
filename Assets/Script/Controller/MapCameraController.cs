using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public GameObject playerGo;
    void LateUpdate()
    {
        transform.position = new Vector3(playerGo.transform.position.x, 20, playerGo.transform.position.z);
    }
}
