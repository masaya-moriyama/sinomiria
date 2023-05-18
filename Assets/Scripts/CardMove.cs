using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMove : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent;

    public bool isDoraggable;
    public int siblingIndex;

    public void OnBeginDrag(PointerEventData eventData)
    {
        siblingIndex = transform.GetSiblingIndex();

        CardController card = GetComponent<CardController>();

        if (!card.isPlayerCard)
        {
            // プレイヤーのカードではない場合
            isDoraggable = false;
        }
        else if (!GameManager.instance.IsDoraggable())
        {
            // ドラッグ操作が不許可の場合
            isDoraggable = false;
        }
        else if (!card.isFieldCard)
        {
            // 手札のカードの場合
            isDoraggable = true;
        }
        else
        {
            isDoraggable = false;
        }

        if (!isDoraggable)
        {
            return;
        }

        defaultParent = transform.parent;
        transform.SetParent(defaultParent.parent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDoraggable)
        {
            return;
        }

        transform.position = eventData.position;
        //マウスの座標を取得してスクリーン座標を更新
        // Vector3 thisPosition = Input.mousePosition;
        // //スクリーン座標→ワールド座標
        // Vector3 worldPosition = Camera.main.ScreenToWorldPoint(thisPosition);
        // worldPosition.z = 0f;

        // this.transform.position = worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDoraggable)
        {
            return;
        }

        transform.SetParent(defaultParent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.SetSiblingIndex(siblingIndex);
    }

    public IEnumerator MoveToField(Transform field)
    {
        transform.SetParent(defaultParent.parent);
        transform.DOMove(field.position, 0.25F);
        yield return new WaitForSeconds(0.25F);

        defaultParent = field;
        transform.SetParent(defaultParent);
    }
}
