using System.IO;
using System.Text;
using UnityEngine;

public class Evaluator : MonoBehaviour
{
    public GridEnvironment environment;
    public QLearningAgent agent;
    public int evaluationEpisodes = 100;
    public int maxStepsPerEpisode = 200;
    public string qtableFileName = "qtable.json";
    public string resultsFolderName = "Results";

    public float lastSuccessRate;
    public float lastAvgSteps;
    public float lastAvgReward;

    public bool Evaluate()
    {
        if (environment == null)
            environment = FindFirstObjectByType<GridEnvironment>();
        if (agent == null)
            agent = FindFirstObjectByType<QLearningAgent>();

        string qpath = Path.Combine(Application.persistentDataPath, qtableFileName);
        if (!agent.LoadQTable(qpath))
        {
            Debug.LogError("Q-table not found at " + qpath);
            return false;
        }

        int successes = 0;
        float totalReward = 0f;
        int totalSteps = 0;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("episode,reward,steps,reached_goal");

        for (int e = 0; e < evaluationEpisodes; e++)
        {
            int state = environment.ResetEnvironment();
            float reward = 0f;
            int steps = 0;
            bool done = false;

            while (!done && steps < maxStepsPerEpisode)
            {
                int action = agent.GreedyAction(state);
                int next;
                float r;
                environment.Step(action, out next, out r, out done);
                state = next;
                reward += r;
                steps++;
            }

            bool reached = environment.AgentCell == environment.goalCell;
            if (reached)
                successes++;
            totalReward += reward;
            totalSteps += steps;
            sb.AppendLine(e + "," + reward.ToString("0.####") + "," + steps + "," + (reached ? 1 : 0));
        }

        lastSuccessRate = (float)successes / evaluationEpisodes * 100f;
        lastAvgSteps = (float)totalSteps / evaluationEpisodes;
        lastAvgReward = totalReward / evaluationEpisodes;

        sb.AppendLine();
        sb.AppendLine("success_rate," + lastSuccessRate.ToString("0.##"));
        sb.AppendLine("avg_steps," + lastAvgSteps.ToString("0.##"));
        sb.AppendLine("avg_reward," + lastAvgReward.ToString("0.####"));

        string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, resultsFolderName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "evaluation_results.csv"), sb.ToString());

        Debug.Log("Evaluation done. success " + lastSuccessRate.ToString("0.##") + "% avgSteps " + lastAvgSteps.ToString("0.##") + " avgReward " + lastAvgReward.ToString("0.####"));
        return true;
    }
}
