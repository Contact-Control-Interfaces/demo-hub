using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGaze : MonoBehaviour
{
    public Transform playerHead;

    public float DistanceFromFace = 1.5f;
    public float SpeedScalingPower = 4;
    public float MinSpeed = 0.75f;

    [Range(0f, 1f)]
    public float UnlockDotProduct = 0.75f;
    [Range(0f, 1f)]
    public float RelockDotProduct = 0.95f;

    private bool Locked = true;

    private Vector3 DefaultMenuPosition => playerHead.position + new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized * DistanceFromFace;

    public bool Active => this.gameObject.activeSelf;

    public void Toggle()
    {
        SetActive(!this.gameObject.activeSelf);
    }

    public void SetActive(bool active)
    {
        this.gameObject.SetActive(active);
        if (active) {
            this.transform.position = DefaultMenuPosition;
            Locked = true;
            OrientToFaceUser();
        }
    }

    private void OrientToFaceUser()
    {
        this.transform.LookAt(playerHead.position);
        this.transform.forward *= -1;
    }

    private void Awake()
    {
        if (playerHead == null)
            playerHead = Camera.main.transform;
    }

    private void Update()
    {
        float dot = GetMenuDotProduct();

        if (Locked) {
            Locked = dot >= UnlockDotProduct;
        } else {
            Locked = dot >= RelockDotProduct;

            UpdateMenuPosition(dot);
        }
    }

    private float GetMenuDotProduct()
    {
        return Vector3.Dot(this.transform.forward, playerHead.forward);
    }

    private void UpdateMenuPosition(float dotProduct)
    {
        float dotDifference = Mathf.Max(UnlockDotProduct - dotProduct, 0); // how turned away from the locked range we are
        Vector3 newPosition = Vector3.MoveTowards(this.transform.position, DefaultMenuPosition, Time.deltaTime * Mathf.Pow(1 + dotDifference, SpeedScalingPower) * MinSpeed);
        this.transform.position = playerHead.position + DistanceFromFace * (newPosition - playerHead.position).normalized;
        OrientToFaceUser();
    }
}
