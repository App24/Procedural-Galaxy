using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class UnityUtilities
{
    public static Gradient Lerp(Gradient a, Gradient b, float t)
    {
        return Lerp(a, b, t, false, false);
    }

    public static Gradient LerpNoAlpha(Gradient a, Gradient b, float t)
    {
        return Lerp(a, b, t, true, false);
    }

    public static Gradient LerpNoColor(Gradient a, Gradient b, float t)
    {
        return Lerp(a, b, t, false, true);
    }

    static Gradient Lerp(Gradient a, Gradient b, float t, bool noAlpha, bool noColor)
    {
        //list of all the unique key times
        var keysTimes = new List<float>();

        if (!noColor)
        {
            for (int i = 0; i < a.colorKeys.Length; i++)
            {
                float k = a.colorKeys[i].time;
                if (!keysTimes.Contains(k))
                    keysTimes.Add(k);
            }

            for (int i = 0; i < b.colorKeys.Length; i++)
            {
                float k = b.colorKeys[i].time;
                if (!keysTimes.Contains(k))
                    keysTimes.Add(k);
            }
        }

        if (!noAlpha)
        {
            for (int i = 0; i < a.alphaKeys.Length; i++)
            {
                float k = a.alphaKeys[i].time;
                if (!keysTimes.Contains(k))
                    keysTimes.Add(k);
            }

            for (int i = 0; i < b.alphaKeys.Length; i++)
            {
                float k = b.alphaKeys[i].time;
                if (!keysTimes.Contains(k))
                    keysTimes.Add(k);
            }
        }

        GradientColorKey[] clrs = new GradientColorKey[keysTimes.Count];
        GradientAlphaKey[] alphas = new GradientAlphaKey[keysTimes.Count];

        //Pick colors of both gradients at key times and lerp them
        for (int i = 0; i < keysTimes.Count; i++)
        {
            float key = keysTimes[i];
            var clr = Color.Lerp(a.Evaluate(key), b.Evaluate(key), t);
            clrs[i] = new GradientColorKey(clr, key);
            alphas[i] = new GradientAlphaKey(clr.a, key);
        }

        var g = new Gradient();
        g.SetKeys(clrs, alphas);

        return g;
    }

    public static Gradient CloneGradient(this Gradient gradient)
    {
        var keysTimes = new List<float>();
        for (int i = 0; i < gradient.colorKeys.Length; i++)
        {
            float k = gradient.colorKeys[i].time;
            if (!keysTimes.Contains(k))
                keysTimes.Add(k);
        }

        for (int i = 0; i < gradient.alphaKeys.Length; i++)
        {
            float k = gradient.alphaKeys[i].time;
            if (!keysTimes.Contains(k))
                keysTimes.Add(k);
        }

        GradientColorKey[] clrs = new GradientColorKey[keysTimes.Count];
        GradientAlphaKey[] alphas = new GradientAlphaKey[keysTimes.Count];
        for (int i = 0; i < keysTimes.Count; i++)
        {
            float key = keysTimes[i];
            var clr = gradient.Evaluate(key);
            clrs[i] = new GradientColorKey(clr, key);
            alphas[i] = new GradientAlphaKey(clr.a, key);
        }

        var g = new Gradient();
        g.SetKeys(clrs, alphas);

        return g;
    }
}
