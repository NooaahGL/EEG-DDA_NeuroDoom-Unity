using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator doorAnim;
    public GameObject areaToSpawn;

    public bool requiresKey;
    public bool reqRed, reqBlue, reqGreen;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (requiresKey){

                if(reqRed && other.GetComponent<PlayerInventory>().hasRed)
                {
                    // open door
                    doorAnim.SetTrigger("OpenDoor");
                    areaToSpawn.SetActive(true);

                    CanvasManager.Instance.ClearRedKey();
                    PlayerInventory.Instance.ClearRedKey();
                }

                if(reqBlue && other.GetComponent<PlayerInventory>().hasBlue)
                {
                    // open door
                    doorAnim.SetTrigger("OpenDoor");
                    areaToSpawn.SetActive(true);

                    CanvasManager.Instance.ClearBlueKey();
                    PlayerInventory.Instance.ClearBlueKey();
                }

                if(reqGreen && other.GetComponent<PlayerInventory>().hasGreen)
                {
                    // open door
                    doorAnim.SetTrigger("OpenDoor");
                    areaToSpawn.SetActive(true);

                    CanvasManager.Instance.ClearGreenKey();
                    PlayerInventory.Instance.ClearGreenKey();
                }
                
            }else{
                
                // open door
                doorAnim.SetTrigger("OpenDoor");
                areaToSpawn.SetActive(true);

            }


            
        }
    }
}
