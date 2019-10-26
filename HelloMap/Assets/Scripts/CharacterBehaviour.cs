using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    [SerializeField] public static float speed = 5;
    [SerializeField] public static float jumpSpeed = 3;
    [SerializeField] public static float lookSpeed = 5;
    [SerializeField] public static float rotationSpeed = 10;

    public static bool grounded = true;
}