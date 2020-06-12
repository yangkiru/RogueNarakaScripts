﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("CustomButton/OnMouseButton")]
public class OnMouseButton : Button, IDragHandler {

    [SerializeField]
    ButtonEvent _onDown = new ButtonEvent();
    [SerializeField]
    ButtonEvent _onUp = new ButtonEvent();
    [SerializeField]
    ButtonEvent _onEnter = new ButtonEvent();
    [SerializeField]
    ButtonEvent _onExit = new ButtonEvent();
    [SerializeField]
    ButtonEvent _onEnterUp = new ButtonEvent();
    [SerializeField]
    ButtonEvent _onDrag = new ButtonEvent();

    protected OnMouseButton() { }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
            return;
        base.OnPointerDown(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onDown.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable)
            return;
        base.OnPointerUp(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onUp.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable)
            return;
        base.OnPointerEnter(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onEnter.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable)
            return;
        base.OnPointerExit(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onExit.Invoke();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
            return;
        base.OnPointerClick(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onEnterUp.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactable)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onDrag.Invoke();
    }

    public ButtonEvent onDown
    {
        get { return _onDown; }
        set { _onDown = value; }
    }
    public ButtonEvent onUp
    {
        get { return _onUp; }
        set { _onUp = value; }
    }

    public ButtonEvent onEnter
    {
        get { return _onEnter; }
        set { _onEnter = value; }
    }

    public ButtonEvent onDrag
    {
        get { return _onDrag; }
        set { _onDrag = value; }
    }

    public ButtonEvent onExit
    {
        get { return _onExit; }
        set { _onExit = value; }
    }
}

[System.Serializable]
public class ButtonEvent : UnityEngine.Events.UnityEvent { }