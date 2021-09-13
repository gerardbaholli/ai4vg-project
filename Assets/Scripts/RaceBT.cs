using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRBT;

public class RaceBT : MonoBehaviour
{
	RaceStatus raceStatus;

	[SerializeField] float reactionTime = 0.1f;
	BehaviorTree AI;

	CarAIHandler carAIHandler;
	CarController carController;
	CarStatus carStatus;

	void Start()
	{
		raceStatus = FindObjectOfType<RaceStatus>();
		carAIHandler = GetComponent<CarAIHandler>();
		carController = GetComponent<CarController>();
		carStatus = GetComponent<CarStatus>();
	}

	public void StartBehaviourTree()
	{
		BTAction a0 = new BTAction(Race);

		BTCondition c1 = new BTCondition(IsSafetyCarDelivered);
		BTAction a1 = new BTAction(SlowSpeed);
		BTSequence s1 = new BTSequence(new IBTTask[] { c1, a1 });

		BTCondition c2 = new BTCondition(IsSafetyCarNotDelivered);
		BTAction a2 = new BTAction(SetOriginalSpeed);
		BTSequence s2 = new BTSequence(new IBTTask[] { c2, a2 });

		BTSelector o0 = new BTSelector(new IBTTask[] { s1 , s2 });

		BTSequence fs = new BTSequence(new IBTTask[] { a0, o0 });

		AI = new BehaviorTree(fs);

		StartCoroutine(Execute());
	}

	public IEnumerator Execute()
	{
		while (AI.Step())
		{
			yield return new WaitForSeconds(reactionTime);
		}
	}

	// ---------------- CONDITIONS -------------- //
	public bool IsSafetyCarDelivered()
    {
		return raceStatus.saferyCar;
    }

	public bool IsSafetyCarNotDelivered()
	{
		return !IsSafetyCarDelivered();
	}

	// ---------------- ACTIONS ---------------- //
	public bool Race()
	{
		carAIHandler.FollowRaceWaypoints();
		return true;
	}

	public bool SlowSpeed()
    {
		Debug.Log("Slow Speed");
		carController.SetSpeed(1f);
		return true;
    }

	public bool SetOriginalSpeed()
    {
		carController.SetSpeed(carController.maxSpeed);
		return true;
	}


}