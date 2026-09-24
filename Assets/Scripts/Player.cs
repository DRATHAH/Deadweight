using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    public InputActionReference moveRef;
    public float moveSpeed = 1f;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        MoveServerAuth();
    }

    private void MoveServerAuth()
    {
        Vector2 input = moveRef.action.ReadValue<Vector2>();
        MoveServerRpc(input);
    }

    [Rpc(SendTo.Server,InvokePermission = RpcInvokePermission.Everyone)] 
    void MoveServerRpc(Vector3 inputVector)
    {
        float horInput = inputVector.x;
        float vertInput = inputVector.y;
        Vector3 movement = new Vector3(horInput, 0, vertInput);

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
