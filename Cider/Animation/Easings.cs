using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Animation
{
    public delegate double EasingFunction(double t);

    public static class Easings
    {
        public static double Linear(double t) => LinearEasing.Ease(t);

        public static double InQuad(double t) => InQuadEasing.Ease(t);
        public static double OutQuad(double t) => OutQuadEasing.Ease(t);
        public static double InOutQuad(double t) => InOutQuadEasing.Ease(t);

        public static double InCubic(double t) => InCubicEasing.Ease(t);
        public static double OutCubic(double t) => OutCubicEasing.Ease(t);
        public static double InOutCubic(double t) => InOutCubicEasing.Ease(t);

        public static double InQuart(double t) => InQuartEasing.Ease(t);
        public static double OutQuart(double t) => OutQuartEasing.Ease(t);
        public static double InOutQuart(double t) => InOutQuartEasing.Ease(t);

        public static double InQuint(double t) => InQuintEasing.Ease(t);
        public static double OutQuint(double t) => OutQuintEasing.Ease(t);
        public static double InOutQuint(double t) => InOutQuintEasing.Ease(t);

        public static double InSine(double t) => InSineEasing.Ease(t);
        public static double OutSine(double t) => OutSineEasing.Ease(t);
        public static double InOutSine(double t) => InOutSineEasing.Ease(t);

        public static double InExpo(double t) => InExpoEasing.Ease(t);
        public static double OutExpo(double t) => OutExpoEasing.Ease(t);
        public static double InOutExpo(double t) => InOutExpoEasing.Ease(t);

        public static double InCirc(double t) => InCircEasing.Ease(t);
        public static double OutCirc(double t) => OutCircEasing.Ease(t);
        public static double InOutCirc(double t) => InOutCircEasing.Ease(t);

        public static double InBack(double t) => InBackEasing.Ease(t);
        public static double OutBack(double t) => OutBackEasing.Ease(t);
        public static double InOutBack(double t) => InOutBackEasing.Ease(t);

        public static double InElastic(double t) => InElasticEasing.Ease(t);
        public static double OutElastic(double t) => OutElasticEasing.Ease(t);
        public static double InOutElastic(double t) => InOutElasticEasing.Ease(t);

        public static double InBounce(double t) => InBounceEasing.Ease(t);
        public static double OutBounce(double t) => OutBounceEasing.Ease(t);
        public static double InOutBounce(double t) => InOutBounceEasing.Ease(t);

        public static double SmoothStep(double t) => SmoothStepEasing.Ease(t);
    }

    public interface IEasing
    {
        static abstract double Ease(double t);
    }

    public class LinearEasing : IEasing
    {
        public static double Ease(double t) => t;
    }

    public class OutExpoEasing : IEasing
    {
        public static double Ease(double t) => t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);
        public static double Derivative(double t) => t == 1 ? 0 : 10 * Math.Pow(2, -10 * t) * Math.Log(2);
    }

    public class InQuadEasing : IEasing
    {
        public static double Ease(double t) => t * t;
    }

    public class OutQuadEasing : IEasing
    {
        public static double Ease(double t) => t * (2 - t);
    }

    public class InOutQuadEasing : IEasing
    {
        public static double Ease(double t) => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    public class InCubicEasing : IEasing
    {
        public static double Ease(double t) => t * t * t;
    }

    public class OutCubicEasing : IEasing
    {
        public static double Ease(double t) { t -= 1; return t * t * t + 1; }
    }

    public class InOutCubicEasing : IEasing
    {
        public static double Ease(double t) => t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }

    public class InQuartEasing : IEasing
    {
        public static double Ease(double t) => t * t * t * t;
    }

    public class OutQuartEasing : IEasing
    {
        public static double Ease(double t) { t -= 1; return 1 - t * t * t * t; }
    }

    public class InOutQuartEasing : IEasing
    {
        public static double Ease(double t) => t < 0.5 ? 8 * t * t * t * t : 1 - 8 * Math.Pow(t - 1, 4);
    }

    public class InQuintEasing : IEasing
    {
        public static double Ease(double t) => Math.Pow(t, 5);
    }

    public class OutQuintEasing : IEasing
    {
        public static double Ease(double t) { t -= 1; return 1 + Math.Pow(t, 5); }
    }

    public class InOutQuintEasing : IEasing
    {
        public static double Ease(double t) => t < 0.5 ? 16 * Math.Pow(t, 5) : 1 + 16 * Math.Pow(t - 1, 5);
    }

    public class InSineEasing : IEasing
    {
        public static double Ease(double t) => 1 - Math.Cos((t * Math.PI) / 2);
    }

    public class OutSineEasing : IEasing
    {
        public static double Ease(double t) => Math.Sin((t * Math.PI) / 2);
    }

    public class InOutSineEasing : IEasing
    {
        public static double Ease(double t) => -0.5 * (Math.Cos(Math.PI * t) - 1);
    }

    public class InExpoEasing : IEasing
    {
        public static double Ease(double t) => t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));
    }

    public class InOutExpoEasing : IEasing
    {
        public static double Ease(double t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            if (t < 0.5) return Math.Pow(2, 20 * t - 10) / 2;
            return (2 - Math.Pow(2, -20 * t + 10)) / 2;
        }
    }

    public class InCircEasing : IEasing
    {
        public static double Ease(double t) => 1 - Math.Sqrt(1 - t * t);
    }

    public class OutCircEasing : IEasing
    {
        public static double Ease(double t) { t -= 1; return Math.Sqrt(1 - t * t); }
    }

    public class InOutCircEasing : IEasing
    {
        public static double Ease(double t) => t < 0.5 ? (1 - Math.Sqrt(1 - 4 * t * t)) / 2 : (Math.Sqrt(1 - Math.Pow(2 * t - 2, 2)) + 1) / 2;
    }

    public class InBackEasing : IEasing
    {
        public static double Ease(double t)
        {
            const double s = 1.70158;
            return t * t * ((s + 1) * t - s);
        }
    }

    public class OutBackEasing : IEasing
    {
        public static double Ease(double t)
        {
            const double s = 1.70158;
            t -= 1; return t * t * ((s + 1) * t + s) + 1;
        }
    }

    public class InOutBackEasing : IEasing
    {
        public static double Ease(double t)
        {
            const double s = 1.70158 * 1.525;
            if (t < 0.5) return (t * 2) * (t * 2) * ((s + 1) * (t * 2) - s) / 2;
            return ((t * 2 - 2) * (t * 2 - 2) * ((s + 1) * (t * 2 - 2) + s) + 2) / 2;
        }
    }

    public class InElasticEasing : IEasing
    {
        public static double Ease(double t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            const double p = 0.3;
            return -Math.Pow(2, 10 * (t - 1)) * Math.Sin((t - 1 - p / 4) * (2 * Math.PI) / p);
        }
    }

    public class OutElasticEasing : IEasing
    {
        public static double Ease(double t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            const double p = 0.3;
            return Math.Pow(2, -10 * t) * Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
        }
    }

    public class InOutElasticEasing : IEasing
    {
        public static double Ease(double t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            const double p = 0.45;
            if (t < 0.5)
                return -0.5 * Math.Pow(2, 20 * t - 10) * Math.Sin((20 * t - 11.125) * (2 * Math.PI) / p);
            return Math.Pow(2, -20 * t + 10) * Math.Sin((20 * t - 11.125) * (2 * Math.PI) / p) * 0.5 + 1;
        }
    }

    internal static class Bounce
    {
        public static double Out(double t)
        {
            if (t < 1 / 2.75)
                return 7.5625 * t * t;
            if (t < 2 / 2.75)
            {
                t -= 1.5 / 2.75;
                return 7.5625 * t * t + 0.75;
            }
            if (t < 2.5 / 2.75)
            {
                t -= 2.25 / 2.75;
                return 7.5625 * t * t + 0.9375;
            }
            t -= 2.625 / 2.75;
            return 7.5625 * t * t + 0.984375;
        }
    }

    public class InBounceEasing : IEasing
    {
        public static double Ease(double t) => 1 - Bounce.Out(1 - t);
    }

    public class OutBounceEasing : IEasing
    {
        public static double Ease(double t) => Bounce.Out(t);
    }

    public class InOutBounceEasing : IEasing
    {
        public static double Ease(double t) => t < 0.5 ? (1 - Bounce.Out(1 - 2 * t)) * 0.5 : Bounce.Out(2 * t - 1) * 0.5 + 0.5;
    }

    public class SmoothStepEasing : IEasing
    {
        public static double Ease(double t) => t * t * (3 - 2 * t);
    }
}
