using UnityEngine;

public class PlayfieldAutoLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform leftPaddle;
    [SerializeField] private Transform rightPaddle;
    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;

    [Header("Offsets (world units)")]
    [SerializeField] private float horizontalMargin = 0.7f;  // distance from edge to paddle center
    [SerializeField] private float verticalMargin;  // keep walls inset slightly

    private void Reset()
    {
        targetCamera = Camera.main;
    }

    private void Start()
    {
        ApplyLayout();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (!targetCamera) return;

        float vertExtent = targetCamera.orthographicSize;
        float horzExtent = vertExtent * targetCamera.aspect;
        Vector3 camPos   = targetCamera.transform.position;

        float leftEdge   = camPos.x - horzExtent;
        float rightEdge  = camPos.x + horzExtent;
        float topEdge    = camPos.y + vertExtent;
        float bottomEdge = camPos.y - vertExtent;

        float leftX  = leftEdge + horizontalMargin;
        float rightX = rightEdge - horizontalMargin;

        if (leftPaddle)
        {
            leftPaddle.position = new Vector3(leftX, leftPaddle.position.y, leftPaddle.position.z);
            leftPaddle.GetComponentInChildren<PaddleIntroSpawn>().UpdateRestPosition(leftPaddle.position);
        }

        if (rightPaddle)
        {
            rightPaddle.position = new Vector3(rightX, rightPaddle.position.y, rightPaddle.position.z);
            rightPaddle.GetComponentInChildren<PaddleIntroSpawn>().UpdateRestPosition(rightPaddle.position);
        }

        if (topWall)
        {
            var col = topWall.GetComponent<BoxCollider2D>();
            var yOffset = col.bounds.size.y / 2;
            topWall.position = new Vector3(camPos.x, topEdge + yOffset - verticalMargin, topWall.position.z);
        }

        if (bottomWall)
        {
            var col = bottomWall.GetComponent<BoxCollider2D>();
            var yOffset = col.bounds.size.y / 2;
            bottomWall.position = new Vector3(camPos.x, bottomEdge - yOffset + verticalMargin, bottomWall.position.z);
        }
    }

}
