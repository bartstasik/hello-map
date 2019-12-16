using UnityEngine;
using UnityEngine.UI;

public class RayController : MonoBehaviour
{
    [SerializeField] private Text rotationText;
    [SerializeField] private Text jumpText;
    [SerializeField] private Text northText;
    [SerializeField] private Text northeastText;
    [SerializeField] private Text eastText;
    [SerializeField] private Text southeastText;
    [SerializeField] private Text southText;
    [SerializeField] private Text southwestText;
    [SerializeField] private Text westText;
    [SerializeField] private Text northwestText;

    private Ray _rays;

    private void Start()
    {
    }

    private void Update()
    {
        northText.text = "N : " + CastRay(transform.forward);
        southText.text = "S : " + CastRay(-transform.forward);
        eastText.text = "E : " + CastRay(transform.right);
        westText.text = "W : " + CastRay(-transform.right);
        jumpText.text = "Jump : " + CastRay(Vector3.down);
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