using UnityEngine;
using UnityEngine.UI;

public class RayController : MonoBehaviour
{
    [SerializeField] private Text rotationText;
    [SerializeField] private Text northText;
    [SerializeField] private Text eastText;
    [SerializeField] private Text southText;
    [SerializeField] private Text westText;

    private Ray _rays;

    private void Start()
    {
    }

    private void Update()
    {
        northText.text = "Ray N : " + CastRay(transform.forward);
        southText.text = "Ray S : " + CastRay(-transform.forward);
        eastText.text = "Ray E : " + CastRay(transform.right);
        westText.text = "Ray W : " + CastRay(-transform.right);
        rotationText.text = "Rotation : " + transform.eulerAngles.y;
    }

    private string CastRay(Vector3 direction)
    {
        var position = transform.position;
        RaycastHit hit;
        Physics.Raycast(position,
                        direction,
                        out hit,
                        Mathf.Infinity,
                        LayerMask.GetMask("Environment"));
        Debug.DrawRay(position, direction);
        var colliderNotExists = ReferenceEquals(hit.collider, null);
        return (colliderNotExists ? -1f : hit.distance).ToString(); //TODO: expensive null check
    }
}