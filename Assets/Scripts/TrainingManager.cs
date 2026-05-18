using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour
{
    public GridEnvironment environment;
    public QLearningAgent agent;
    public Transform agentVisual;
    public Text statsText;

    public int episodes = 2500;
    public int maxStepsPerEpisode = 200;
    public int stepsPerFrame = 30;
    public int logEvery = 200;
    public float agentHeight = 0.6f;
    public bool trainOnStart = true;
    public string saveFileName = "qtable.json";

    [System.NonSerialized] public List<float> rewardHistory = new List<float>();
    [System.NonSerialized] public List<int> stepHistory = new List<int>();
    [System.NonSerialized] public List<int> successHistory = new List<int>();

    private void Start()
    {
        if (trainOnStart)
            StartCoroutine(TrainRoutine());
    }

    public IEnumerator TrainRoutine()
    {
        if (environment == null)
            environment = FindFirstObjectByType<GridEnvironment>();
        if (agent == null)
            agent = FindFirstObjectByType<QLearningAgent>();

        agent.Initialize(environment.StateCount, environment.ActionCount);
        rewardHistory.Clear();
        stepHistory.Clear();
        successHistory.Clear();

        int budget = 0;

        for (int e = 0; e < episodes; e++)
        {
            int state = environment.ResetEnvironment();
            MoveAgentVisual(environment.AgentCell);
            float total = 0f;
            int steps = 0;
            bool done = false;
            bool reached = false;

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
                MoveAgentVisual(environment.AgentCell);

                budget++;
                if (budget >= stepsPerFrame)
                {
                    budget = 0;
                    UpdateStats(e + 1, total, steps, reached);
                    yield return null;
                }
            }

            reached = environment.AgentCell == environment.goalCell;
            rewardHistory.Add(total);
            stepHistory.Add(steps);
            successHistory.Add(reached ? 1 : 0);
            agent.DecayEpsilon();
            UpdateStats(e + 1, total, steps, reached);

            if ((e + 1) % logEvery == 0)
                LogProgress(e + 1);
        }

        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        agent.SaveQTable(path);
        LogProgress(episodes);
        Debug.Log("Training finished. Q-table saved to " + path);
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

    private void MoveAgentVisual(Vector2Int cell)
    {
        if (agentVisual == null)
            return;
        Vector3 p = environment.CellToWorld(cell);
        p.y = agentHeight;
        agentVisual.position = p;
    }

    private void UpdateStats(int episode, float reward, int steps, bool reached)
    {
        if (statsText == null)
            return;
        statsText.text =
            "Episode: " + episode + " / " + episodes + "\n" +
            "Reward: " + reward.ToString("0.00") + "\n" +
            "Steps: " + steps + "\n" +
            "Epsilon: " + agent.Epsilon.ToString("0.000") + "\n" +
            "Reached goal: " + (reached ? "yes" : "no");
    }

    private void LogProgress(int episode)
    {
        int n = rewardHistory.Count;
        if (n == 0)
            return;
        int w = Mathf.Min(100, n);
        float r = 0f, s = 0f, suc = 0f;
        for (int i = n - w; i < n; i++)
        {
            r += rewardHistory[i];
            s += stepHistory[i];
            suc += successHistory[i];
        }
        Debug.Log("episode " + episode +
            " | avg reward " + (r / w).ToString("0.000") +
            " | avg steps " + (s / w).ToString("0.0") +
            " | success " + (suc / w * 100f).ToString("0") + "%" +
            " | epsilon " + agent.Epsilon.ToString("0.000"));
    }
}
