using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNum : MonoBehaviour, IInteractable
{
    public Material glow;
    public Material mainMaterial;
    public void Interact() {
        Debug.Log(Random.Range(0, 100));
    }
    public void Highlight() {
        GetComponent<MeshRenderer>().material = glow;
    }
    public void UnHighlight() {
        GetComponent<MeshRenderer>().material = mainMaterial;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
