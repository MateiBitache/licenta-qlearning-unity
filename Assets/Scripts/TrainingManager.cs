using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    public GridEnvironment environment;
    public QLearningAgent agent;
    public int episodes = 2000;
    public int maxStepsPerEpisode = 200;
    public bool trainOnStart = false;

    [System.NonSerialized] public List<float> rewardHistory = new List<float>();
    [System.NonSerialized] public List<int> stepHistory = new List<int>();
    [System.NonSerialized] public List<int> successHistory = new List<int>();

    private void Start()
    {
        if (trainOnStart)
            Train();
    }

    public void Train()
    {
        if (environment == null)
            environment = FindFirstObjectByType<GridEnvironment>();
        if (agent == null)
            agent = FindFirstObjectByType<QLearningAgent>();

        agent.Initialize(environment.StateCount, environment.ActionCount);
        rewardHistory.Clear();
        stepHistory.Clear();
        successHistory.Clear();

        for (int e = 0; e < episodes; e++)
        {
            int state = environment.ResetEnvironment();
            float total = 0f;
            int steps = 0;
            bool done = false;

            while (!done && steps < maxStepsPerEpisode)
            {
                int action = agent.SelectAction(state);
                int nextState;
                float reward;
                environment.Step(action, out nextState, out reward, out done);
                agent.UpdateQ(state, action, reward, nextState, done);
                state = nextState;
                total += reward;
                steps++;
            }

            agent.DecayEpsilon();
            rewardHistory.Add(total);
            stepHistory.Add(steps);
            successHistory.Add(environment.AgentCell == environment.goalCell ? 1 : 0);
        }
    }
}
