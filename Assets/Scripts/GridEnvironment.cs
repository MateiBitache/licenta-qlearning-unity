using System.Collections.Generic;
using UnityEngine;

public class GridEnvironment : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public float cellSize = 1f;

    public Vector2Int startCell = new Vector2Int(0, 0);
    public Vector2Int goalCell = new Vector2Int(7, 7);
    public Vector2Int trapCell = new Vector2Int(5, 2);

    public List<Vector2Int> walls = new List<Vector2Int>
    {
        new Vector2Int(2, 1),
        new Vector2Int(2, 2),
        new Vector2Int(2, 3),
        new Vector2Int(2, 4),
        new Vector2Int(4, 4),
        new Vector2Int(5, 4),
        new Vector2Int(6, 4),
        new Vector2Int(4, 6)
    };

    public float goalReward = 1f;
    public float trapReward = -1f;
    public float stepCost = -0.01f;
    public float wallPenalty = -0.1f;

    public Color floorColor = new Color(0.82f, 0.82f, 0.82f);
    public Color wallColor = new Color(0.18f, 0.18f, 0.2f);
    public Color startColor = new Color(0.2f, 0.55f, 1f);
    public Color goalColor = new Color(0.25f, 0.85f, 0.35f);
    public Color trapColor = new Color(0.9f, 0.2f, 0.2f);

    private Vector2Int agentCell;

    public int StateCount { get { return width * height; } }
    public int ActionCount { get { return 4; } }
    public Vector2Int AgentCell { get { return agentCell; } }

    public int CellToState(Vector2Int c)
    {
        return c.y * width + c.x;
    }

    public Vector2Int StateToCell(int s)
    {
        return new Vector2Int(s % width, s / width);
    }

    public Vector3 CellToWorld(Vector2Int c)
    {
        return new Vector3(c.x * cellSize, 0.5f, c.y * cellSize);
    }

    public int ResetEnvironment()
    {
        agentCell = startCell;
        return CellToState(agentCell);
    }

    public void Step(int action, out int nextState, out float reward, out bool done)
    {
        Vector2Int target = agentCell;
        if (action == 0) target.y += 1;
        else if (action == 1) target.y -= 1;
        else if (action == 2) target.x -= 1;
        else if (action == 3) target.x += 1;

        if (!InBounds(target) || IsWall(target))
        {
            agentCell = agentCell;
            nextState = CellToState(agentCell);
            reward = wallPenalty + stepCost;
            done = false;
            return;
        }

        agentCell = target;

        if (agentCell == goalCell)
        {
            nextState = CellToState(agentCell);
            reward = goalReward;
            done = true;
            return;
        }

        if (agentCell == trapCell)
        {
            nextState = CellToState(agentCell);
            reward = trapReward;
            done = true;
            return;
        }

        nextState = CellToState(agentCell);
        reward = stepCost;
        done = false;
    }

    public bool InBounds(Vector2Int c)
    {
        return c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;
    }

    public bool IsWall(Vector2Int c)
    {
        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i] == c) return true;
        }
        return false;
    }

    public void BuildGrid()
    {
        ClearGrid();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Cell_" + x + "_" + y;
                tile.transform.SetParent(transform);
                tile.transform.localPosition = new Vector3(x * cellSize, 0f, y * cellSize);
                tile.transform.localScale = new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f);
                ApplyColor(tile, CellColor(cell));
            }
        }

        for (int i = 0; i < walls.Count; i++)
        {
            Vector2Int w = walls[i];
            if (!InBounds(w)) continue;
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall_" + w.x + "_" + w.y;
            wall.transform.SetParent(transform);
            wall.transform.localPosition = new Vector3(w.x * cellSize, 0.5f, w.y * cellSize);
            wall.transform.localScale = new Vector3(cellSize * 0.9f, 1f, cellSize * 0.9f);
            ApplyColor(wall, wallColor);
        }
    }

    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private Color CellColor(Vector2Int cell)
    {
        if (cell == startCell) return startColor;
        if (cell == goalCell) return goalColor;
        if (cell == trapCell) return trapColor;
        return floorColor;
    }

    private void ApplyColor(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material m = new Material(shader);
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        r.sharedMaterial = m;
    }
}
