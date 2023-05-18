using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject infoPanel;
    [SerializeField] Text infoPanelText;

    [SerializeField] Text playerHaveCoinCount;
    [SerializeField] Text enemyHaveCoinCount;

    [SerializeField] Text playerBetTrunText;
    [SerializeField] Text enemyBetTrunText;

    [SerializeField] GameObject leavePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] Text resultPanelText;

    // シングルトン化
    public static UIManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void RefreshViewHaveCoinText(int playerHaveCoin, int enemyHaveCoin)
    {
        this.playerHaveCoinCount.text = playerHaveCoin.ToString();
        this.enemyHaveCoinCount.text = enemyHaveCoin.ToString();
    }

    public void ShowBetTrunText(bool isPlayer)
    {
        playerBetTrunText.gameObject.SetActive(isPlayer);
        enemyBetTrunText.gameObject.SetActive(!isPlayer);
    }

    /// <summary>
    /// 行動情報の通知パネルを表示する。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IEnumerator ShowInfoPanel(string text)
    {
        Image image = infoPanel.gameObject.GetComponent<Image>();

        infoPanel.gameObject.SetActive(true);
        RectTransform rectTransform = infoPanel.gameObject.GetComponent<RectTransform>();
        rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + 200), 0.5f);
        DOVirtual.DelayedCall(1f, () =>
            {
                rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - 200), 0.5f);
            }
        );

        infoPanelText.text = text;

        yield return new WaitForSeconds(2);
        infoPanel.gameObject.SetActive(false);
    }

    public void ShowResultPanel(bool isPlayerWin)
    {
        resultPanelText.text = isPlayerWin ? "You Win" : "You Lose";
        resultPanel.SetActive(true);
    }

    public void ShowLeavePanel()
    {
        leavePanel.SetActive(true);
    }
}
