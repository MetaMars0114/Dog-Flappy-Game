﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public Player player;
    public GameObject pillarPrefab;
    public float spawnTime = 4f;

    [HideInInspector]
    public bool isGameStart = false;
    [HideInInspector]
    public bool isGameOver = false;
    Transform startButton, overButton, readyLabel, overLabel, tabLabel, titleLabel, rankBoard;
    Vector3 posStartButton, posOverButton, posReadyLabel, posOverLabel, posTabLabel, posTitleLabel, posRankBoard;

    int oldScore = 0;
    int score = 0;

    Text scoreLabel, resultScore, resultBestScore;
    Text oldScoreLabel;

    Image damageSprite;

    Camera cam;
    Vector3 camPosition, playerPosition;

    //public SpawnPool pool;

	void Start () {
        Application.targetFrameRate = 60;
        LoadOldScore();
        cam = Camera.main.GetComponent<Camera>();
        camPosition = cam.transform.position;
        resultScore = GameObject.Find("ResultScore").GetComponentInChildren<Text>();
        resultBestScore = GameObject.Find("ResultBestScore").GetComponentInChildren<Text>();
        scoreLabel = GameObject.Find("GameScore").GetComponentInChildren<Text>();
        oldScoreLabel = GameObject.Find("BestScore").GetComponentInChildren<Text>();

        damageSprite = GameObject.Find("DamageSprite").GetComponent<Image>();

        rankBoard = GameObject.Find("RankBoard").transform;
        titleLabel = GameObject.Find("GameTitle").transform;
        readyLabel = GameObject.Find("AreYouReady").transform;
        overLabel = GameObject.Find("GameOver").transform;
        tabLabel = GameObject.Find("TapToJump").transform;
        overButton = GameObject.Find("OverButton").transform;
        startButton = GameObject.Find("StartButton").transform;

        posRankBoard = rankBoard.localPosition;
        posTitleLabel = titleLabel.localPosition;
        posStartButton = startButton.localPosition;
        posOverButton = overButton.localPosition;
        posReadyLabel = readyLabel.localPosition;
        posOverLabel = overLabel.localPosition;
        posTabLabel = tabLabel.localPosition;

        rankBoard.localPosition = Vector3.right * 700f;
        titleLabel.localPosition = Vector3.up * 700f;
        readyLabel.localPosition = Vector3.left * 700f;
        startButton.localPosition = Vector3.down * 700f;
        overButton.localPosition = Vector3.down * 700f;
        overLabel.localPosition = Vector3.up * 700f;
        tabLabel.localPosition = Vector3.right * 700f;



        if (player) playerPosition = player.transform.position;

        ShowIntro();
        //GlobalVarManager.instance.SetScoreValue(0);
    }

    void ShowIntro()
    {
        isGameStart = false;
        isGameOver = false;
        if (!player) return;
        player.transform.position = playerPosition;
        //player.pos = 0;
        player.gameManager = this;
        player.OnRight();
        TweenMove(titleLabel, Vector3.up * 700f, posTitleLabel);
        TweenMove(readyLabel, Vector3.down * 700f, posReadyLabel);
        TweenMove(tabLabel, tabLabel.localPosition, posTabLabel);
        StartCoroutine(DelayActoin(0.2f, () =>
        {
            TweenMove(startButton, Vector3.down * 700f, posStartButton);
            scoreLabel.text = "0";
        }));

        DisplayScore();
    }

    void LoadOldScore()
    {
        oldScore = GlobalVarManager.instance.GetScoreValue();
    }

    void DisplayScore()
    {
        oldScoreLabel.text = oldScore.ToString();
    }


    void SaveNewScore()
    {
        LoadOldScore();
        if (oldScore < score)
        {
            GlobalVarManager.instance.SetScoreValue(score);
            oldScore = score;
        }
    }

    public void DoneShakeCam()
    {
        cam.transform.position = camPosition;
    }

    public void TweenMove(Transform tr, Vector3 pos1, Vector3 pos2)
    {
        tr.localPosition = pos1;
        TweenParms parms = new TweenParms().Prop("localPosition", pos2).Ease(EaseType.EaseOutQuad);
        HOTween.To(tr, 0.4f, parms);
    }

    public void ShakeCam()
    {
        cam.transform.position = camPosition + Vector3.right * 0.2f;
        TweenParms parms = new TweenParms().Prop("localPosition", camPosition).Ease(EaseType.EaseOutBounce).OnComplete(DoneShakeCam);
        HOTween.To(cam.transform, 0.4f, parms);
    }

    public void DoneDamageEffect()
    {
        damageSprite.color = new Color(1f, 1f, 1f, 0f);
        damageSprite.enabled = false;
    }

    public void DamageEffect()
    {
        damageSprite.color = new Color(1f, 1f, 1f, 0f);
        damageSprite.enabled = true;
        TweenParms parms = new TweenParms().Prop("color", new Color(1f,1f,1f,0.5f)).Ease(EaseType.Linear).Loops(2,LoopType.Restart).OnComplete(DoneDamageEffect);
        HOTween.To(damageSprite, 0.05f, parms);
    }

    public void AddScore()
    {
        score++;
        scoreLabel.text = score.ToString();
        player.PlayGoodSound();
    }

    public void StartGame()
    {
        if (isGameStart) return;
        StopCoroutine("SpawnPillar");
        //pool.DespawnAll();
        score = 0;
        startButton.localPosition = Vector3.up * 10000f;
        isGameStart = true;
        player.StartPlayer();
        StartCoroutine("SpawnPillar");
        TweenMove(titleLabel, posTitleLabel, Vector3.up * 700f);
        TweenMove(startButton, posStartButton, Vector3.down * 700f);
        StartCoroutine(DelayActoin(0.2f, () =>
        {
            TweenMove(readyLabel, posReadyLabel, Vector3.down * 700f);
        }));
        StartCoroutine(DelayActoin(0.5f, () =>
        {
            TweenMove(tabLabel, posTabLabel, Vector3.right * 700f);
        }));
    }

    public void ReloadGame()
    {
        Application.LoadLevel(Application.loadedLevel);

        if (player.transform.position.y > -2f) return;
        HideGameOver();
        StartCoroutine(DelayActoin(0.5f, () =>
        {
            ShowIntro();
        }));
    }

    public void StopGame()
    {
        if (!isGameOver)
        {
            StopCoroutine("SpawnPillar");
            SaveNewScore();
            ShowGameOver();
        }
        isGameOver = true;
    }


    void ShowGameOver()
    {
        resultScore.text = score.ToString();
        resultBestScore.text = oldScore.ToString();
        TweenMove(overLabel, Vector3.up * 700f, posOverLabel);
        TweenMove(overButton, Vector3.down * 700f, posOverButton);
        TweenMove(rankBoard, Vector3.right * 700f, posRankBoard);
    }

    void HideGameOver()
    {
        TweenMove(overLabel, posOverLabel, Vector3.up * 700f);
        TweenMove(overButton, posOverButton, Vector3.down * 700f);
        TweenMove(rankBoard, posRankBoard, Vector3.right * 700f);
        //pool.DespawnAll();
    }

    IEnumerator SpawnPillar()
    {
        yield return new WaitForSeconds(spawnTime);
        /*
        Transform tr = pool.Spawn(pillarPrefab.transform, Vector3.right * 5f + Vector3.down * Random.Range(1f, 7f), Quaternion.identity);
        Pillar pillar = tr.GetComponent<Pillar>();
        pillar.gameManager = this;
        pillar.ok = false;
        pillar.pool = pool;
         */
        GameObject go = Instantiate(pillarPrefab, Vector3.right * 5f + Vector3.down * Random.Range(1f, 7f), Quaternion.identity) as GameObject;
        Pillar pillar = go.GetComponent<Pillar>();
        pillar.gameManager = this;
        pillar.ok = false;
        //pillar.collider2D.enabled = true;
        if (isGameStart && !isGameOver) StartCoroutine("SpawnPillar");
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (isGameStart && !isGameOver)
        if (Input.GetMouseButtonDown(0))
        {
            player.OnJump();
        }
        //if (Input.GetMouseButtonDown(0)) if (isGameStart && isGameOver) Application.LoadLevel(Application.loadedLevel);
	}

    public void GoBuntGames()
    {
        Application.OpenURL("http://facebok.com/buntgames");
    }

    public void OpenLeaderBoard()
    {
#if UNITY_IPHONE
        GlobalVarManager.instance.ShowLeaderboard();
#else
        Application.OpenURL("http://youtube.com/textcube");
#endif
    }

    public IEnumerator DelayActoin(float dtime, System.Action callback)
    {
        yield return new WaitForSeconds(dtime);
        callback();
    }
}
