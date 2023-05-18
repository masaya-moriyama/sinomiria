using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    public Text numberText;

    public int number;

    public bool isPlayerCard;
    public bool isFieldCard;

    public CardMove move;

    /// <summary>
    ///  コントローラー起動時に子クラスを設定
    /// </summary>
    public void Awake()
    {
        move = GetComponent<CardMove>();
    }

    public void Init(bool isPlayer, bool isFieldCard, int setInt)
    {
        this.isPlayerCard = isPlayer;
        this.isFieldCard = isFieldCard;
        SetNumber(setInt);
        numberText.gameObject.SetActive(isPlayer);
    }

    public void SetNumber(int setInt)
    {
        numberText.text = setInt.ToString();
        number = setInt;
    }

    public void OnField()
    {
        if (this.isPlayerCard)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector3(160, -190, 0);
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector3(560, -190, 0);
        }

        GameManager.instance.playerPutCard = true;
        GameManager.instance.SetIsDoraggable(false);

        if (GameManager.instance.isCardPutPhase)
        {
            GameManager.instance.SendPlayCard(this.number);
        }
        else
        {
            GameManager.instance.SendPlaySwichCard(this.number);
        }
    }

}
