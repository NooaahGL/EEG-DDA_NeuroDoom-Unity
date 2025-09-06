using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public TextMeshProUGUI health;
    public TextMeshProUGUI armor;
    public TextMeshProUGUI ammo;

    public Image healthIndicator;

    public Sprite healthy; //healthy
    public Sprite health2;
    public Sprite health3;
    public Sprite health4;
    public Sprite health5; //dead
    public Sprite dead;

    public GameObject redKey;
    public GameObject blueKey;
    public GameObject greenKey;

    private static CanvasManager _instance;
    public static CanvasManager Instance 
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

    // update the UI
    public void UpdateHealth(int healthValue)
    {
        health.text = healthValue.ToString() + "%";
        UpdateHealthIndicator(healthValue);
    }

    public void UpdateArmor(int armorValue)
    {
        armor.text = armorValue.ToString() + "%";
    }

    public void UpdateAmmo(int ammoValue)
    {
        ammo.text = ammoValue.ToString();
    }

    public void UpdateHealthIndicator(int healthValue)
    {
        if(healthValue >= 80) //healthy
        {
            healthIndicator.sprite = healthy;
        }
        if(healthValue < 80 && healthValue >= 60)
        {
            healthIndicator.sprite = health2;
        }
        if(healthValue < 60 && healthValue >= 40)
        {
            healthIndicator.sprite = health3;
        }
        if(healthValue < 40 && healthValue >= 20)
        {
            healthIndicator.sprite = health4;
        }
        if(healthValue < 20 && healthValue > 0)
        {
            healthIndicator.sprite = health5;
        }
        if(healthValue <= 0)
        {
            healthIndicator.sprite = dead; // dead face
        }
    }
    
    public void UpdateKeys(string keyColor)
    {
        if (keyColor =="red")
        {
            redKey.SetActive(true);
        }
        if (keyColor =="green")
        {
            greenKey.SetActive(true);
        }
        if (keyColor =="blue")
        {
            blueKey.SetActive(true);
        }
    }

    public void ClearKeys()
    {
        blueKey.SetActive(false);
        greenKey.SetActive(false);
        redKey.SetActive(false);
    }

    public void ClearRedKey()
    {
        redKey.SetActive(false);
    }

    public void ClearBlueKey()
    {
        blueKey.SetActive(false);
    }
    
    public void ClearGreenKey()
    {
        greenKey.SetActive(false);
    }
    
}
