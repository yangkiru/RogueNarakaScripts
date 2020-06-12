using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using RogueNaraka.SingletonPattern;

namespace RogueNaraka.TheBackendScripts {
    public partial class TheBackendManager : MonoSingleton<TheBackendManager> {
        private WWWForm formDataForGetRank;
        private string RequestURL = "https://roguenaraka.com/userRanking.php";

        private bool isLoadedRankData;
        public bool IsLoadedRankData { get { return this.isLoadedRankData; } }
        private bool isRefreshingPlayerRank;
        public bool IsRefreshingPlayerRank { get { return this.isRefreshingPlayerRank; } }
        private bool isRefreshingTopRankerData;
        public bool IsRefreshingTopRankerData { get { return this.isRefreshingTopRankerData; } }

        private int clearedStageForRank;
        public int ClearedStageForRank { get { return this.clearedStageForRank; } }

        private float topPercentToClearStageForRank = 100.0f;
        public float TopPercentToClearStageForRank { get { return this.topPercentToClearStageForRank; } }

        private int topRankingOfPlayer = -1;
        public int TopRankingOfPlayer { get { return this.topRankingOfPlayer; } }

        private List<RankerData> topRankerDataList = new List<RankerData>();
        public List<RankerData> TopRankerDataList { get { return this.topRankerDataList; } }

        private int totalRankedUserNum;
        public int TotalRankedUserNum { get { return this.totalRankedUserNum; } }

        public void UpdateRankData(int _clearedStage) {
            this.isRefreshingPlayerRank = true;
            if(this.clearedStageForRank >= _clearedStage) {
                StartCoroutine(RefreshRankDataCoroutine());
            } else {
                StartCoroutine(UploadRankDataCoroutine(_clearedStage));
            }
        }

        ///<summary>최대 10명</summary>
        public void RefreshTopRankerData(int _num) {
            this.isRefreshingTopRankerData = true;
            StartCoroutine(GetTopRankerData(_num));
        }

        private void StartForRank() {
            StartCoroutine(LoadRankDataCoroutine());
        }

        private void StartForRankWithoutGPSLogin() {
            this.userInDate = PlayerPrefs.GetString("UserInDateWithoutGPS");
            if(this.userInDate == "") {
                this.userInDate = DateTime.Now.ToString();
                PlayerPrefs.SetString("UserInDateWithoutGPS", this.userInDate);
            }
            this.isSavedUserInDate = true;
        }

        private void SavedFormDataForGetRank() {
            this.formDataForGetRank = new WWWForm();
            this.formDataForGetRank.AddField("userid", this.userInDate);
            this.formDataForGetRank.AddField("command", "get");
        }

        private IEnumerator LoadRankDataCoroutine() {
            yield return new WaitUntil(() => this.isLoginSuccess && this.isSavedUserInDate);

            SavedFormDataForGetRank();
            UnityWebRequest www = UnityWebRequest.Post(
                this.RequestURL, this.formDataForGetRank);
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
            } else {
                JsonData respondJson = JsonMapper.ToObject(www.downloadHandler.text);
                if(respondJson["result"].ToString() == "1") {
                    Debug.LogError("Error : Failed to Load RankData");
                } else {
                    if(respondJson["score"] != null) {
                        this.clearedStageForRank = int.Parse(respondJson["score"].ToString());
                        int total = int.Parse(respondJson["total"].ToString());
                        int ranking = total - int.Parse(respondJson["value"].ToString());
                        this.topRankingOfPlayer = ranking + 1;
                        this.topPercentToClearStageForRank = (float)ranking / total * 100.0f;
                        if(respondJson["nick"].ToString() == "Unknown") {
                            WWWForm formData = new WWWForm();
                            formData.AddField("userid", this.userInDate);
                            formData.AddField("command", "set");
                            formData.AddField("score", this.clearedStageForRank);
                            formData.AddField("nick", this.UserNickName);
                            UnityWebRequest wwwSet = UnityWebRequest.Post(this.RequestURL, formData);
                            yield return wwwSet.SendWebRequest();
                        }
                    }

                    this.isLoadedRankData = true;
                }
            }
        }

        private IEnumerator UploadRankDataCoroutine(int _clearedStage) {
            yield return new WaitUntil(() => this.isSavedUserInDate);

            WWWForm formData = new WWWForm();
            formData.AddField("userid", this.userInDate);
            formData.AddField("command", "set");
            formData.AddField("score", _clearedStage);
            formData.AddField("nick", this.UserNickName);
            UnityWebRequest www = UnityWebRequest.Post(
                this.RequestURL, formData);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
            } else {
                JsonData respondJson = JsonMapper.ToObject(www.downloadHandler.text);
                if(respondJson["result"].ToString() == "1") {
                    Debug.LogError("Error : Failed to Upload RankData");
                } else {
                    this.clearedStageForRank = _clearedStage;
                    int total = int.Parse(respondJson["total"].ToString());
                    int ranking = total - int.Parse(respondJson["value"].ToString());
                    this.topRankingOfPlayer = ranking + 1;
                    this.topPercentToClearStageForRank = (float)ranking / total * 100.0f;
                }
            }

            this.isRefreshingPlayerRank = false;
        }

        private IEnumerator RefreshRankDataCoroutine() {
            UnityWebRequest www = UnityWebRequest.Post(
                this.RequestURL, this.formDataForGetRank);
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
            } else {
                JsonData respondJson = JsonMapper.ToObject(www.downloadHandler.text);
                if(respondJson["result"].ToString() == "1") {
                    Debug.LogError("Error : Failed to Load RankData");
                } else if(respondJson["score"] != null) {
                    this.clearedStageForRank = int.Parse(respondJson["score"].ToString());
                    int total = int.Parse(respondJson["total"].ToString());
                    int ranking = total - int.Parse(respondJson["value"].ToString());
                    this.topRankingOfPlayer = ranking + 1;
                    this.topPercentToClearStageForRank = (float)ranking / total * 100.0f;
                }
            }

            this.isRefreshingPlayerRank = false;
        }

        private IEnumerator GetTopRankerData(int _num) {
            yield return new WaitUntil(() => this.isSavedUserInDate);
            this.topRankerDataList.Clear();

            WWWForm formData = new WWWForm();
            formData.AddField("command", "top");
            UnityWebRequest www = UnityWebRequest.Post(
                this.RequestURL, formData);
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
            } else {
                JsonData respondJson = JsonMapper.ToObject(www.downloadHandler.text);
                if(respondJson["result"].ToString() == "1") {
                    Debug.LogError("Error : Failed to Upload RankData");
                } else {
                    this.totalRankedUserNum = int.Parse(respondJson["total"].ToString());
                    for(int i = 0; i < _num || i < respondJson["value"].Count; i++) {
                        string rankerData = respondJson["value"][i].ToString();
                        int FirstSplitIdx;
                        int SecondSplitIdx;
                        //Get userID
                        FirstSplitIdx = rankerData.IndexOf("::");
                        string userId = rankerData.Substring(0, FirstSplitIdx);
                        //Get Score
                        SecondSplitIdx = rankerData.IndexOf("::", FirstSplitIdx + 2);
                        string score = rankerData.Substring(FirstSplitIdx + 2, SecondSplitIdx - (FirstSplitIdx + 2));
                        //Get NickName
                        string nickName = rankerData.Substring(SecondSplitIdx + 2);
                        
                        //Add In TopRankerList
                        this.TopRankerDataList.Add(new RankerData(userId, score, nickName));
                    }
                }
            }

            this.isRefreshingTopRankerData = false;
        }
    }

    public struct RankerData {
        public readonly string userId;
        public readonly int score;
        public readonly string nickName;

        public RankerData(string _id, int _score, string _nickName) {
            this.userId = _id;
            this.score = _score;
            this.nickName = _nickName;
        }

        public RankerData(string _id, string _score, string _nickName) {
            this.userId = _id;
            this.score = int.Parse(_score);
            this.nickName = _nickName;
        }
    }
}