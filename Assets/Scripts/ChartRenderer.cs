using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ChartRenderer
{
    public static void RenderRewardCurve(List<float> rewards, string path, int width, int height, int smoothWindow)
    {
        Texture2D tex = new Texture2D(width, height);
        Color bg = new Color(0.12f, 0.13f, 0.17f);
        Color axis = new Color(0.55f, 0.55f, 0.6f);
        Color raw = new Color(0.35f, 0.55f, 0.9f);
        Color avg = new Color(1f, 0.6f, 0.1f);

        Color[] fill = new Color[width * height];
        for (int i = 0; i < fill.Length; i++)
            fill[i] = bg;
        tex.SetPixels(fill);

        int left = 60;
        int right = 20;
        int top = 20;
        int bottom = 40;
        int plotW = width - left - right;
        int plotH = height - top - bottom;

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i] < min) min = rewards[i];
            if (rewards[i] > max) max = rewards[i];
        }
        if (rewards.Count == 0) { min = 0f; max = 1f; }
        if (Mathf.Approximately(min, max)) { min -= 1f; max += 1f; }

        DrawLine(tex, left, bottom, left, height - top, axis);
        DrawLine(tex, left, bottom, width - right, bottom, axis);

        PlotSeries(tex, rewards, left, bottom, plotW, plotH, min, max, raw);
        List<float> ma = MovingAverage(rewards, smoothWindow);
        PlotSeries(tex, ma, left, bottom, plotW, plotH, min, max, avg);

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void PlotSeries(Texture2D tex, List<float> data, int left, int bottom, int plotW, int plotH, float min, float max, Color c)
    {
        if (data.Count < 2)
            return;
        int prevX = 0;
        int prevY = 0;
        for (int i = 0; i < data.Count; i++)
        {
            int x = left + Mathf.RoundToInt((float)i / (data.Count - 1) * plotW);
            int y = bottom + Mathf.RoundToInt((data[i] - min) / (max - min) * plotH);
            if (i > 0)
                DrawLine(tex, prevX, prevY, x, y, c);
            prevX = x;
            prevY = y;
        }
    }

    private static List<float> MovingAverage(List<float> data, int window)
    {
        List<float> result = new List<float>(data.Count);
        Queue<float> q = new Queue<float>();
        float sum = 0f;
        for (int i = 0; i < data.Count; i++)
        {
            q.Enqueue(data[i]);
            sum += data[i];
            if (q.Count > window)
                sum -= q.Dequeue();
            result.Add(sum / q.Count);
        }
        return result;
    }

    private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color c)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            SetPixelSafe(tex, x0, y0, c);
            if (x0 == x1 && y0 == y1)
                break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private static void SetPixelSafe(Texture2D tex, int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
            return;
        tex.SetPixel(x, y, c);
    }
}
