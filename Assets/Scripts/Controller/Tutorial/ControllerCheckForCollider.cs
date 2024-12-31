using UnityEngine;
using UnityEngine.UI;
//make sure to unparent the text from the controller.
public class ControllerCheckForCollider : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;
    [SerializeField]
    Transform parentControllerComponent;
    [SerializeField]
    Transform targetControl;
    [SerializeField]
    Vector3 globalOffset;
    [SerializeField]
    Vector3 targetOffset;

    Transform cameraTransform;
    [SerializeField] LayerMask mask;
    [SerializeField] float lerpSpeed = 1000f;


    void Start()
    {
        lineRenderer.positionCount = 2;
        EventSystem.level.AddListener(LevelEvents.FINISH_TUTORIAL, OnEndTutorial);
        lineRenderer.useWorldSpace = false;
        cameraTransform = Camera.main.transform;
    }
   
    void OnDestroy()
    {
        EventSystem.level.RemoveListener(LevelEvents.FINISH_TUTORIAL, OnEndTutorial);
    }

    #region legacy
    private void Update()
    {
        Vector3 targetPosition = targetControl.TransformPoint(targetOffset);
        // print($"Target Position {targetPosition} targetControl Position {targetControl.position}");
        if (!RaycastFromPointToPoint(targetPosition, cameraTransform.position))
        {
            //if hit the controller instead of the head.
            targetPosition = parentControllerComponent.transform.position + globalOffset;
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * lerpSpeed);
        // transform.position = targetPosition;
    }

    private void LateUpdate()
    {
        UpdateLine();
    }

    void UpdateLine()
    {
        // transform.position = parentControllerComponent.transform.position + globalOffset;
        var targetposition = targetControl.InverseTransformPoint(transform.position);
        lineRenderer.SetPositions(new Vector3[] { targetControl.localPosition, targetposition });
    }

    void OnEndTutorial()
    {
        EventSystem.level.RemoveListener(LevelEvents.FINISH_TUTORIAL, OnEndTutorial);
        gameObject.SetActive(false);
        lineRenderer.enabled = false;
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawRay(ray);
    // }

    bool RaycastFromPointToPoint(Vector3 from, Vector3 to)
    {
        // Direction from start point to end point
        Vector3 direction = to - from;

        // Calculate the distance between the two points
        float distance = direction.magnitude;

        // Normalize the direction (turn it into a unit vector)
        direction.Normalize();

        // Perform the raycast
        RaycastHit hit;
        bool hasHit;
        if (Physics.Raycast(from, direction, out hit, distance, mask))
        {
            if (hit.collider.CompareTag("Player Head")) hasHit = true;
            else hasHit = false;
            // print($"{hit.collider.tag} has hit{hasHit}");
        }
        else
        {
            // print("Not hit");
            hasHit = false;
        }


        // Optionally, visualize the raycast in the scene view (for debugging purposes)
        Debug.DrawLine(from, to, hasHit ? Color.green : Color.red);

        return hasHit;
    }

    #endregion
}

