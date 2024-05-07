using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierStack : MonoBehaviour
{
    private const int stickyStackLimit = 10;
    private List<GameObject> stickyStack = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CreateStickyStack(GameObject go)
    {
        if (stickyStack.Count >= stickyStackLimit)
        {
            stickyStack.Add(go);
            if (stickyStack.Count <= stickyStackLimit)
            {
                CreateStickyStack();
            }
        }
    }

    private void CreateStickyStack()
    {
        for (int i = 0; i < stickyStack.Count; i++)
        {
            stickyStack[i].SetActive(false); //depooled
        }
        stickyStack.Clear();
    }
}
