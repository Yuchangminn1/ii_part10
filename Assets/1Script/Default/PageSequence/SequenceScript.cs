using System;
using System.Collections;
using Microsoft.SqlServer.Server;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

public abstract class SequenceScript : MonoBehaviour
{

    Graphic[] graphic;
    public Coroutine coroutine;

    public int currentIndex = 0;

    [SerializeField] protected float startDelay = -1f; // 한 글자씩 출력되는 간격
    protected WaitForSeconds _startDelay;

    [SerializeField] protected bool isTrigger = true;

    [SerializeField] protected bool originTrigger;

    [SerializeField] protected float nextDelayTime = -1f;


    WaitForFixedUpdate waitFixedUpdate = new WaitForFixedUpdate();

    WaitForSeconds waitNextDelay;

    public UnityEvent sequenceCallback;

    public VideoPlayer nextVedeoPlayer;


    protected bool isInitialize = false;



    protected void Awake()
    {
        originTrigger = isTrigger;
        if (nextDelayTime > 0)
        {
            waitNextDelay = new WaitForSeconds(nextDelayTime);
        }
        graphic = GetComponents<Graphic>();
        AwakeSetup();

        isInitialize = true;
    }

    protected IEnumerator WaitNextDelay()
    {
        if (waitNextDelay == null) yield break;
        yield return waitNextDelay;
    }



    public IEnumerator StartSequence()
    {
        //초기화 전도 대기
        while (!isInitialize)
        {
            yield return waitFixedUpdate;
        }
        isTrigger = originTrigger;
        //특정 트리거 필요하면 대기 
        while (!isTrigger)
        {
            //Debug.Log($"{this.name}Wait isTrigger");
            yield return waitFixedUpdate;
        }

        yield return coroutine = StartCoroutine(RunSequence());

        if (sequenceCallback != null) sequenceCallback.Invoke();

        yield return WaitNextDelay();

        EndPageSequence();
    }

    public void TriggerOn()
    {
        isTrigger = true;
    }

    protected abstract IEnumerator RunSequence();

    protected abstract void AwakeSetup();

    protected void NextSequence()
    {

        PageSequenceManager.Instance.NextSequence();
    }

    protected virtual void EndPageSequence()
    {
        if (nextVedeoPlayer != null) nextVedeoPlayer.Prepare();
        NextSequence();
    }
}