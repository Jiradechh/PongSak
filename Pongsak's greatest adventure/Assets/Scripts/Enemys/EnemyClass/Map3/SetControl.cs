using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetControl : MonoBehaviour
{
    public GameObject rubikShuffle;
    public List<PowerOfDoritos> allFinished;

    public float duration;
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            allFinished.Add(transform.GetChild(i).gameObject.GetComponent<PowerOfDoritos>());
            allFinished[i].duration = duration;
        }
        // foreach (PowerOfDoritos p in allFinished)
        // {
        //     p.gameObject.SetActive(true);
        // }
        int rnd = Random.Range(0, allFinished.Count + 1);
        rubikShuffle = allFinished[rnd].gameObject;
    }

}
