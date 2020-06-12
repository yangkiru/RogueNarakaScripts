using System.Collections;
using System.Collections.Generic;
using BackEnd;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using RogueNaraka.SingletonPattern;
using RogueNaraka.NotificationScripts;
using RogueNaraka.PopUpScripts;

namespace RogueNaraka.TheBackendScripts {
    public partial class TheBackendManager : MonoSingleton<TheBackendManager> {
        public TextMeshProUGUI logoutBtnText;

        private bool isLoginSuccess;
        public bool IsLoginSuccess { get { return this.isLoginSuccess; } }

        private string userInDate;
        public string UserInDate { get { return this.userInDate; } }

        private bool isSavedUserInDate;
        public bool IsSavedUserInDate { get { return this.isSavedUserInDate; } }

        private bool isLogout;
        public bool IsLogout { get { return this.isLogout; } }

        private string userNickName = "";
        public string UserNickName { get { return this.userNickName; } }

        private bool isLogining;

        void StartForLogin() {
            this.isLogining = true;
            ActivatePlayGamesPlatform();
            if(!Backend.IsInitialized) {
                Backend.Initialize(() => {
                    if(Backend.IsInitialized) {
                        BackendInit();
                        AuthorizeFederationSync();
                    } else {
                        Debug.LogError("Backend Initialize Failed");
                    }
                });
            } else {
                BackendInit();
                AuthorizeFederationSync();
            }
            this.userNickName = PlayerPrefs.GetString("Player_NickName");
        }

        public void SetUserNickName(string _name) {
            this.userNickName = _name;
            PlayerPrefs.SetString("Player_NickName", _name);
        }

        private void ActivatePlayGamesPlatform() {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
                .Builder()
                .RequestServerAuthCode(false)
                .RequestIdToken()
                .RequestEmail()
                .Build();
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
        }

        private void AuthorizeFederationSync() {
            //안드로이드
            GPGSLogin();
            //
        }

        private void GPGSLogin() {
            if(Social.localUser.authenticated == true) {
                BackendReturnObject BRO = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");
                WorkAfterGPGSLogin();
            } else {
                Social.localUser.Authenticate((bool success) => {
                    if(success) {
                        BackendReturnObject BRO = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");
                        WorkAfterGPGSLogin();
                    } else {
                        //로그인 실패
                        Debug.LogError("GPGS Login Failed");
                        this.logoutBtnText.text = "Login";
                        this.isLogout = true;
                        PlayerPrefs.SetInt("IsLogout", 1);
                        ActiveLogoutPopup();
                        this.isLogining = false;
                    }
                });
            }
        }

        private void WorkAfterGPGSLogin() {
            //접속 체크
            //Debug.LogError(Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google));
            //푸시 설정
            Debug.Log("Logined!");
            this.isLoginSuccess = true;
            this.userInDate = Backend.BMember.GetUserInfo().GetReturnValuetoJSON()["row"]["inDate"].ToString();
            PlayerPrefs.SetString("UserInDateWithoutGPS", this.userInDate);
            this.isSavedUserInDate = true;
            Backend.Android.PutDeviceToken();
            this.isLogining = false;
        }

        private string GetTokens() {
            if(PlayGamesPlatform.Instance.localUser.authenticated) {
                string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
                return _IDtoken;
            } else {
                Debug.Log("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
                return null;
            }
        }

        private void ActiveLogoutPopup() {
            string popUpContext = "";
            switch(GameManager.language) {
                case Language.English:
                    popUpContext = "If you don't log in to Google Play Service, there may be restrictions on the use of some content.\n You can login again at any time in the Settings window.";
                break;
                case Language.Korean:
                    popUpContext = "구글 플레이 로그인을 하지 않을 경우 일부 컨텐츠의 이용에 제한이 있을 수 있습니다.\n설정 창에서 언제든지 다시 로그인 후 이용하실 수 있습니다.";
                break;
            } 
            PopUpManager.Instance.ActivateOneButtonPopUp(
                popUpContext,
                (OneButtonPopUpController _popUp) => { 
                    _popUp.DeactivatePopUp(); 
                    PopUpManager.Instance.DeactivateBackPanel();
                    GameManager.instance.SetPause(false);
                });
        }

        public IEnumerator ClickOnLoginButtonCoroutine() {
            StartForLogin();
            
            yield return new WaitUntil(() => !this.isLogining);

            if(this.isLoginSuccess) {
                this.logoutBtnText.text = "Logout";
                this.isLogout = false;
                PlayerPrefs.SetInt("IsLogout", 0);
                switch(GameManager.language) {
                    case Language.English:
                        NotificationWindowManager.Instance.EnqueueNotificationData("You're logged in.");
                    break;
                    case Language.Korean:
                        NotificationWindowManager.Instance.EnqueueNotificationData("로그인 했습니다.");
                    break;
                }
            }
        }

        ///<summary>Click Logout Button</summary>
        public void ClickOnLogoutButton() {
            if(!this.isLogout) {
                //Logout
                Backend.BMember.Logout();
                ((PlayGamesPlatform)Social.Active).SignOut();
                this.logoutBtnText.text = "Login";
                this.isLoginSuccess = false;
                this.isLogout = true;
                PlayerPrefs.SetInt("IsLogout", 1);
                switch(GameManager.language) {
                    case Language.English:
                        NotificationWindowManager.Instance.EnqueueNotificationData("You've logged out.");
                    break;
                    case Language.Korean:
                        NotificationWindowManager.Instance.EnqueueNotificationData("로그아웃 했습니다.");
                    break;
                }
            } else {
                //Login
                StartCoroutine(ClickOnLoginButtonCoroutine());
            }
        }
    }
}