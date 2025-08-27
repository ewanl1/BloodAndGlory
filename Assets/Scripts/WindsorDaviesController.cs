using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class WindsorDaviesController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip greetingClip;
    public AudioClip pickupClip;
    public AudioClip prodClip;

    [Header("Timing")]
    public float greetingDelay = 3f;
    public float prodDelay = 30f;

    [Header("Animator")]
    public string talkTrigger1 = "Line1";
    public string talkTrigger2 = "Line2";
    public string talkTrigger3 = "Line3";

    private Animator anim;
    private AudioSource audioSrc;
    private bool _weaponPicked = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
    }

    void Start()
    {
        // 1) Kick off greeting after a delay
        StartCoroutine(PlayGreeting());
    }

    IEnumerator PlayGreeting()
    {
        yield return new WaitForSeconds(greetingDelay);
        TriggerLine(greetingClip, talkTrigger1);

        while (Time.time < prodDelay)
        {
            if (_weaponPicked)
                yield break;
            yield return null;
        }

        TriggerLine(prodClip, talkTrigger2);
    }

    /// <summary>
    /// Call this method from your pickup logic when the player grabs the weapon.
    /// </summary>
    public void OnPlayerPickedUpWeapon()
    {
        if (_weaponPicked == true) return;
        _weaponPicked = true;
        TriggerLine(pickupClip, talkTrigger3);
    }

    /// <summary>
    /// Plays the given clip and fires the Talk trigger so the Animator plays the talk animation.
    /// </summary>
    void TriggerLine(AudioClip clip, string trigger)
    {
        if (clip == null) return;
        audioSrc.Stop();

        // 2) Fire the talk animation
        anim.SetTrigger(trigger);
        // 3) Play the voice line
        audioSrc.PlayOneShot(clip);
    }
}
