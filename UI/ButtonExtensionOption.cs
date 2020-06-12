using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExtensionOption : MonoBehaviour {
    public Button ThisButton;
    public Image[] ChildImage;
    public TextMeshProUGUI[] ChildText;
    public GameObject[] ChildObject;

    public void SetInteractable(bool _isInteractable) {
        this.ThisButton.interactable = _isInteractable;
        float alpha = this.ThisButton.colors.disabledColor.a;
        if(_isInteractable) {
            alpha = this.ThisButton.colors.normalColor.a;
        }
        for(int i = 0; i < this.ChildImage.Length; ++i) {
            this.ChildImage[i].color = new Color(this.ChildImage[i].color.r, this.ChildImage[i].color.g, this.ChildImage[i].color.b, alpha);
        }
        for(int i = 0; i < this.ChildText.Length; ++i) {
            this.ChildText[i].color = new Color(this.ChildText[i].color.r, this.ChildText[i].color.g, this.ChildText[i].color.b, alpha);
        }
        for(int i = 0; i < this.ChildObject.Length; ++i) {
            this.ChildObject[i].SetActive(!_isInteractable);
        }
    }
}
