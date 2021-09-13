using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRBT;

public class CarFSM : MonoBehaviour
{
    RaceStatus raceStatus;

    CarAIHandler carAIHandler;
    CarController carController;
    CarStatus carStatus;

    private FSM fsm;
    public float fsmUpdateTimer = 0.015f;

    public bool start = false; // TO BE REMOVED
    public bool pitstop = false; // TO BE REMOVED

    // BEHAVIOUR TREES
    RaceBT raceBehaviorTree;
    PitstopBT pitstopBehaviorTree;

    private void Awake()
    {
        raceStatus = FindObjectOfType<RaceStatus>();
        carAIHandler = GetComponent<CarAIHandler>();
        carController = GetComponent<CarController>();
        carStatus = GetComponent<CarStatus>();
        raceBehaviorTree = GetComponent<RaceBT>();
        pitstopBehaviorTree = GetComponent<PitstopBT>();
    }

    private void Start()
    {
        FSMState stop = new FSMState();
        stop.stayActions.Add(Stop);

        FSMState race = new FSMState();
        race.stayActions.Add(Race);

        FSMState pitstop = new FSMState();
        pitstop.stayActions.Add(Pit);

        FSMTransition t1 = new FSMTransition(StartRace);
        FSMTransition t2 = new FSMTransition(StopRace);
        FSMTransition t3 = new FSMTransition(GoPit);
        FSMTransition t4 = new FSMTransition(ExitPit);

        stop.AddTransition(t1, race);
        race.AddTransition(t2, stop);
        race.AddTransition(t3, pitstop);
        pitstop.AddTransition(t4, race);

        fsm = new FSM(stop);

        StartCoroutine(Patrol());
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            fsm.Update();
            yield return new WaitForSeconds(fsmUpdateTimer);
        }
    }


    // --------------- CONDITIONS --------------- //
    public bool StartRace()
    {
        return raceStatus.start;
    }

    public bool StopRace()
    {
        return !StartRace();
    }

    public bool GoPit()
    {
        if (carStatus.GetTiresCondition() <= 20f)
        {
            return true;
        }
        return false;
    }

    public bool ExitPit()
    {
        if (carStatus.GetTiresCondition() >= 100f)
        {
            return true;
        }
        return false;
    }


    // --------------- ACTIONS --------------- //
    public void Stop()
    {
        Debug.Log("Stop");
        carAIHandler.StopCar();
    }

    public void Race()
    {
        Debug.Log("Race");
        raceBehaviorTree.StartBehaviourTree();
        carStatus.ConsumesTires();
    }

    public void Pit()
    {
        Debug.Log("Pit");
        pitstopBehaviorTree.StartBehaviourTree();
    }

}
