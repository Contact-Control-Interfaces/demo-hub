using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BasketballSpawner : MonoBehaviour {

    public SteamVR_Action_Boolean action; //Grab Pinch is the trigger, select from inspecter
    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;//which controller

    public Transform launchTransform;
    public AudioSource launchAudio;
    public float launchSpeed;
    public int launchQueueSize = 4;
    public float fillTick = 0.1f;
    private float elapsed = 0.0f;
    public float spacing = 0.25f;
    public bool launchReady = false;
    public float fillSpeed = 0.1f;
    public bool spawnWithTrigger;

    public GameObject toSpawn;
    public List<GameObject> spawned;

    public List<GameObject> launchQueue;

    public string spawnName;

    private int numSpawned = 0;

    public bool launchFirst;
    private bool hasLaunchedFirst = false;

    void Start()
    {
        elapsed = 0.0f;
        spawned = new List<GameObject>();
        launchQueue = new List<GameObject>();

        if (toSpawn == null)
            toSpawn = this.transform.GetChild(0).gameObject;

        if (toSpawn != null)
        {
            toSpawn.gameObject.SetActive(false);
            //Spawn(); Don't spawn automatically anymore
        }
    }

    void Update()
    {
        // Trim spawned list
        List<int> toRemove = new List<int>();
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i].transform.position.y < -10) toRemove.Add(i);
        }

        for (int i = toRemove.Count - 1; i >= 0; i--)
        {
            //go from back to front to not mess up indices
            int index = toRemove[i];
            Destroy(spawned[index]);
            spawned.RemoveAt(index);
        }

        // Add to launch queue
        elapsed += Time.deltaTime;
        if (elapsed > fillTick && launchQueue.Count < launchQueueSize)
        {
            elapsed %= fillTick;
            GameObject temp = SpawnAt(launchTransform.position + maxPosition(), launchTransform.rotation, false);
            Rigidbody rb = temp.GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;
            rb.useGravity = false;
            launchQueue.Add(temp);
        }

        // Move basketballs up queue
        for (int i = 0; i < launchQueue.Count; i++)
        {
            float speed = Time.deltaTime * fillSpeed;
            if ((launchQueue[i].transform.position - (launchTransform.position + targetPosition(i))).magnitude < speed)
            {
                launchQueue[i].transform.position = launchTransform.position + targetPosition(i);
                if (i == 0)
                {
                    launchReady = true;
                    if (!hasLaunchedFirst && launchFirst)
                    {
                        hasLaunchedFirst = true;
                        Launch();
                    }
                }
            } else
            {
                launchQueue[i].transform.position += Vector3.up * speed;
            }
        }
    }

    private Vector3 targetPosition(int index)
    {
        return (spacing * index) * -Vector3.up;
    }

    private Vector3 maxPosition()
    {
        return targetPosition(launchQueueSize + 1);
    }

    public GameObject SpawnAt(Vector3 location, Quaternion rotation, bool include)
    {
        GameObject temp = Instantiate(toSpawn);

        numSpawned++;
        temp.name = spawnName + " " + numSpawned; 

        temp.transform.parent = null;//this.transform;
        temp.transform.SetPositionAndRotation(location, rotation);
        temp.SetActive(true);
        if (include)
            spawned.Add(temp);
        return temp;
    }

    private void Spawn()
    {
        SpawnAt(this.transform.position, this.transform.rotation, true);
    }

    void OnEnable()
    {
        if (action != null)
        {
            action.AddOnChangeListener(OnAction, inputSource);
        }
    }

    public void Launch()
    {
        if (launchReady)
        {
            launchReady = false;
            launchQueue[0].SetActive(false);
            Destroy(launchQueue[0]);
            launchQueue.RemoveAt(0);
            GameObject temp = SpawnAt(launchTransform.position, launchTransform.rotation, true);
            temp.GetComponent<Rigidbody>().velocity = Vector3.up * launchSpeed;

            if (launchAudio)
                launchAudio.Play();
        }
    }

    private void OnDisable()
    {
        if (action != null)
        {
            action.RemoveOnChangeListener(OnAction, inputSource);
        }
    }


    private void OnAction(SteamVR_Action_Boolean action_In, SteamVR_Input_Sources sources, bool newBool)
    {
        if (newBool && spawnWithTrigger)
        {
            Debug.Log("Trigger was pressed or released");

            Spawn();
        }
    }
}
