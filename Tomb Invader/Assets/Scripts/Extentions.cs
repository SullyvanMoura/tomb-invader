using UnityEngine;

public static class Extentions
{

    public static void CrossFade(this Animator animator, CrossFadeSettings settings)
    {
        animator.CrossFade(
            settings.stateName,
            settings.transitionDuration,
            settings.layer,
            settings.timeOffset);
    }

    public static void CrossFadeInFixedTime(this Animator animator, CrossFadeSettings settings)
    {
        animator.CrossFadeInFixedTime(
            settings.stateName,
            settings.transitionDuration,
            settings.layer,
            settings.timeOffset);
    }
}
