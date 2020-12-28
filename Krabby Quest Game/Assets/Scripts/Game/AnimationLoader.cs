using Assets.Components.Extention;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationLoader : MonoBehaviour
{
    Animator UnityAnimator;
    Dictionary<string, int> SequenceNames = new Dictionary<string, int>();
    Queue<int> enqueuedAnimations = new Queue<int>();
    StinkyFile.Blitz3D.Animator BaseAnimator;

    private bool _shouldLoop = true;
    public bool ShouldLoop = true;

    private bool _canPlay = true;
    public bool CanPlay = true;

    public int PlayingSequence
    {
        get; private set;
    }

    public void Copy(AnimationLoader loader)
    {
        SetAnimator(loader.BaseAnimator);
    }

    void Start()
    {        
        PlayDefaultAnimation();
    }    

    /// <summary>
    /// Set by the AnimationCompiler allowing the object use information from the source B3D file
    /// </summary>
    public void SetAnimator(StinkyFile.Blitz3D.Animator animator)
    {
        BaseAnimator = animator;
        if (animator == null) return;
        var child = transform.Find(animator.Objects[0].Name, true);
        UnityAnimator = child?.GetComponent<Animator>();
        SequenceNames.Clear();        
        foreach (var seq in animator.Sequences)
            if (string.IsNullOrWhiteSpace(seq.Name)) continue;
            else SequenceNames.Add(seq.Name, seq.ID);               
    }

    void applySettings()
    {
        UnityAnimator.SetBool("CanPlay", CanPlay);
        UnityAnimator.SetBool("IsLooping", ShouldLoop);
        UnityAnimator.speed = .5f;
    }

    public void PlayDefaultAnimation() => PlayAnimationSequence(0);

    public void EnqueueSequence(int Sequence, bool? Looping = null) => PlayAnimationSequence(Sequence, Looping, true);
    public void EnqueueSequence(string Sequence, bool? Looping = null) => PlayAnimationSequence(Sequence, Looping, true);

    /// <summary>
    /// Plays the sequence with the ID specified immediately
    /// </summary>
    /// <param name="Sequence">The sequence ID to play (check the Animator for this)</param>
    /// <param name="Looping">Specifies whether to update the existing <see cref="ShouldLoop"/> variable. <c>null</c> for no change, <c>true</c>/<c>false</c> to update looping setting.</param>
    /// <param name="enqueue">True will wait until the current animation is completed</param>
    public void PlayAnimationSequence(int Sequence, bool? Looping = null, bool enqueue = false, bool forceAnim = false)
    {
        if (UnityAnimator == null) return;
        if (PlayingSequence == Sequence && !forceAnim) return;
        CanPlay = true;
        if (Looping.HasValue)
            ShouldLoop = Looping.Value;
        applySettings();
        if (!enqueue)
        {
            UnityAnimator.SetInteger("Sequence", Sequence);
            PlayingSequence = Sequence;
            UnityAnimator.Play("ANIM_PLAY");
        }
        else
            enqueuedAnimations.Enqueue(Sequence);
    }
    /// <summary>
    /// Plays the sequence with the name specified immediately - if the name is not blank and exists in the Blitz3D Animator tied to this AnimationLoader
    /// </summary>
    /// <param name="Name">The name of the sequence (check the AnimatorController for this)</param>
    /// <param name="Looping">Specifies whether to update the existing <see cref="ShouldLoop"/> variable. <c>null</c> for no change, <c>true</c>/<c>false</c> to update looping setting.</param>
    public void PlayAnimationSequence(string Name, bool? Looping = null, bool enqueue = false)
    {
        if (TryGetSequenceByName(Name, out int seq))
            PlayAnimationSequence(seq, Looping, enqueue);
        else Debug.LogWarning("Requested Animation Sequence: " + Name + " was not found.");
    }

    public bool TryGetSequenceByName(string Name, out int SequenceID) => SequenceNames.TryGetValue(Name, out SequenceID);

    // Update is called once per frame
    void Update()
    {
        if (UnityAnimator == null) return;
        if (_canPlay != CanPlay || _shouldLoop != ShouldLoop)
            applySettings();
        _canPlay = CanPlay;
        _shouldLoop = ShouldLoop;
        var currentAnimatorState = UnityAnimator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimatorState.IsName("ANIM_ENTER"))
            if (enqueuedAnimations.Any())
            {
                ShouldLoop = true;
                applySettings();
                UnityAnimator.SetInteger("Sequence", enqueuedAnimations.Peek());
                PlayingSequence = enqueuedAnimations.Dequeue();
            }
    }
}
