using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentOnStart : MonoBehaviour
{
    [SerializeField] Transform parentContainer;
    [SerializeField] Vector3 localPosition;
    [SerializeField] Quaternion localRotation;
    // Start is called before the first frame update
    void Start()
    {
        if(parentContainer != null)
        {
            ParentUI(parentContainer);
        }
    }

    public void ParentUI(Transform parent)
    {
        parentContainer = parent;
        transform.SetParent(parentContainer);
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
