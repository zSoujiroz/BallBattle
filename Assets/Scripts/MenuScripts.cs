using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class MenuScripts : MonoBehaviour
{
    public Toggle arMode;

    void Start()
    {
        arMode.isOn = GameOptions.sharedinstance.GetArMode();

        arMode.onValueChanged.AddListener(delegate {
            ToggleValueChanged(arMode);
        });
    }

    void ToggleValueChanged(Toggle change)
    {
        Debug.Log("arMode is " + arMode.isOn);
        if(arMode.isOn)
        {
        #if UNITY_ANDROID
            if (CheckAndroidCameraPermission())
                SceneManager.LoadScene(1);     
            else
                RequestPermission(Permission.Camera);
        #endif 
        }   
    }

    public void ExitVRMode()
    {
        Debug.Log("Exit click");
        SceneManager.LoadScene(0);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        GameManager.instance.SetPauseGame(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        GameManager.instance.SetPauseGame(false);
    }

    public void ExitMatch()
    {
        GameManager.instance.ResetPlayer();
    }

    public void HomeButton(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void PlayAIMode(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void PlayMultiplayerMode(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

#if UNITY_ANDROID
    private bool CheckAndroidCameraPermission() 
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Camera)) 
        {
            return true;
        }
        return false;
    }

    internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
        if (permissionName == Permission.Camera)
        {
            SceneManager.LoadScene(1);
        }
    }

    internal void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }

    public void RequestPermission(string permissionName)
    {
        if (Permission.HasUserAuthorizedPermission(permissionName))
        {
            // The user authorized use of the microphone.
            Debug.Log($"Permiss {permissionName} has been granted");
        }
        else
        {
            bool useCallbacks = false;
            if (!useCallbacks)
            {
                // We do not have permission to use the microphone.
                // Ask for permission or proceed without the functionality enabled.
                Permission.RequestUserPermission(permissionName);
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                Permission.RequestUserPermission(permissionName, callbacks);
            }
        }
    }
#endif
}
