using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOptions : MonoBehaviour
{
    public static GameOptions sharedinstance = null;

    private bool arMode = false;

    void Awake()
    {
        if (sharedinstance == null)
        {
            sharedinstance = this;
        }
        else if (sharedinstance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void SetArMode(bool value)
    {
        this.arMode = value;
    }
    
    public bool GetArMode()
    {
        return this.arMode;
    }

}
