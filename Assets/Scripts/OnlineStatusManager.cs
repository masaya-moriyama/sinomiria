using UnityEngine;

public class OnlineStatusManager : MonoBehaviour
{
    // オンライン戦かどうか
    public bool IsOnlineBattle { get; set; }
    // オンラインのホストかどうか
    public bool IsOnlineHost { get; set; }
    
    // シングルトン化
    public static OnlineStatusManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
