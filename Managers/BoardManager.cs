﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueNaraka.UnitScripts;
using TMPro;
using RogueNaraka.NotificationScripts;
using RogueNaraka.TheBackendScripts;

public class BoardManager : MonoBehaviour {

    public static BoardManager instance = null;
    public GameObject unitPrefab;
    public GameObject bulletPrefab;
    public GameObject[] bossPrefabs;
    public GameObject effectPrefab;
    public GameObject soulPrefab;

    public Fade fade;

    public Unit player;
    public Unit boss;
    public Vector2 spawnPoint;//player spawn
    public Vector2 goalPoint;//next Stage
    public Vector2 bossPoint;
    public Vector2[] boardRange;//0:min, 1:max
    public ObjectPool unitPool;
    const int unitObjCount = 100;
    public ObjectPool bulletPool;
    const int bulletObjCount = 250;
    public ObjectPool effectPool;
    const int effectObjCount = 200;
    public ObjectPool soulPool;
    const int soulObjCount = 50;

    public List<Unit> enemies = new List<Unit>();
    public List<Unit> friendlies = new List<Unit>();
    public List<Unit> corpses = new List<Unit>();

    public TextMeshProUGUI stageTxt;
    /// <summary>
    /// PlayerPrefs value
    /// </summary>
    public int Stage { get { return PlayerPrefs.GetInt("stage"); } set { PlayerPrefs.SetInt("stage", value); } }
    /// <summary>
    /// Local value
    /// </summary>
    public int stage
    { get { return _stage; } }
    [SerializeField]
    private int _stage;
    public bool isReady;

    private bool isNotifiedHighScore;

    private void Awake()
    {
        instance = this;

        int count = unitPool.GetCount();
        if (count < unitObjCount)//unit Pooling
        {
            for (int i = count; i < unitObjCount; i++)
            {
                GameObject obj = SpawnObj(unitPrefab);
                unitPool.EnqueueObjectPool(obj);
            }
        }
        count = bulletPool.GetCount();
        if (count < bulletObjCount)//bullet Pooling
        {
            for (int i = count; i < bulletObjCount; i++)
            {
                GameObject obj = SpawnObj(bulletPrefab);
                bulletPool.EnqueueObjectPool(obj);
            }
        }
        count = effectPool.GetCount();
        if (count < effectObjCount)//effect Pooling
        {
            for (int i = count; i < effectObjCount; i++)
            {
                GameObject obj = SpawnObj(effectPrefab);
                effectPool.EnqueueObjectPool(obj);
            }
        }
        count = soulPool.GetCount();
        if (count < soulObjCount)//soul Pooling
        {
            for (int i = count; i < soulObjCount; i++)
            {
                GameObject obj = SpawnObj(soulPrefab);
                soulPool.EnqueueObjectPool(obj);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(goalPoint, 0.1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(bossPoint, 0.1f);
        if (boardRange.Length > 1)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(boardRange[0], 0.1f);
            Gizmos.DrawWireSphere(boardRange[1], 0.1f);
        }
    }

    public GameObject SpawnObj(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, unitPool.transform);
        return obj;
    }

    public void InitBoard()
    {
        Debug.Log("InitBoard");
        isReady = false;

        player.rigid.velocity = Vector3.zero;

        //GameManager.instance.SetPause(true);
        InitStage(_stage);
        
        fade.FadeIn();
    }

    /// <summary>
    /// stage = value;
    /// </summary>
    /// <param name="value"></param>
    public void SetStage(int value)
    {
        ClearStage();
        _stage = value;
    }

    public void StageUp()
    {
        _stage++;
    }

    public void Save()
    {
        PlayerPrefs.SetInt("stage", stage);
    }

    public void OnFadeIn()
    {
        isReady = true;
        if (!GameManager.instance.settingPnl.activeSelf)
            GameManager.instance.SetPause(false);
        if (!player.deathable.isDeath)
            GameManager.instance.SetPauseBtn(true);
        player.collider.enabled = true;

        TutorialManager.instance.StartTutorial(6);
    }

    //void RandomEnemy(int leftCost)
    //{
    //    List<UnitData> list = new List<UnitData>();
    //    int max = GameDatabase.instance.unitCosts[Unitcost]
    //    int cost = Random.Range(1, leftCost)
    //}


    private void InitStage(int stage)
    {
        //Debug.Log("SetStage(" + stage + ")");
        if(stage != 0 && stage % 30 == 0)
        {
            SpawnBoss(0);
            //AudioManager.instance.PlayMusic(AudioManager.instance.GetRandomBossMusic());
            
            StartCoroutine(StageTxtEffect(true));
            return;
        }

        if (RageManager.instance.isRage)
        {
            RageManager.instance.BigPnlOpen();
            AudioManager.instance.FadeInMusic(1);
        }

        if (AudioManager.instance.currentMainMusic.CompareTo(string.Empty)==0)
            AudioManager.instance.PlayMusic(AudioManager.instance.GetRandomMainMusic());
        else
            AudioManager.instance.PlayMusic(AudioManager.instance.currentMainMusic);

        float cost = GameDatabase.instance.stageCosts[(stage - 1) % 30];

        while(cost > 0)
            SpawnRandomEnemy(ref cost);

        StartCoroutine(StageTxtEffect());

        #if !UNITY_EDITOR
        if(stage == 1) {
            this.isNotifiedHighScore = false;
        } else if(!this.isNotifiedHighScore && TheBackendManager.Instance.ClearedStageForRank + 1 < stage) {
            this.isNotifiedHighScore = true;
            NotificationWindowManager.Instance.ActiveHighScoreBanner();
        }
        #endif
    }

    public void SpawnRandomEnemy(ref float cost)
    {
        UnitCost[] unitCosts = GameDatabase.instance.unitCosts;//코스트 별 유닛들

        List<int> able = new List<int>();//소환 가능한 유닛들
        int stage = _stage % 30;
        for (int i = 0; i < unitCosts.Length; i++)
        {
            if (unitCosts[i].cost <= cost)
            {
                for(int j = 0; j < unitCosts[i].unitId.Length; j++)
                {
                    UnitData temp = GameDatabase.instance.enemies[unitCosts[i].unitId[j]];
                    if (temp.stage <= stage && (temp.maxStage == 0 ? true : temp.maxStage >= stage))
                        able.Add(temp.id);
                }
            }
        }

        if(able.Count <= 0)
        {
            cost = 0;
            //Debug.Log("No More Spawnable Enemies");
            return;
        }

        int rnd = Random.Range(0, able.Count);
        int rndID = able[rnd];

        SpawnEnemy(rndID);
        cost -= GameDatabase.instance.enemies[rndID].cost;
    }

    public void SpawnPlayer(UnitData data)
    {
        if(player == null)
            player = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity).GetComponent<Unit>();
        player.Init(data);
        player.Spawn(spawnPoint);
        player.cachedTransform.SetParent(unitPool.transform);
        player.cachedTransform.SetSiblingIndex(0);
    }

    public void SpawnEnemy(int id)
    {
        //Debug.Log(id + " Enemies Spawned");
        Unit enemy = unitPool.DequeueObjectPool().GetComponent<Unit>();
        enemy.Init(GameDatabase.instance.enemies[id]);
        enemy.stat.dmg *= RageManager.instance.enemiesDmg;
        enemy.stat.hp *= RageManager.instance.enemiesHp;
        enemy.stat.currentHp *= RageManager.instance.enemiesHp;
        enemy.hpable.SetHp(enemy.stat.currentHp);

        enemy.Spawn(GetRandomSpawn());
    }

    public void SpawnBoss(int id)
    {
        //Debug.Log(id + " Boss Spawned");
        Unit boss = unitPool.DequeueObjectPool().GetComponent<Unit>();
        this.boss = boss;
        boss.Init(GameDatabase.instance.bosses[id]);
        boss.stat.dmg *= RageManager.instance.enemiesDmg;
        boss.stat.hp *= RageManager.instance.enemiesHp;
        boss.stat.currentHp *= RageManager.instance.enemiesHp;
        boss.hpable.SetHp(boss.stat.currentHp);
        boss.Spawn(bossPoint);
        Fillable.bossHp.gameObject.SetActive(true);
    }

    public Vector2 GetRandomSpawn()
    {
        Vector2 pos;
        do
        {
            float radius = PolyNav.PolyNav2D.current.radiusOffset;
            float x = Random.Range(boardRange[0].x + radius, boardRange[1].x - radius);
            float y = Random.Range(boardRange[0].y + radius, boardRange[1].y - radius);
            pos = new Vector2(x, y);
        } while (Vector2.Distance(pos, spawnPoint) < 1f);
        return pos;
    }

    private IEnumerator StageTxtEffect(bool isBoss = false)
    {
        Debug.Log("stageTxt");
        stageTxt.gameObject.SetActive(true);
        string text = isBoss ? string.Format("BOSS STAGE {0}", RageManager.instance.rageLevel + 1) : string.Format("STAGE {0}", _stage);
        stageTxt.text = string.Empty;
        Color color = Color.white;
        stageTxt.color = color;
        yield return null;
        //Appear
        for (int i = 0; i < text.Length; i++)
        {
            float t = 0.1f;
            while (t > 0)
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
            }
            stageTxt.text = string.Format("{0}{1}", stageTxt.text, text[i]);
        }
    }

    public void ClearStage()
    {
        for (int i = 0; i < bulletPool.transform.childCount; i++)
        {
            GameObject obj = bulletPool.transform.GetChild(i).gameObject;
            if (obj.activeSelf)
            {
                bulletPool.EnqueueObjectPool(obj);
            }
        }
        
        Debug.Log("Clear stage " + enemies.Count + " enemies");

        for (int i = 0; i < bulletPool.transform.childCount; i++)
        {
            GameObject obj = bulletPool.transform.GetChild(i).gameObject;
            if (obj.activeSelf)
            {
                bulletPool.EnqueueObjectPool(obj);
            }
        }

        for (int i = corpses.Count - 1; i >= 0; i--)
        {
            unitPool.EnqueueObjectPool(corpses[i].gameObject);
            corpses.RemoveAt(i);
        }

        for(int i = enemies.Count - 1; i >= 0; i--)
            unitPool.EnqueueObjectPool(enemies[i].gameObject);

        for (int i = friendlies.Count - 1; i >= 0; i--)
        {
            if (friendlies[i].Equals(player))
                continue;
            unitPool.EnqueueObjectPool(friendlies[i].gameObject);
            //friendlies[i].Kill();
        }
        boss = null;
        Fillable.bossHp.gameObject.SetActive(false);
        //this.clearedStage = this._stage;
    }

    public Vector3 ClampToBoard(Vector3 pos, float offsetX = 0, float offsetY = 0)
    {
        float x = Mathf.Clamp(pos.x, boardRange[0].x + offsetX, boardRange[1].x - offsetX);
        float y = Mathf.Clamp(pos.y, boardRange[0].y + offsetY, boardRange[1].y - offsetY);
        return new Vector3(x, y, pos.z);
    }

    public static bool IsMouseInBoard()
    {
        Vector3 mp = GameManager.instance.GetMousePosition() + new Vector2(0, Pointer.instance.offset);
        if (mp.y < BoardManager.instance.boardRange[0].y)
            return false;
        mp = BoardManager.instance.ClampToBoard(mp);
        return IsPointInBoard(mp, true);
    }

    public static bool IsPointInBoard(Vector2 point, bool isInclusive = false)
    {
        Vector3 min = BoardManager.instance.boardRange[0];
        Vector3 max = BoardManager.instance.boardRange[1];
        if (!isInclusive)
            return point.x > min.x && point.y > min.y && point.x < max.x && point.y < max.y;
        else
            return point.x >= min.x && point.y >= min.y && point.x <= max.x && point.y <= max.y;
    }
}
