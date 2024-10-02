using System;
using System.Collections;
using UnityEngine;

public class FollowGaze : MonoBehaviour
{
    public Transform playerHead;
    private WristMenu wristMenu;

    public float DistanceFromFace = 1.5f;
    public float SpeedScalingPower = 4;
    public float MinSpeed = 0.75f;

    [Range(0f, 1f)]
    public float UnlockDotProduct = 0.75f;
    [Range(0f, 1f)]
    public float RelockDotProduct = 0.95f;

    private bool Locked = true;

    public Vector3 DefaultMenuPosition => playerHead.position + new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized * DistanceFromFace;

    public bool Active = false;

    public float AnimationDuration = 0f;
    private Coroutine ActiveCoroutine = null;
    private Vector3 InitialScale;

    public GrabMaterials materialSwapper;

    public void Toggle()
    {
        Active = !Active;
        if (Active)
            Activate();
        else
            Deactivate();
    }

    public void SetActive(bool active)
    {
        this.gameObject.SetActive(active);
        if (active)
        {
            transform.parent = null;
            if (playerHead == null)
                playerHead = Camera.main.transform;
        }
    }

    public void Activate()
    {
        Active = true;
        this.SetActive(true);

        if (AnimationDuration <= 0) {
            this.transform.position = DefaultMenuPosition;
            Locked = true;
            OrientToFaceUser();
        } else {
            Locked = true;
            materialSwapper?.ApplyGhostShader();
            ActiveCoroutine = StartCoroutine(MenuFoldOut(false, () => {
                this.transform.position = DefaultMenuPosition;
                this.transform.localScale = InitialScale;
                Locked = true;
                materialSwapper?.RemoveGhostShader();
                OrientToFaceUser();
            }));
        }
    }

    public void Deactivate()
    {
        Active = false;
        this.SetActive(true);

        if (AnimationDuration <= 0) {
            this.transform.position = DefaultMenuPosition;
            Locked = true;
            OrientToFaceUser();
        } else {
            Locked = true;
            materialSwapper?.ApplyGhostShader();
            ActiveCoroutine = StartCoroutine(MenuFoldOut(true, () => {
                materialSwapper?.RemoveGhostShader();
                this.gameObject.SetActive(false);
            }));
        }
    }

    private void OrientToFaceUser()
    {
        this.transform.LookAt(playerHead.position);
        this.transform.forward *= -1;
    }

    private void Awake()
    {
        this.transform.SetParent(null, true); // detach from anything we're attached to
        InitialScale = this.transform.localScale;

        if (playerHead == null)
            playerHead = Camera.main.transform;

        wristMenu = FindObjectOfType<WristMenu>(true);
    }

    private void Update()
    {
        if (ActiveCoroutine != null)
            return; // Don't interrupt coroutine

        float dot = GetMenuDotProduct();

        if (Locked) {
            Locked = dot >= UnlockDotProduct;
        } else {
            Locked = dot >= RelockDotProduct;

            UpdateMenuPosition(dot);
        }
    }

    private IEnumerator MenuFoldOut(bool invert, Action onComplete)
    {
        float start = Time.time;

        while (Time.time - start < AnimationDuration) {
            float interp = (Time.time - start) / AnimationDuration;
            if (invert)
                interp = 1f - interp;

            this.transform.position = Vector3.Lerp(wristMenu.transform.position, DefaultMenuPosition, interp);
            this.transform.localScale = interp * InitialScale;
            OrientToFaceUser();
            yield return new WaitForEndOfFrame();
        }
        ActiveCoroutine = null;
        onComplete();
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
