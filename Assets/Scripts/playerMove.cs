using UnityEngine;

public class playerMove : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config; 

    public float playerSpeed = 10f;
    public float momentumDamping = 5f;

    private CharacterController myCC;
    public Animator camAnim;
    private bool isWalking;

    private Vector3 inputVector;
    private Vector3 movementVector;
    private float myGravity = -10f;

    void Start()
    {   

        myCC = GetComponent<CharacterController>();
    }

    void Update()
    {
        GetInput();
        MovePlayer();

        camAnim.SetBool(name: "isWalking", isWalking);
    }

    void GetInput()
    {
        // If we are holding down, then give us -1, 0, 1
        if(Input.GetKey(KeyCode.W) || 
           Input.GetKey(KeyCode.A) ||
           Input.GetKey(KeyCode.S) ||
           Input.GetKey(KeyCode.D) )
        {
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            inputVector.Normalize();
            inputVector = transform.TransformDirection(inputVector);

            isWalking = true;
        } else {
            // if we are not, then give us whatever inputVector was at when it was last checked and lerp it towards zero 
            inputVector = Vector3.Lerp(a: inputVector, b:Vector3.zero, momentumDamping *Time.deltaTime );
            isWalking = false;
        }
        movementVector = (inputVector * config.playerSpeed) + (Vector3.up * myGravity);
    }

    void MovePlayer()
    {
        myCC.Move(movementVector * Time.deltaTime);
    }


}
