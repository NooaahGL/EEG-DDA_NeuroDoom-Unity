using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public bool isRedKey;
    public bool isGreenKey;
    public bool isBlueKey;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isRedKey)
            {
                other.GetComponent<PlayerInventory>().hasRed = true;
                CanvasManager.Instance.UpdateKeys("red");
            }

            if (isGreenKey)
            {
                other.GetComponent<PlayerInventory>().hasGreen = true;
                CanvasManager.Instance.UpdateKeys("green");
            }

            if (isBlueKey)
            {
                other.GetComponent<PlayerInventory>().hasBlue = true;
                CanvasManager.Instance.UpdateKeys("blue");
            }
            
            Destroy(gameObject);
        }
    }
}
