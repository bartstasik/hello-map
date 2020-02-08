using System;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RayController : MonoBehaviour
{
    [NonSerialized] public double
        rotation,
        north,
        northwest,
        northeast,
        east,
        south,
        west;

    private Ray _rays;

    private void Update()
    {
        rotation = transform.eulerAngles.y;
        north = CastRay(transform.forward);
        south = CastRay(-transform.forward);
        east = CastRay(transform.right);
        west = CastRay(-transform.right);

        var sqrtHalf = Mathf.Sqrt(0.5f);
        northeast = CastRay(transform.right * sqrtHalf + transform.forward * sqrtHalf);
        northwest = CastRay(-transform.right * sqrtHalf + transform.forward * sqrtHalf);
        
        DataContainer.northRay = north;
        DataContainer.northwestRay = northwest;
        DataContainer.northeastRay = northeast;
        DataContainer.southRay = south;
        DataContainer.eastRay = east;
        DataContainer.westRay = west;
//        DataContainer.rotation = rotation;
    }

    private double CastRay(Vector3 direction)
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
        return (colliderNotExists ? -1f : hit.distance); //TODO: expensive null check
    }
}