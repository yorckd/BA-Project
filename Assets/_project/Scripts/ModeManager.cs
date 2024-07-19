using UnityEngine;

public class ModeManager : MonoBehaviour
{
    private static ModeManager instance;

    private bool isMotionControl = true;
    private bool isPilot = true;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void setMotionMode()
    {
        isMotionControl = true;
    }

    public void setKeyboardMode()
    {
        isMotionControl = false;
    }

    public bool IsMotionMode()
    {
        return isMotionControl;
    }

    public void setPilot(bool value)
    {
        isPilot = value;
    }

    public bool IsPilot()
    {
        return isPilot;
    }
}
