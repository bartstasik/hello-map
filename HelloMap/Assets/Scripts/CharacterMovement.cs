using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float jump = 200;
    [SerializeField] private float lookSpeed = 0;

//    [SerializeField] private Text rotationText;
//    [SerializeField] private Text northText;
//    [SerializeField] private Text northeastText;
//    [SerializeField] private Text eastText;
//    [SerializeField] private Text southeastText;
//    [SerializeField] private Text southText;
//    [SerializeField] private Text southwestText;
//    [SerializeField] private Text westText;
//    [SerializeField] private Text northwestText;

    [SerializeField] private AnimatorController runAnimation;
    [SerializeField] private AnimatorController runBackwardsAnimation;
    private Animator animator;

    private float height = 0;
    private bool grounded, idle;

    private Rigidbody body;

    private Animation animation;

    private void Start()
    {
        animator = GetComponent<Animator>();
        body = gameObject.GetComponent<Rigidbody>();
        animation = gameObject.GetComponent<Animation>();
        idle = true;
    }

    private void OnCollisionStay(Collision other)
    {
        grounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal") * transform.right;
        var vertical = Input.GetAxis("Vertical") * transform.forward;
        var jumping = Input.GetButton("Jump");
        var rotatingX = Input.GetAxis("Mouse X");
        var direction = Vector3.zero;

        if (jumping && grounded)
        {
            body.AddForce(new Vector3(0, jump, 0), ForceMode.Impulse);
            idle = false;
            grounded = false;
            animator.Play("Jump");
        }

        if (Input.GetAxis("Vertical") > 0)
        {
            animator.Play("Run");
            idle = false;
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            animator.Play("Run Backwards");
            idle = false;
        }
        else if (idle == false)
        {
            animator.Play("To Idle01");
            idle = true;
        }


//        transform.position += speed * Time.deltaTime * new Vector3(horizontal, height, vertical);
//        horizontal.Scale(vertical);
        transform.position += speed * Time.deltaTime * (vertical + horizontal);
        transform.Rotate(lookSpeed * Time.deltaTime * new Vector3(0, rotatingX));
    }
    
    static public void drawString(string text, Vector3 worldPos, Color? colour = null) {
        UnityEditor.Handles.BeginGUI();
 
        var restoreColor = GUI.color;
 
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
 
        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }
 
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }
    private void LateUpdate()
    {
//        rotationText.text = transform.rotation.ToString();
//        northText.text = transform.forward.ToString();
//        northeastText.text = Input.GetAxis("Horizontal").ToString();
//        eastText.text = (Input.GetAxis("Horizontal") * transform.forward).ToString();
//        southeastText.text = Input.GetAxis("Mouse X").ToString();
//        southText.text = idle.ToString();
    }
}