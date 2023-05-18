using UnityEngine;
using UnityEngine.UI;

public class CoinModel : MonoBehaviour
{
    // 太陽コインかどうか
    public bool isSun;

    // 何枚目のコインか
    public int countFieldCoin;

    public Image image;
    
    public void Init(bool isSun, int countFieldCoin)
    {
        this.isSun = isSun;
        this.countFieldCoin = countFieldCoin;
        this.image = GetComponent<Image>();
    }
}
