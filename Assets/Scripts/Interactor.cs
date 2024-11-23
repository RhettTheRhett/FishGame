using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractable {
    public void Interact();
    public void Highlight();
    public void UnHighlight();
}

public class Interactor : MonoBehaviour
{

    public Transform interactorSource;
    public float interactRange;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray r = new Ray(interactorSource.position, interactorSource.forward);
        Debug.DrawRay(interactorSource.position, interactorSource.forward * interactRange, Color.red);
        if (Physics.Raycast(r, out RaycastHit hitInfo, interactRange)) {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                interactObj.Highlight();
                if (Input.GetKeyDown(KeyCode.E)) {
                    interactObj.Interact();
                }

            }           
        }

    }
}
