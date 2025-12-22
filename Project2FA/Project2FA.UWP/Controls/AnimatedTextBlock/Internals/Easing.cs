using System;

namespace Project2FA.UWP.Controls;

internal static partial class Easing
{
    public static float Lerp(float v0, float v1, float t)
    {
        return v0 + t * (v1 - v0);
    }

    public enum EasingFunction
    {
        Linear,
        QuadraticIn,
        QuadraticOut,
        QuadraticInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuarticIn,
        QuarticOut,
        QuarticInOut,
        QuinticIn,
        QuinticOut,
        QuinticInOut,
        SinusoidalIn,
        SinusoidalOut,
        SinusoidalInOut,
        ExponentialIn,
        ExponentialOut,
        ExponentialInOut,
        CircularIn,
        CircularOut,
        CircularInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        BackIn,
        BackOut,
        BackInOut,
    }

    public static float UpdateProgress(float k, EasingFunction easing = EasingFunction.Linear)
    {
        float r = 0;

        switch (easing)
        {
            case EasingFunction.Linear:
                r = Linear(k);
                break;
            case EasingFunction.QuadraticIn:
                r = QuadraticIn(k);
                break;
            case EasingFunction.QuadraticOut:
                r = QuadraticOut(k);
                break;
            case EasingFunction.QuadraticInOut:
                r = QuadraticInOut(k);
                break;
            case EasingFunction.CubicIn:
                r = CubicIn(k);
                break;
            case EasingFunction.CubicOut:
                r = CubicOut(k);
                break;
            case EasingFunction.CubicInOut:
                r = CubicInOut(k);
                break;
            case EasingFunction.QuarticIn:
                r = QuarticIn(k);
                break;
            case EasingFunction.QuarticOut:
                r = QuarticOut(k);
                break;
            case EasingFunction.QuarticInOut:
                r = QuarticInOut(k);
                break;
            case EasingFunction.QuinticIn:
                r = QuinticIn(k);
                break;
            case EasingFunction.QuinticOut:
                r = QuinticOut(k);
                break;
            case EasingFunction.QuinticInOut:
                r = QuinticInOut(k);
                break;
            case EasingFunction.SinusoidalIn:
                r = SinusoidalIn(k);
                break;
            case EasingFunction.SinusoidalOut:
                r = SinusoidalOut(k);
                break;
            case EasingFunction.SinusoidalInOut:
                r = SinusoidalInOut(k);
                break;
            case EasingFunction.ExponentialIn:
                r = ExponentialIn(k);
                break;
            case EasingFunction.ExponentialOut:
                r = ExponentialOut(k);
                break;
            case EasingFunction.ExponentialInOut:
                r = ExponentialInOut(k);
                break;
            case EasingFunction.CircularIn:
                r = CircularIn(k);
                break;
            case EasingFunction.CircularOut:
                r = CircularOut(k);
                break;
            case EasingFunction.CircularInOut:
                r = CircularInOut(k);
                break;
            case EasingFunction.ElasticIn:
                r = ElasticIn(k);
                break;
            case EasingFunction.ElasticOut:
                r = ElasticOut(k);
                break;
            case EasingFunction.ElasticInOut:
                r = ElasticInOut(k);
                break;
            case EasingFunction.BounceIn:
                r = BounceIn(k);
                break;
            case EasingFunction.BounceOut:
                r = BounceOut(k);
                break;
            case EasingFunction.BounceInOut:
                r = BounceInOut(k);
                break;
            case EasingFunction.BackIn:
                r = BackIn(k);
                break;
            case EasingFunction.BackOut:
                r = BackOut(k);
                break;
            case EasingFunction.BackInOut:
                r = BackInOut(k);
                break;
            default:
                r = Linear(k);
                break;
        }

        return r;
    }

    private static float Linear(float k)
    {
        return k;
    }

    private static float QuadraticIn(float k)
    {
        return k * k;
    }

    private static float QuadraticOut(float k)
    {
        return k * (2f - k);
    }

    private static float QuadraticInOut(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * k * k;
        return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
    }

    private static float CubicIn(float k)
    {
        return k * k * k;
    }

    private static float CubicOut(float k)
    {
        return 1f + ((k -= 1f) * k * k);
    }

    private static float CubicInOut(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * k * k * k;
        return 0.5f * ((k -= 2f) * k * k + 2f);
    }

    private static float QuarticIn(float k)
    {
        return k * k * k * k;
    }

    private static float QuarticOut(float k)
    {
        return 1f - ((k -= 1f) * k * k * k);
    }

    private static float QuarticInOut(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * k * k * k * k;
        return -0.5f * ((k -= 2f) * k * k * k - 2f);
    }

    private static float QuinticIn(float k)
    {
        return k * k * k * k * k;
    }

    private static float QuinticOut(float k)
    {
        return 1f + ((k -= 1f) * k * k * k * k);
    }

    private static float QuinticInOut(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * k * k * k * k * k;
        return 0.5f * ((k -= 2f) * k * k * k * k + 2f);
    }

    private static float SinusoidalIn(float k)
    {
        return 1f - (float)Math.Cos(k * Math.PI / 2f);
    }

    private static float SinusoidalOut(float k)
    {
        return (float)Math.Sin(k * Math.PI / 2f);
    }

    private static float SinusoidalInOut(float k)
    {
        return 0.5f * (1f - (float)Math.Cos(Math.PI * k));
    }

    private static float ExponentialIn(float k)
    {
        return k == 0f ? 0f : (float)Math.Pow(1024f, k - 1f);
    }

    private static float ExponentialOut(float k)
    {
        return Math.Abs(k - 1f) < 0.01 ? 1f : 1f - (float)Math.Pow(2f, -10f * k);
    }

    private static float ExponentialInOut(float k)
    {
        if (k == 0f) return 0f;
        if (Math.Abs(k - 1f) < 0.01) return 1f;
        if ((k *= 2f) < 1f) return 0.5f * (float)Math.Pow(1024f, k - 1f);
        return 0.5f * (-(float)Math.Pow(2f, -10f * (k - 1f)) + 2f);
    }

    private static float CircularIn(float k)
    {
        return 1f - (float)Math.Sqrt(1f - k * k);
    }

    private static float CircularOut(float k)
    {
        return (float)Math.Sqrt(1f - ((k -= 1f) * k));
    }

    private static float CircularInOut(float k)
    {
        if ((k *= 2f) < 1f) return -0.5f * ((float)Math.Sqrt(1f - k * k) - 1);
        return 0.5f * ((float)Math.Sqrt(1f - (k -= 2f) * k) + 1f);
    }

    private static float ElasticIn(float k)
    {
        if (k == 0) return 0;
        if (Math.Abs(k - 1) < 0.01) return 1;
        return -(float)Math.Pow(2f, 10f * (k -= 1f)) * (float)Math.Sin((k - 0.1f) * (2f * (float)Math.PI) / 0.4f);
    }

    private static float ElasticOut(float k)
    {
        if (k == 0) return 0;
        if (Math.Abs(k - 1) < 0.01) return 1;
        return (float)Math.Pow(2f, -10f * k) * (float)Math.Sin((k - 0.1f) * (2f * (float)Math.PI) / 0.4f) + 1f;
    }

    private static float ElasticInOut(float k)
    {
        if ((k *= 2f) < 1f)
            return -0.5f * (float)Math.Pow(2f, 10f * (k -= 1f)) *
                   (float)Math.Sin((k - 0.1f) * (2f * (float)Math.PI) / 0.4f);
        return (float)Math.Pow(2f, -10f * (k -= 1f)) * (float)Math.Sin((k - 0.1f) * (2f * (float)Math.PI) / 0.4f) *
            0.5f + 1f;
    }

    private static float BounceIn(float k)
    {
        return 1f - BounceOut(1f - k);
    }

    private static float BounceOut(float k)
    {
        if (k < (1f / 2.75f))
        {
            return 7.5625f * k * k;
        }
        else if (k < (2f / 2.75f))
        {
            return 7.5625f * (k -= (1.5f / 2.75f)) * k + 0.75f;
        }
        else if (k < (2.5f / 2.75f))
        {
            return 7.5625f * (k -= (2.25f / 2.75f)) * k + 0.9375f;
        }
        else
        {
            return 7.5625f * (k -= (2.625f / 2.75f)) * k + 0.984375f;
        }
    }

    private static float BounceInOut(float k)
    {
        if (k < 0.5f) return BounceIn(k * 2f) * 0.5f;
        return BounceOut(k * 2f - 1f) * 0.5f + 0.5f;
    }

    private static float BackIn(float k)
    {
        return k * k * ((1.70158f + 1f) * k - 1.70158f);
    }

    private static float BackOut(float k)
    {
        return (k -= 1f) * k * ((1.70158f + 1f) * k + 1.70158f) + 1f;
    }

    private static float BackInOut(float k)
    {
        if ((k *= 2f) < 1f) return 0.5f * (k * k * ((1.70158f + 1f) * k - 1.70158f));
        return 0.5f * ((k -= 2f) * k * ((1.70158f + 1f) * k + 1.70158f) + 2f);
    }
}
