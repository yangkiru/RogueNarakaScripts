using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RogueNaraka.SingletonPattern;
using RogueNaraka.TimeScripts;
using RogueNaraka.TheBackendScripts;
using RogueNaraka.PopUpScripts;

namespace RogueNaraka.TitleScripts {
    public class TitleManager : SingletonPattern.MonoSingleton<TitleManager> {
        public GameObject Grid;
        public GameObject Background;
        public GameObject Title;
        public GameObject Lobby;
        public TextMeshProUGUI ProcessText;
        public TextMeshProUGUI StartButtonText;
        public TextMeshProUGUI ShopButtonText;
        public TextMeshProUGUI InputContext;
        public TextMeshProUGUI InputConfirmButtonText;

        [Header("About Input Nick Name")]
        public GameObject InputName;
        public TMP_InputField InputField;

        private bool isAbleToGoMain;
        private bool isFirstStart = true;

        private void Start() {
            UpdateText();
            if(PlayerPrefs.GetString("stat") == "") {
                GameManager.instance.ResetData();
            }
        
            StartCoroutine(CheckToCanGoMain());
        }

        public void ClickOnTitleScreen() {
            if(!isAbleToGoMain) {
                return;
            }

            this.Title.GetComponent<Fade>().FadeOut();
        }

        public void ClickOnGameStartButton() {
            AdMobManager.instance.RequestBanner();
            AudioManager.instance.PlaySFX("gameStart");
            this.Lobby.GetComponent<Fade>().FadeOut();
            this.Grid.SetActive(true);
        }

        public void ClickOnShopButton() {
            SoulShopManager.instance.SetSoulShop(true);
        }

        public void ClickOnComfirmNickNameButton() {
            //CheckNickName
            if(this.InputField.text.Contains(":")) {
                string popUpContext = "";
                switch(GameManager.language) {
                    case Language.English:
                        popUpContext = "Nickname cannot contain ':'. Please re-enter.";
                    break;
                    case Language.Korean:
                        popUpContext = "닉네임에 ':' 가 포함될 수 없습니다. 다시 입력해주세요.";
                    break;
                } 
                PopUpManager.Instance.ActivateOneButtonPopUp(
                    popUpContext,
                    (OneButtonPopUpController _popUp) => { 
                        _popUp.DeactivatePopUp(); 
                        PopUpManager.Instance.DeactivateBackPanel();
                    });
                return;
            }
            if(this.InputField.text == "") {
                string popUpContext = "";
                switch(GameManager.language) {
                    case Language.English:
                        popUpContext = "Please enter Nickname.";
                    break;
                    case Language.Korean:
                        popUpContext = "닉네임을 입력해주세요.";
                    break;
                } 
                PopUpManager.Instance.ActivateOneButtonPopUp(
                    popUpContext,
                    (OneButtonPopUpController _popUp) => { 
                        _popUp.DeactivatePopUp(); 
                        PopUpManager.Instance.DeactivateBackPanel();
                    });
                return;
            }
            //
            TheBackendManager.Instance.SetUserNickName(this.InputField.text);
            this.InputName.SetActive(false);
        }

        private IEnumerator CheckToCanGoMain() {
            WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
            this.ProcessText.text = "Logining...";
            float time = 0.0f;

            yield return waitForFixedUpdate;
            #if !UNITY_EDITOR
            while(true) {
                time += TimeManager.Instance.FixedDeltaTime;
                if(TheBackendManager.Instance.IsSavedUserInDate) {
                    break;
                } else if(time > 20.0f) {
                    string context = "";
                    switch(GameManager.language) {
                        case Language.English:
                            context = "You are in Offline mode because of bad network connection.";
                        break;
                        case Language.Korean:
                            context = "연결이 원할하지 않아 오프라인 모드로 진행합니다.";
                        break;
                    }
                    PopUpManager.Instance.ActivateOneButtonPopUp(context,
                        (OneButtonPopUpController _popUp) => { 
                            _popUp.DeactivatePopUp(); 
                            PopUpManager.Instance.DeactivateBackPanel();
                            GameManager.instance.SetPause(false);
                        });
                    TheBackendManager.Instance.gameObject.SetActive(false);
                    break;
                }
                yield return waitForFixedUpdate;
            }
            #endif

            if(TheBackendManager.Instance.UserNickName == "") {
                this.InputName.gameObject.SetActive(true);
            }

            this.ProcessText.text = "Touch To Start";
            this.isAbleToGoMain = true;
        }

        public void UpdateText() {
            switch(GameManager.language) {
                case Language.English:
                    this.StartButtonText.text = "Start";
                    this.ShopButtonText.text = "Shop";
                    this.InputContext.text = "Please enter a nickname.";
                    this.InputConfirmButtonText.text = "Confirm";
                break;
                case Language.Korean:
                    this.StartButtonText.text = "시작하기";
                    this.ShopButtonText.text = "상점";
                    this.InputContext.text = "닉네임을 입력해주세요.";
                    this.InputConfirmButtonText.text = "확인";
                break;
            }
        }
    }
}