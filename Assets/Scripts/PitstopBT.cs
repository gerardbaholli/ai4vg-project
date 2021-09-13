using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRBT;

public class PitstopBT : MonoBehaviour
{
	RaceStatus raceStatus;

	[SerializeField] float reactionTime = 0.05f;
	BehaviorTree AI;

	CarAIHandler carAIHandler;
	CarController carController;
	CarStatus carStatus;

	[SerializeField] Transform pitstopEntrance;
	[SerializeField] Transform pitstopExit;

	void Start()
	{
		raceStatus = FindObjectOfType<RaceStatus>();
		carAIHandler = GetComponent<CarAIHandler>();
		carController = GetComponent<CarController>();
		carStatus = GetComponent<CarStatus>();
	}

	public void StartBehaviourTree()
	{
		BTCondition c0 = new BTCondition(NeedToPit);

		BTCondition c1 = new BTCondition(IsOutsidePitlane);
		BTAction a0 = new BTAction(EnterPitlane);
		BTSequence s0 = new BTSequence(new IBTTask[] { c1, a0 });
		BTDecorator d0 = new BTDecoratorUntilFail(s0);

		BTCondition c2 = new BTCondition(IsNotInPitstopPosition);
		BTAction a4 = new BTAction(FollowPitlane);
		BTSequence s1 = new BTSequence(new IBTTask[] { c2, a4 });
		BTDecorator d1 = new BTDecoratorUntilFail(s1);

		BTAction a1 = new BTAction(TeleportToBox);
		BTAction a2 = new BTAction(ChangeTires);

		BTCondition c3 = new BTCondition(IsInsidePitlane);
		BTAction a3 = new BTAction(ExitPitlane);
		BTSequence s3 = new BTSequence(new IBTTask[] { c3, a3 });
		BTDecorator d2 = new BTDecoratorUntilFail(s3);

		BTSequence fs = new BTSequence(new IBTTask[] { c0, d0, d1, a1, a2, d2 });


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



	// ---------------- CONDITIONS ---------------- //
	public bool NeedToPit()
    {
		return carStatus.tiresCondition <= 20;
    }

	public bool IsInsidePitlane()
	{
		return carStatus.GetActualLocation() == CarStatus.ActualLocation.Pitlane;
	}

	public bool IsOutsidePitlane()
    {
		return !IsInsidePitlane();
    }

	public bool IsInPitstopPosition()
    {
		return IsInsidePitlane() & (gameObject.transform.position == carStatus.GetBoxPosition());
    }

	public bool IsNotInPitstopPosition()
    {
		return !IsInPitstopPosition();
    }




	// ---------------- ACTIONS ---------------- //
	public bool EnterPitlane()
    {
		Debug.Log("Enter Pitlane");
		carAIHandler.FollowRaceWaypoints();

		if (IsPitlaneAvailableToEnter())
        {
			carAIHandler.FollowPitlaneWaypoints();
		}

		return true;
	}

	public bool FollowPitlane()
    {
		carAIHandler.FollowPitlaneWaypoints();
		if ((carStatus.GetBoxPosition() - gameObject.transform.position).magnitude < 0.5f)
		{
			return false;
        }
		return true;
    }

	public bool TeleportToBox()
	{
		Debug.Log("Teleporting to box");
		Vector3 boxPosition = carStatus.GetBoxPosition();

		carAIHandler.StopCar();
		gameObject.transform.position = boxPosition;
		return true;
	}

	public bool ChangeTires()
	{
		Debug.Log("Changing tires: " + carStatus.GetTiresCondition());
		carStatus.PutNewTires();
		return true;
	}

    public bool ExitPitlane()
	{
		Debug.Log("Exit Pitlane");
		carAIHandler.FollowPitlaneWaypoints();
		if (IsOutOfPitlane())
        {
			carAIHandler.SetCurrentWaypoint(null);
			carAIHandler.FollowRaceWaypoints();
        }
		return true;
	}



	// ----------------------------------------- //
	private bool IsPitlaneAvailableToEnter()
    {
		return 
			((pitstopEntrance.position - gameObject.transform.position).magnitude < 2f) &
			Vector3.Dot(gameObject.transform.up.normalized, pitstopEntrance.position.normalized) > 0;
	}

	private bool IsOutOfPitlane()
    {
		return Vector3.Dot(gameObject.transform.up.normalized, pitstopExit.position.normalized) < 0;
    }

}
