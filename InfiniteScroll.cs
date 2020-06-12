﻿using RogueNaraka.RollScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScroll : MonoBehaviour {
    public RectTransform[] objs;
    public RectTransform layout;
    public Vector2 start;
    public float spacing;
    public float size;
    public float speed;
    public int first;
    public int rolling
    { get { return _rolling; } }
    private int _rolling;

    private void Awake()
    {
        Init();
    }

    [ContextMenu("Init")]
    public void Init()
    {
        first = 0;
        _rolling = 0;
        objs[0].localPosition = new Vector2(start.x + size/2, start.y);
        for(int i = 1; i < objs.Length; i++)
        {
            objs[i].localPosition = GetPosition(i);
        }
    }

    public Vector2 GetPosition(int index)
    {
        int pre = index - 1;
        if (pre == -1)
            pre = objs.Length - 1;
        return new Vector2(objs[pre].localPosition.x + spacing + size, start.y);
    }

    public IEnumerator MoveNext()
    {
        float distance = size + spacing;
        float moved = 0;
        _rolling++;
        while (true)
        {
            yield return null;
            float moving = !RollManager.instance.isPause ? speed * Time.unscaledDeltaTime * (Input.anyKey ? 4 : 1) : 0;
            bool isLast = false;
            if (moved + moving >= distance)
            {
                //Debug.Log("moved + moving >= distance : " + moved + " + " + moving + " >= " + distance);
                moving = distance - moved;
                isLast = true;
            }
            //Debug.Log("moved + moving >= distance : " + moved + " + " + moving + " >= " + distance);
            for (int i = 0; i < this.objs.Length; i++)
            {
                this.objs[i].localPosition = new Vector3(objs[i].localPosition.x - moving, this.objs[i].localPosition.y, objs[i].localPosition.z);
            }
            moved += moving;
            if (isLast)
                break;
        }
        _rolling--;
    }

    public void MoveFirstToEnd()
    {
        objs[first].localPosition = GetPosition(first);
        first = (++first) % objs.Length;
    }

    public void OutCheck()
    {
        if (objs[first].localPosition.x < start.x - size / 2)
        {
            MoveFirstToEnd();
            AudioManager.instance.PlaySFX("skillRoulette");
        }
    }

    private void Update()
    {
        OutCheck();
    }

    public void Spin(int count)
    {
        StartCoroutine(SpinCoroutine(count));
    }

    public IEnumerator SpinCoroutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(MoveNext());
            float t = 0.1f;
            do
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
            } while (t > 0);
        }
    }

    [ContextMenu("MoveOne")]
    public void MoveOne()
    {
        Spin(1);
    }
}
