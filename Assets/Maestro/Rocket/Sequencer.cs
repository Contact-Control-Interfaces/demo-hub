using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Sequencer : MonoBehaviour {
	public int difficulty;
	public int taskTimeReceived;
	public List<GameObject> tasks;//list of task managers
	public static Sequencer instance;
	public enum States {queuing,waiting, interupted, failure};
	public States state;
	private IEnumerator taskToQueue;
	public GameObject taskQueued;

	public bool queuing;


	void Awake(){
		instance = this;

	}

	void Start () {
		state = States.waiting;

	}
	
	void Update(){
		switch(state){
			case States.queuing:
				if(!queuing){	
				AddToQueue(taskTimeReceived);
				queuing = true;
				}
				break;

			case States.waiting:
				break;

			case States.interupted:
				if(queuing){
					StopCoroutine(taskToQueue);
					queuing = false;
				}
			break;
			case States.failure:
				if(queuing){
					StopCoroutine(taskToQueue);
					queuing = false;
				}
			break;

		}

//		if(Input.GetKeyDown(KeyCode.Z)){
//			CommenceSequencing();
//
//		}
	} 

	public void CommenceSequencing(){
		taskToQueue = TaskQueue(UnityEngine.Random.Range(1,7));
		StartCoroutine(taskToQueue);

	}

	public void AddToQueue(int timeReceived){
		state = States.waiting;
		taskToQueue = TaskQueue(timeReceived+(12-difficulty));//gets time expected from last task triggered, adds leniency based on difficulty
		StartCoroutine(taskToQueue);
	}

	IEnumerator TaskQueue(int TaskTime){//task time received from add to queue, which comes from the last task triggered
		yield return new WaitForSeconds(TaskTime);
		queuing = false;
		if(tasks.Count!=0){
			int taskPicker = UnityEngine.Random.Range(0,(tasks.Count));
			taskQueued = tasks[taskPicker];//when there's multiple tasks this will pull from a range tasks
			tasks.RemoveAt(taskPicker);//removes task from list of possible tasks, only gets re-added when task is completed/failed
			taskQueued.SendMessage("listener");

		} else {
			taskToQueue = TaskQueue(10);
			StartCoroutine(taskToQueue);
		}
	} 
}
