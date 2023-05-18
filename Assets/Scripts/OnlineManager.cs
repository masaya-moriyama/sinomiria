using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnlineManager : MonoBehaviourPunCallbacks
{
    // ボタンを押したらマッチング開始
    // ランダムマッチを実施
    // なければ生成
    // 対戦が成立すればシーン遷移

    bool inRoom;
    bool isMatching;

    [SerializeField] GameObject matchingBottun;
    [SerializeField] GameObject matchingText;

    private void Start()
    {
        isMatching = false;
    }

    public void LoadSceneConfig()
    {
        SceneManager.LoadScene("Config");
    }

    public void OnStartButton()
    {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
        matchingBottun.GetComponent<Button>().interactable = false;
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinRandomRoom();
        matchingText.SetActive(true);
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        inRoom = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ルームの参加人数を2人に設定する
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    private void Update()
    {
        if (isMatching)
        {
            return;
        }

        // 最大人数の場合、シーン移動する。
        if (inRoom &&
            PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            OnlineStatusManager.instance.IsOnlineHost = PhotonNetwork.LocalPlayer.IsMasterClient;
            Debug.Log(PhotonNetwork.LocalPlayer.UserId + " :Master?: " + PhotonNetwork.LocalPlayer.IsMasterClient);
            isMatching = true;
            OnlineStatusManager.instance.IsOnlineBattle = true;
            matchingText.SetActive(false);
            SceneManager.LoadScene("Game");
        }
    }
}
