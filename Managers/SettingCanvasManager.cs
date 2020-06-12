using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RogueNaraka.SingletonPattern;

namespace RogueNaraka.SettingSripts {
    public class SettingCanvasManager : MonoSingleton<SettingCanvasManager> {
        public TextMeshProUGUI LanguageButtonText;
        public TextMeshProUGUI TutorialResetButtonText;
        public TextMeshProUGUI ShakeButtonText;
        public TextMeshProUGUI MusicText;
        public TextMeshProUGUI SoundEffectText;
        public TextMeshProUGUI AdRemoveButtonText;

        void Start() {
            UpdateText();
        }

        public void UpdateText() {
            switch(GameManager.language) {
                case Language.English:
                    this.LanguageButtonText.text = "Language";
                    this.TutorialResetButtonText.text = "Tutorial\nReset";
                    if(CameraShake.instance.isShake) {
                        this.ShakeButtonText.text = "Screen\nShake Off";
                    } else {
                        this.ShakeButtonText.text = "Screen\nShake On";
                    }
                    this.MusicText.text = "Music";
                    this.SoundEffectText.text = "SFX";
                    this.AdRemoveButtonText.text = "AD Remove";
                break;
                case Language.Korean:
                    this.LanguageButtonText.text = "언어";
                    this.TutorialResetButtonText.text = "튜토리얼\n리셋";
                    if(CameraShake.instance.isShake) {
                        this.ShakeButtonText.text = "화면\n흔들림 Off";
                    } else {
                        this.ShakeButtonText.text = "화면\n흔들림 On";
                    }
                    this.MusicText.text = "음악";
                    this.SoundEffectText.text = "효과음";
                    this.AdRemoveButtonText.text = "광고 제거";
                break;
            }
        }
    }
}