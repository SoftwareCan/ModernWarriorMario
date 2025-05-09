using System;
using UnityEngine;

public class HintDoorManager : MonoBehaviour
{
    [SerializeField] GameObject hintWay;

    void Start()
    {
        hintWay.SetActive(false);
    }
    public void ChangeColorAndOpenWay()
    {
        GetComponent<MeshRenderer>().material.color = Color.green;
        GetComponent<Light>().color = Color.green;
        hintWay.SetActive(true);
    }

   
}
