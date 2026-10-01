using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private string hitStateName = "HeartHit";

    private Animator animator;
    private Image image;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
    }

    public void Show()
    {
        StopAllCoroutines();
        if (animator != null)
        {
            animator.ResetTrigger("Hit");

            animator.Play("Idle", 0, 0f);
        }

        image.enabled = true;
    }



    public void PlayHitAndHide()
    {
        if (!image.enabled)
        {
            return;
        }
        StartCoroutine(HitSequence());

    }

    private IEnumerator HitSequence()
    {
        if (animator == null)
        {
            image.enabled = false;
            yield break;
        }

        animator.SetTrigger("Hit");

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(hitStateName));

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        image.enabled = false;
    }
}
