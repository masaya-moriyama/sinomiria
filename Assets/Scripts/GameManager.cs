using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] CardController cardPrehub;
    [SerializeField] CoinModel coinSunPrehub;
    [SerializeField] CoinModel coinMoonPrehub;

    [SerializeField] Transform playerHandTransform;
    [SerializeField] Transform enemyHandTransform;

    [SerializeField] Transform putCardArea;
    [SerializeField] Transform coinArea;

    [SerializeField] GameObject passButton;
    [SerializeField] GameObject betSunCoinButton;
    [SerializeField] GameObject betMoonCoinButton;
    [SerializeField] GameObject changeCardButton;

    int playerHaveCoin = 15;
    int enemyHaveCoin = 15;

    bool isDoraggable;
    bool isSwitchCardByNawRound; // このラウンド中カードを入れ替えたか
    bool nowSwitchCard; // 現在カードの入れ替え中か

    public bool isCardPutPhase;

    public bool playerPutCard;
    public bool enemyPutCard;

    bool playerPrecedence;

    bool isBetPlayerTurn;

    bool playerJustBeforePass;
    bool enemyJustBeforePass;

    bool isJustBeforeSunCoin;

    bool isResult;

    // シングルトン化
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCardPutPhase && playerPutCard && enemyPutCard)
        {
            // 両者がカードを出したら、次フェーズへ
            StartCoroutine(UIManager.instance.ShowInfoPanel("Bet Phase"));

            isCardPutPhase = false;
            playerPutCard = false;
            enemyPutCard = false;
            isSwitchCardByNawRound = true;
            if (isBetPlayerTurn)
            {
                DOVirtual.DelayedCall(1.5f, () => SetAbleButton());
            }
            else
            {
                SetDisableButton();
            }
            UIManager.instance.ShowBetTrunText(isBetPlayerTurn);
        }

        if (!isCardPutPhase && nowSwitchCard && playerPutCard)
        {
            // FIXME: メソッド化
            isDoraggable = false;

            UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);
            isBetPlayerTurn = false;
            UIManager.instance.ShowBetTrunText(isBetPlayerTurn);
        }
    }

    void StartGame()
    {
        playerJustBeforePass = false;
        enemyJustBeforePass = false;
        playerPutCard = false;
        enemyPutCard = false;

        SetDisableButton();
        playerPrecedence = OnlineStatusManager.instance.IsOnlineHost;
        isBetPlayerTurn = playerPrecedence;
        isCardPutPhase = true;

        for (int i = 0; i <= 9; i++)
        {
            CardController entity = Instantiate(cardPrehub, playerHandTransform, false);
            entity.Init(true, false, i);
        }

        for (int i = 0; i <= 9; i++)
        {
            CardController entity = Instantiate(cardPrehub, enemyHandTransform, false);
            entity.Init(false, false, i);
        }

        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);
        isDoraggable = true;

        StartCoroutine(UIManager.instance.ShowInfoPanel("Card Put Phase"));
    }

    public void SetIsDoraggable(bool flg)
    {
        isDoraggable = flg;
    }

    /// <summary>
    /// ドラッグ操作を許可しているか
    /// </summary>
    /// <returns></returns>
    public bool IsDoraggable()
    {
        return isDoraggable;
    }

    /// <summary>
    /// パスボタンの押下
    /// </summary>
    public void OnPassButton()
    {
        playerJustBeforePass = true;
        SetDisableButton();

        // 対戦相手にパスの情報を送信する。 
        SendPass();

        EndBetTurn();
    }

    /// <summary>
    /// 太陽コインのベット
    /// </summary>
    public void OnPutSunCointButton()
    {
        playerHaveCoin--;
        playerJustBeforePass = false;
        SetDisableButton();

        // ベットしたコインを表示し、対戦相手にベット情報を送信する。 
        CoinModel coin = Instantiate(coinSunPrehub, coinArea, false);
        coin.Init(true, coinArea.transform.childCount + 1);
        SendBetCoin(true);

        EndBetTurn();
    }

    /// <summary>
    /// 月コインのベット
    /// </summary>
    public void OnPutMoonButton()
    {
        playerHaveCoin--;
        playerJustBeforePass = false;
        SetDisableButton();

        // ベットしたコインを表示し、対戦相手にベット情報を送信する。 
        CoinModel coin = Instantiate(coinMoonPrehub, coinArea, false);
        coin.Init(false, coinArea.transform.childCount + 1);
        SendBetCoin(false);

        EndBetTurn();
    }

    /// <summary>
    /// カードの入れ替え
    /// </summary>
    public void OnChangeCardButton()
    {
        playerHaveCoin --;
        enemyHaveCoin ++;
        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);

        isSwitchCardByNawRound = false;

        SetDisableButton();

        nowSwitchCard = true;
        // 場のカードを手札に戻す。
        CardController playerCard = GetPutPlayerCard();

        CardController card = Instantiate(cardPrehub, playerHandTransform, false);
        card.Init(true, false, playerCard.number);
        card.transform.SetSiblingIndex(playerCard.number);

        Destroy(playerCard.gameObject);

        isDoraggable = true;

        SendPlaySwichCardBefore();
    }

    /// <summary>
    /// 行動ボタンをアクティブへ変更する。
    /// </summary>
    public void SetAbleButton()
    {
        passButton.GetComponent<Button>().interactable = true;

        // 所持コインが0枚以上の場合のみ、パス以外の行動を実行可能とする。
        if (playerHaveCoin > 0)
        {
            betSunCoinButton.GetComponent<Button>().interactable = true;
            betMoonCoinButton.GetComponent<Button>().interactable = true;

            if (isSwitchCardByNawRound)
            {
                changeCardButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                changeCardButton.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            betSunCoinButton.GetComponent<Button>().interactable = false;
            betMoonCoinButton.GetComponent<Button>().interactable = false;
            changeCardButton.GetComponent<Button>().interactable = false;
        }
    }

    /// <summary>
    /// 行動ボタンを非アクティブへ変更する。
    /// </summary>
    public void SetDisableButton()
    {
        passButton.GetComponent<Button>().interactable = false;
        betSunCoinButton.GetComponent<Button>().interactable = false;
        betMoonCoinButton.GetComponent<Button>().interactable = false;
        changeCardButton.GetComponent<Button>().interactable = false;
    }

    /// <summary>
    /// ベット開始処理
    /// </summary>
    public void StartBetTurn()
    {
        StartCoroutine(UIManager.instance.ShowInfoPanel("You Bet Turn"));

        playerJustBeforePass = false;
        isBetPlayerTurn = true;
        DOVirtual.DelayedCall(1.5f, () => SetAbleButton());
        UIManager.instance.ShowBetTrunText(isBetPlayerTurn);
    }

    public void EndBetTurn()
    {
        // 所持コインの再描画
        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);
        // カードオープン条件が発生しているなら、カードをオープンしコインの変動を行う。
        if (CheckCardOpen())
        {
            ExecCoinChange();
            return;
        }
        isBetPlayerTurn = false;
        UIManager.instance.ShowBetTrunText(isBetPlayerTurn);
    }

    bool CheckCardOpen()
    {
        // チップが9枚の場合
        int coinCount = coinArea.transform.childCount;
        Debug.Log("コイン枚数チェック : " + coinCount);
        if (coinCount >= 9)
        {
            Debug.Log("チップが9枚によりShowDown");
            return true;
        }

        // 2手連続でパスが発生した場合
        Debug.Log(
            "双方パスチェック : " +
            (playerJustBeforePass ? "自分パス" : "自分非パス") +
            " : " +
            (enemyJustBeforePass ? "相手パス" : "相手非パス"));

        if (playerJustBeforePass && enemyJustBeforePass)
        {
            Debug.Log("2手連続でパスによりShowDown");
            return true;
        }

        // 月のベット直後にパスが発生した場合
        CoinModel[] fieldCoins = coinArea.GetComponentsInChildren<CoinModel>();
        bool isLastCoinToMoon = false;
        if (fieldCoins.Length > 0)
        {
            isLastCoinToMoon = !fieldCoins.Last().isSun;
        }

        // 最後のベットコインが月の状態で、以下の条件に該当する場合
        // 1. 自分のターンかつ、自分がパスした。
        // 2. 相手のターンかつ、相手がパスした。
        Debug.Log(
            "月コイン後パスチェック : " +
            (isLastCoinToMoon ? "最後コイン月" : "最後コイン太陽") +
            " : " +
            (isBetPlayerTurn ? "自分ターン状態" : "相手ターン状態"));

        if (isLastCoinToMoon && (
            (isBetPlayerTurn && playerJustBeforePass) ||
            (!isBetPlayerTurn && enemyJustBeforePass)))
        {
            Debug.Log("月の直後にパスによりShowDown");
            return true;
        }

        return false;
    }

    /// <summary>
    /// コインの交換を実施し、ラウンドを移行させる。
    /// </summary>
    void ExecCoinChange()
    {
        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);
        StartCoroutine(UIManager.instance.ShowInfoPanel("ShowDown"));
        SetDisableButton();

        CardController[] cardList = putCardArea.GetComponentsInChildren<CardController>();
        CardController playerCard = cardList.FirstOrDefault(x => x.isPlayerCard == true);
        CardController enemyCard = cardList.FirstOrDefault(x => x.isPlayerCard == false);

        CoinModel[] coinList = coinArea.GetComponentsInChildren<CoinModel>();

        // 手札を開ける
        ShowPutAreaCard(playerCard, enemyCard);

        // コインの変動を行う
        bool isWinByPlayer = IsPlayerWinToRound(playerCard, enemyCard, coinList.Length);
        DOVirtual.DelayedCall(2f, () => ExchangeCoin(isWinByPlayer, coinList));

        // ラウンドを終了し、カード提出フェーズに移行
        DOVirtual.DelayedCall(3f, () => SwitchiRound(playerCard, enemyCard, isWinByPlayer));
    }

    private void SwitchiRound(CardController playerCard, CardController enemyCard, bool isWinByPlayer)
    {
        // 9"を出した場合、9は捨て札にせず再度生成させる。
        if (playerCard.number == 9)
        {
            CardController entity = Instantiate(cardPrehub, playerHandTransform, false);
            entity.Init(true, false, 9);
        }

        if (enemyCard.number == 9)
        {
            CardController entity = Instantiate(cardPrehub, enemyHandTransform, false);
            entity.Init(false, false, 9);
        }

        SetDisableButton();
        isDoraggable = true;
        isBetPlayerTurn = isWinByPlayer;
        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);
        isCardPutPhase = true;
        playerPutCard = false;
        enemyPutCard = false;
        playerJustBeforePass = false;
        enemyJustBeforePass = false;

        // 提出されているカードオブジェクトを破棄する。
        Destroy(playerCard.gameObject);
        Destroy(enemyCard.gameObject);

        StartCoroutine(UIManager.instance.ShowInfoPanel("Card Put Phase"));
    }

    /// <summary>
    /// カード置き場にあるカードを公開する。
    /// </summary>
    void ShowPutAreaCard(CardController playerCard, CardController enemyuCard)
    {
        // TODO: 開閉演出の追加ß
        enemyuCard.numberText.gameObject.SetActive(true);
    }

    bool IsPlayerWinToRound(CardController playerCard, CardController enemyCard, int coinCount)
    {
        int compareInt = CompareTwoNumbersValue(playerCard.number, enemyCard.number, coinCount);

        if (compareInt == 1)
        {
            return true;
        }
        else if (compareInt == -1)
        {
            return false;
        }
        else
        {
            return !isBetPlayerTurn;
        }
    }

    void ExchangeCoin(bool isWinByPlayer, CoinModel[] coinList)
    {
        // 勝利: +2;
        int exchangeCoin = 2;

        // 勝利プレイヤーのコインが0枚の場合: +3
        if (isWinByPlayer && playerHaveCoin == 0)
        {
            exchangeCoin += 3;
        }
        else if (!isWinByPlayer && enemyHaveCoin == 0)
        {
            exchangeCoin += 3;
        }

        // 月の枚数分、ボーナス 
        int moonCoin = coinList.Where(x => x.isSun == false).ToArray().Length;
        exchangeCoin += moonCoin;

        if (isWinByPlayer)
        {
            exchangeCoin = enemyHaveCoin >= exchangeCoin ? exchangeCoin : enemyHaveCoin;

            playerHaveCoin += exchangeCoin;
            playerHaveCoin += coinList.Length;
            enemyHaveCoin -= exchangeCoin;
        }
        else
        {
            exchangeCoin = playerHaveCoin >= exchangeCoin ? exchangeCoin : playerHaveCoin;

            enemyHaveCoin += exchangeCoin;
            enemyHaveCoin += coinList.Length;
            playerHaveCoin -= exchangeCoin;
        }

        Debug.Log(
            "コイン変動 : { 対象 " +
            (isWinByPlayer ? "相手→自分 }" : "自分→相手 }") +
            "{ 交換コイン : " + exchangeCoin + " } " +
            "{ 場のコイン : " + coinList.Length + " } ");

        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);

        // コインの交換終了後、場のコインオブジェクトを消去
        int i = 0;
        foreach (var item in coinList)
        {
            i ++;
            item.image.DOFade(0f, 0.3f + (0.2f * i));
            DOVirtual.DelayedCall(0.5f + (0.2f * coinList.Length), () => Destroy(item.gameObject));
        }

        // どちらかのコインが0、もしくは所持カードが2枚以下の場合、ゲームを終了する。
        CardController[] cardList = playerHandTransform.GetComponentsInChildren<CardController>();
        if (
            (playerHaveCoin <= 0 || enemyHaveCoin <= 0) ||
            cardList.Length <= 2)
        {
            SetDisableButton();
            isDoraggable = false;
            UIManager.instance.ShowResultPanel(playerHaveCoin > enemyHaveCoin);
        }
    }

    /// <summary>
    /// 数値Aと数値Bのうち、どちらが基準値に近いがを比較する。
    /// </summary>
    /// <param name="target1">数値A</param>
    /// <param name="target2">数値B</param>
    /// <param name="base">基準値</param>
    /// <returns>
    /// 1: 数値Aが近い <br /> 
    /// -1: 数値Bが近い <br /> 
    /// 0: 同一
    /// </returns>
    public int CompareTwoNumbersValue(int target1, int target2, int basis)
    {
        // 1つ目の値と基準値の差を計算
        float difference1 = Mathf.Abs(basis - target1);

        // 2つ目の値と基準値の差を計算
        float difference2 = Mathf.Abs(basis - target2);

        // どちらが基準値に近いかを判別
        if (difference1 < difference2)
        {
            return 1;
        }
        else if (difference2 < difference1)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public void SwitchSceneToTitle()
    {
        SceneManager.LoadScene("Title");

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        OnlineStatusManager.instance.IsOnlineBattle = false;
    }

    CardController GetPutPlayerCard()
    {
        CardController[] cardList = putCardArea.GetComponentsInChildren<CardController>();
        return cardList.FirstOrDefault(x => x.isPlayerCard == true);
    }

    CardController GetPutEnemyCard()
    {
        CardController[] cardList = putCardArea.GetComponentsInChildren<CardController>();
        return cardList.FirstOrDefault(x => x.isPlayerCard == false);
    }

    // 以下オンライン戦

    public override void OnPlayerLeftRoom(Player player)
    {

        Debug.Log(OnlineStatusManager.instance.IsOnlineBattle);
        if (!OnlineStatusManager.instance.IsOnlineBattle)
        {
            return;
        }

        if (!isResult)
        {
            UIManager.instance.ShowLeavePanel();
        }
    }

    public void SendPlayCard(int number)
    {
        photonView.RPC(nameof(RPCOnRecievedPlayCard), RpcTarget.Others, number);
    }

    [PunRPC]
    void RPCOnRecievedPlayCard(int number)
    {
        CardController[] handCardList = GameManager.instance.enemyHandTransform.GetComponentsInChildren<CardController>();
        CardController card = Array.Find(handCardList, card => card.number == number);
        card.Init(false, true, number);

        card.transform.SetParent(putCardArea.transform, false);
        card.GetComponent<RectTransform>().anchoredPosition = new Vector3(560, -190, 0);

        enemyPutCard = true;

        Debug.Log("RPCOnRecievedCard : " + number);
    }

    public void SendPlaySwichCardBefore()
    {
        photonView.RPC(nameof(RPCOnRecievedPlaySwichCardBefore), RpcTarget.Others);
    }

    [PunRPC]
    void RPCOnRecievedPlaySwichCardBefore()
    {
        enemyHaveCoin--;
        playerHaveCoin++;

        CardController putCard = GetPutEnemyCard();

        CardController card = Instantiate(cardPrehub, enemyHandTransform, false);
        card.Init(false, false, putCard.number);
        card.transform.SetSiblingIndex(putCard.number);

        Debug.Log("RPCOnRecievedPlaySwichCard : " + putCard.number);

        Destroy(putCard.gameObject);
    }

    public void SendPlaySwichCard(int number)
    {
        photonView.RPC(nameof(RPCOnRecievedPlaySwichCard), RpcTarget.Others, number);
    }

    [PunRPC]
    void RPCOnRecievedPlaySwichCard(int number)
    {
        CardController[] handCardList = GameManager.instance.enemyHandTransform.GetComponentsInChildren<CardController>();
        CardController card = Array.Find(handCardList, card => card.number == number);

        Debug.Log(card.number);
        card.Init(false, true, number);

        card.transform.SetParent(putCardArea.transform, false);
        card.GetComponent<RectTransform>().anchoredPosition = new Vector3(560, -190, 0);


        isBetPlayerTurn = true;
        StartBetTurn();

        Debug.Log("RPCOnRecievedPlaySwichCard : " + number);
    }

    public void SendPass()
    {
        photonView.RPC(nameof(RPCOnRecievedPass), RpcTarget.Others);
    }

    [PunRPC]
    void RPCOnRecievedPass()
    {
        enemyJustBeforePass = true;

        // ベットフェーズ終了チェック
        if (CheckCardOpen())
        {
            ExecCoinChange();
            return;
        }

        isBetPlayerTurn = true;
        StartBetTurn();

        Debug.Log("RPCOnRecievedPass");
    }

    public void SendBetCoin(bool isSun)
    {
        photonView.RPC(nameof(RPCOnRecievedBetCoin), RpcTarget.Others, isSun);
    }

    [PunRPC]
    void RPCOnRecievedBetCoin(bool isSun)
    {
        playerJustBeforePass = false;
        enemyJustBeforePass = false;
        enemyHaveCoin--;
        UIManager.instance.RefreshViewHaveCoinText(playerHaveCoin, enemyHaveCoin);

        CoinModel coin = null;
        if (isSun)
        {
            coin = Instantiate(coinSunPrehub, coinArea, false);
        }
        else
        {
            coin = Instantiate(coinMoonPrehub, coinArea, false);
        }
        coin.Init(isSun, coinArea.transform.childCount + 1);

        // ベットフェーズ終了チェック
        if (CheckCardOpen())
        {
            ExecCoinChange();
            return;
        }

        isJustBeforeSunCoin = isSun;
        isBetPlayerTurn = true;
        StartBetTurn();
        Debug.Log("RPCOnRecievedBetCoin : " + (isSun ? "太陽コインをベット" : "月コインをベット"));
    }
}
