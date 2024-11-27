using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollUV : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material mat = mr.material;

        Vector2 offset = mat.mainTextureOffset;
        offset.x += speed * Time.deltaTime;

        mat.mainTextureOffset = offset;
    }
}