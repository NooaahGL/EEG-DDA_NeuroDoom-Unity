using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    public bool isHealth;
    public bool isArmor;
    public bool isAmmo;

    public int amount;

    public static event System.Action OnAmmoPickupCollected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            
            if (isHealth)
            {
                other.GetComponent<PlayerHealth>().GiveHealth(amount, this.gameObject);
            }

            if (isAmmo)
            {
                if (other.GetComponentInChildren<gun>().GiveAmmo(amount, this.gameObject))
                    OnAmmoPickupCollected?.Invoke();
            }

            if(isArmor)
            {
                other.GetComponent<PlayerHealth>().GiveArmor(amount, this.gameObject);
            }

        }
    }
}
