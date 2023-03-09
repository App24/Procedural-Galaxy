using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class CelestialUtilities
{
    // https://codepen.io/blaketarter/pen/EjxRMX?editors=1111
    public static Vector3 Xyy(float temperature)
    {
        float x = 0;
        float y = 0;
        float Y = 1;

        if (temperature >= 1667 && temperature <= 4000)
        {
            x = (-0.2661239f * (Mathf.Pow(10, 9) / Mathf.Pow(temperature, 3))) -
                (-0.2343580f * (Mathf.Pow(10, 6) / Mathf.Pow(temperature, 2))) +
                (0.8776956f * (Mathf.Pow(10, 3) / temperature)) + 0.179910f;
        }
        else if (temperature >= 4000 && temperature <= 25000)
        {
            x = (-3.0258469f * (Mathf.Pow(10, 9) / Mathf.Pow(temperature, 3))) +
                (2.1070379f * (Mathf.Pow(10, 6) / Mathf.Pow(temperature, 2))) +
                (0.2226347f * (Mathf.Pow(10, 3) / temperature)) + 0.240390f;
        }

        if (temperature >= 1667 && temperature <= 2222)
        {
            y = (-1.1063814f * Mathf.Pow(x, 3)) -
                (1.34811020f * Mathf.Pow(x, 2)) +
                (2.18555832f * x) -
                 0.20219683f;
        }
        else if (temperature >= 2222 && temperature <= 4000)
        {
            y = (-0.9549476f * Mathf.Pow(x, 3)) -
                (1.37418593f * Mathf.Pow(x, 2)) +
                (2.09137015f * x) -
                 0.16748867f;
        }
        else if (temperature >= 4000 && temperature <= 25000)
        {
            y = (3.0817580f * Mathf.Pow(x, 3)) -
                (5.87338670f * Mathf.Pow(x, 2)) +
                (3.75112997f * x) -
                 0.37001483f;
        }

        return new Vector3(x, y, Y);
    }

    public static Vector3 xyz(Vector3 xyy)
    {
        float X = 0;
        float Y = 0;
        float Z = 0;
        float x = xyy.x;
        float y = xyy.y;

        Y = xyy.z;
        X = (y == 0) ? 0 : (x * Y) / y;
        Z = (y == 0) ? 0 : ((1 - x - y) * Y) / y;

        return new Vector3(X, Y, Z);
    }

    public static Color rgb(Vector3 xyz)
    {
        float r = 0;
        float g = 0;
        float b = 0;

        float x = xyz.x;
        float y = xyz.y;
        float z = xyz.z;

        r = (3.2406f * x) +
      (-1.5372f * y) +
      (-0.4986f * z);

        g = (-0.9689f * x) +
            (1.8758f * y) +
            (0.0415f * z);

        b = (0.0557f * x) +
            (-0.2040f * y) +
            (1.0570f * z);

        r = (r > 1) ? 1 : r;
        g = (g > 1) ? 1 : g;
        b = (b > 1) ? 1 : b;

        return new Color(r, g, b);
    }

    public static Color TemperatureToColor(float temperature)
    {
        return rgb(xyz(Xyy(temperature)));
    }
}
