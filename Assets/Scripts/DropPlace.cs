using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
public enum TYPE
    {
        HAND,
        FIELD,
    }

    public TYPE type;

    public void OnDrop(PointerEventData eventData)
    {
        // 手札内でのドラッグドロップ、もしくは相手ターンの場合、何もしない。
        if (type == TYPE.HAND || !GameManager.instance.IsDoraggable())
        {
            Debug.Log("HAND");
            return;
        }

        CardController card = eventData.pointerDrag.GetComponent<CardController>();

        // 対象のカードが不在・対象が相手のカード・対象のドラッグが非許可、対象がフィールドのカード
        if (card == null || !card.isPlayerCard || card.isFieldCard)
        {
            return;
        }

        if (type == TYPE.FIELD)
        {
            Debug.Log("FIELD");
            card.move.defaultParent = this.transform;
            // ドロップ時の処理
            card.OnField();
        }
    }

}
