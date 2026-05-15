using UnityEngine;

public class QLearningAgent : MonoBehaviour
{
    public float alpha = 0.1f;
    public float gamma = 0.95f;
    public float initialEpsilon = 1f;
    public float minEpsilon = 0.05f;
    public float epsilonDecay = 0.995f;

    private float[,] q;
    private int stateCount;
    private int actionCount;
    private float epsilon;

    public float Epsilon { get { return epsilon; } }
    public int StateCount { get { return stateCount; } }
    public int ActionCount { get { return actionCount; } }

    public void Initialize(int states, int actions)
    {
        stateCount = states;
        actionCount = actions;
        q = new float[stateCount, actionCount];
        epsilon = initialEpsilon;
    }

    public int SelectAction(int state)
    {
        if (Random.value < epsilon)
            return Random.Range(0, actionCount);
        return GreedyAction(state);
    }

    public int GreedyAction(int state)
    {
        int best = 0;
        float bestValue = q[state, 0];
        for (int a = 1; a < actionCount; a++)
        {
            if (q[state, a] > bestValue)
            {
                bestValue = q[state, a];
                best = a;
            }
        }
        return best;
    }

    public void UpdateQ(int state, int action, float reward, int nextState, bool done)
    {
        float target = reward;
        if (!done)
            target += gamma * MaxQ(nextState);
        q[state, action] += alpha * (target - q[state, action]);
    }

    public void DecayEpsilon()
    {
        epsilon = Mathf.Max(minEpsilon, epsilon * epsilonDecay);
    }

    public float[,] GetQTable()
    {
        return q;
    }

    public void SetQTable(float[,] table)
    {
        q = table;
        stateCount = table.GetLength(0);
        actionCount = table.GetLength(1);
    }

    private float MaxQ(int state)
    {
        float max = q[state, 0];
        for (int a = 1; a < actionCount; a++)
        {
            if (q[state, a] > max)
                max = q[state, a];
        }
        return max;
    }
}
