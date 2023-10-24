
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween : MonoBehaviour {
    public static float defaultDuration = 0.2f;
    public static float defaultAmount = 1.05f;

    public static void Bounce(Transform t, float amount) {
        Bounce(t, amount, defaultDuration);
    }

    public static void Bounce(Transform t) {
        Bounce(t, defaultAmount);
    }

    public static void Bounce(Transform t, float amount, float duration) {
        var prevScale = t.localScale.x;

        _ = t.DOScale(amount, duration).SetEase(Ease.OutBounce);
        _ = t.DOScale(prevScale, duration).SetEase(Ease.Linear).SetDelay(duration);
    }
}
