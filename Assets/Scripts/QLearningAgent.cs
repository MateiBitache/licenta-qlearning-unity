using System.IO;
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

    public void SetEpsilon(float value)
    {
        epsilon = value;
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

    public void SaveQTable(string path)
    {
        QTableData data = new QTableData();
        data.states = stateCount;
        data.actions = actionCount;
        data.values = new float[stateCount * actionCount];
        for (int s = 0; s < stateCount; s++)
            for (int a = 0; a < actionCount; a++)
                data.values[s * actionCount + a] = q[s, a];
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    public bool LoadQTable(string path)
    {
        if (!File.Exists(path))
            return false;
        QTableData data = JsonUtility.FromJson<QTableData>(File.ReadAllText(path));
        stateCount = data.states;
        actionCount = data.actions;
        q = new float[stateCount, actionCount];
        for (int s = 0; s < stateCount; s++)
            for (int a = 0; a < actionCount; a++)
                q[s, a] = data.values[s * actionCount + a];
        return true;
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

    [System.Serializable]
    private class QTableData
    {
        public int states;
        public int actions;
        public float[] values;
    }
}
