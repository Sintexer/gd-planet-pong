using UnityEngine;

public class MenuRotate : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed;

    private void FixedUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed);
    }

}
