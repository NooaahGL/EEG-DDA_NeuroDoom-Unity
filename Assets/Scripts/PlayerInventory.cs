using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasRed;
    public bool hasGreen;
    public bool hasBlue;

    
    private static PlayerInventory _instance;
    public static PlayerInventory Instance 
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        CanvasManager.Instance.ClearKeys();
    }
    


    public void ClearRedKey()
    {
        hasRed = false;
    }

    public void ClearBlueKey()
    {
        hasGreen = false;
    }
    
    public void ClearGreenKey()
    {
        hasBlue = false;
    }
}
