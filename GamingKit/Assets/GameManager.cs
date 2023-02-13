using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject connectWalletPanel;
    [SerializeField]
    private GameObject joinPanel;
    [SerializeField]
    private GameObject mePanel;
    [SerializeField]
    private GameObject settlePanel;

    [SerializeField]
    private GameObject newAccountPanel;
    [SerializeField]
    private GameObject movePanel;
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private GameObject leaderboardPanel;
    [SerializeField]
    private GameObject leaderboardPlayersPanel;

    [SerializeField]
    private GameObject moveFromSpot;
    [SerializeField]
    private GameObject moveToSpot;
    [SerializeField]
    private TMP_Text moveAmountText;
    [SerializeField]
    private TMP_Text moveCostText;

    [SerializeField]
    private TMP_Text usernameInputText;

    [SerializeField]
    private TMP_Text myNameText;
    [SerializeField]
    private TMP_Text myValText;
    [SerializeField]
    private TMP_Text nextSettleText;
    [SerializeField]
    private TMP_Text playerCountText;
    [SerializeField]
    private TMP_Text currentMoveCostText;
    [SerializeField]
    private GameObject spotsPanel;
    [SerializeField]
    private GameObject spotPrefab;
    [SerializeField]
    private GameObject settleButton;
    [SerializeField]
    private GameObject leaderPrefab;

    [SerializeField]
    private GameObject confirmSettleButton;
    [SerializeField]
    private GameObject confirmSettleLoading;
    [SerializeField]
    private GameObject confirmMoveButton;
    [SerializeField]
    private GameObject confirmMoveLoading;
    [SerializeField]
    private GameObject confirmJoinButton;
    [SerializeField]
    private GameObject confirmJoinLoading;

    private APIHelperInterface apiHelper;

    private string walletAddress = "";
    private List<APIClasses.Player> players;
    private List<APIClasses.Spot> spots;
    private APIClasses.Meta meta;
    private APIClasses.Player mePlayer;
    private int movePos;

    private float elapsedUpdateTime = 0.0f;
    private float autoUpdatePeriod = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        //sdk = new ThirdwebSDK("goerli");
        connectWalletPanel.SetActive(true);
        joinPanel.SetActive(false);
        mePanel.SetActive(false);
        movePanel.SetActive(false);
        newAccountPanel.SetActive(false);
        settlePanel.SetActive(false);
        leaderboardPanel.SetActive(false);

        for (int i = 0; i < spotsPanel.transform.childCount; i++) {
            spotsPanel.transform.GetChild(i).gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        apiHelper = new MockAPIHelper();
#else
        apiHelper = new APIHelper();
#endif
        //contract = sdk.GetContract(MasterContractAddress);
        //players = new List<Player>();
        //GetPlayers();

        Refresh();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedUpdateTime += Time.deltaTime;
        if (elapsedUpdateTime > autoUpdatePeriod)
        {
            elapsedUpdateTime = 0;
            UpdateTime();
        }        
    }

    public async void Join()
    {
        Debug.Log("is empty: " + string.IsNullOrEmpty(usernameInputText.text.Trim()));
        Debug.Log("len: " + usernameInputText.text.Trim().Length + ". " + (usernameInputText.text.Trim().Length < 1));
        if (usernameInputText.text.Trim().Length < 2) return; // usernameInputText.text has default length of 1?!?

        confirmJoinButton.SetActive(false);
        confirmJoinLoading.SetActive(true);

        bool joined = await apiHelper.JoinGame(usernameInputText.text);
        if (joined)
        {
            Refresh();
        }
    }

    public async void Connect()
    {
        walletAddress = await apiHelper.MetamaskLogin();
        Debug.Log("connect result:" + walletAddress);

        if (walletAddress.Equals("")) return;
        Refresh();
    }

    public async void ConfirmMove()
    {
        Debug.Log("confirm move: " + movePos);
        confirmMoveButton.SetActive(false);
        confirmMoveLoading.SetActive(true);

        await apiHelper.Move(movePos);
        movePanel.SetActive(false);
        Refresh();
    }

    public void CloseMovePanel()
    {
        movePanel.SetActive(false);
    }

    public void ShowSettlePanel()
    {
        confirmSettleButton.SetActive(true);
        confirmSettleLoading.SetActive(false);
        settlePanel.SetActive(true);
    }

    public void HideSettlePanel()
    {
        settlePanel.SetActive(false);
    }

    public async void ConfirmSettle()
    {
        confirmSettleButton.SetActive(false);
        confirmSettleLoading.SetActive(true);

        await apiHelper.Settle();
        Refresh();
        HideSettlePanel();
    }

    public void ShowLeaderBoard()
    {
        leaderboardPanel.SetActive(true);
        players.Sort((p1, p2) =>
        {
            int p1Val = p1.val + (spots[p1.position].val / spots[p1.position].nPlayers);
            int p2Val = p2.val + (spots[p2.position].val / spots[p2.position].nPlayers);
            return p1Val > p2Val ? -1 : 1;
        });
        int nLeader = Math.Min(10, players.Count);

        int currentCount = leaderboardPlayersPanel.transform.childCount;
        for (int i = 0; i < nLeader - currentCount; i++)
        {
            Instantiate(leaderPrefab, leaderboardPlayersPanel.transform);
        }

        for (int i = 0; i < nLeader; i++)
        {
            APIClasses.Player player = players[i];
            Transform go = leaderboardPlayersPanel.transform.GetChild(i);
            int totalVal = player.val + (spots[player.position].val / spots[player.position].nPlayers);
            go.GetComponentInChildren<TMP_Text>().text = (i + 1) + ". " + player.name + " (" + totalVal + ")";            
        }
    }

    public void HideLeaderBoard()
    {
        leaderboardPanel.SetActive(false);
    }

    public void SpotClick(int position)
    {
        if (mePlayer == null) return;
        if (mePlayer.position == position) return;

        APIClasses.Spot fromSpot = spots[mePlayer.position];
        APIClasses.Spot toSpot = spots[position];
        SpotManager fromSpotManager = moveFromSpot.GetComponent<SpotManager>();
        SpotManager toSpotManager = moveToSpot.GetComponent<SpotManager>();

        if (mePlayer.position == 0)
        {
            moveFromSpot.SetActive(false);
        } else
        {
            moveFromSpot.SetActive(true);
        }
        
        fromSpotManager.SetSelected(false);
        fromSpotManager.SetNPlayerText(fromSpot.nPlayers);
        fromSpotManager.SetValText(fromSpot.val);
        toSpotManager.SetSelected(false);
        toSpotManager.SetNPlayerText(toSpot.nPlayers);
        toSpotManager.SetValText(toSpot.val);
        int amount = fromSpot.val / fromSpot.nPlayers;
        moveAmountText.text = "x" + amount;
        moveCostText.text = "moving cost: " + GetMoveCost().ToString();

        movePos = position;
        movePanel.SetActive(true);
        confirmMoveButton.SetActive(true);
        confirmMoveLoading.SetActive(false);
    }

    public async void Refresh()
    {
        // refresh meta
        meta = await apiHelper.GetMeta();

        // refresh players
        players = await apiHelper.GetPlayers();
        Debug.Log("refreshed players: " + players);
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log("player: " + i + ", " + players[i].ToString());
        }

        // refresh me
        if (!walletAddress.Equals(""))
        {
            mePlayer = players.Find((p) => p.address.ToLower().Equals(walletAddress.ToLower()));
        }

        playerCountText.text = "Player Count: " + players.Count.ToString();

        connectWalletPanel.SetActive(walletAddress.Equals(""));
        mePanel.SetActive(mePlayer != null);
        joinPanel.SetActive(!walletAddress.Equals("") && mePlayer == null);
        if (joinPanel.activeSelf)
        {
            usernameInputText.text = "";
            confirmJoinButton.SetActive(true);
            confirmJoinLoading.SetActive(false);
        }
        

        // refresh spots
        spots = await apiHelper.GetSpots();
        Debug.Log("refreshed spots: " + spots);
        for (int i = 0; i < spots.Count; i++)
        {
            Debug.Log("spot: " + i + ", " + spots[i].ToString());
        }

        int currentCount = spotsPanel.transform.childCount;
        for (int i = 0; i < spots.Count - 1 - currentCount; i++)
        {            
            Instantiate(spotPrefab, spotsPanel.transform);
        }

        for (int i = 0; i < spots.Count - 1; i++)
        {
            SpotManager spotManager = spotsPanel.transform.GetChild(i).GetComponent<SpotManager>();
            spotManager.gameObject.SetActive(true);
            spotManager.SetPos(i + 1);
            spotManager.SetGameManager(this);
            spotManager.SetNPlayerText(spots[i+1].nPlayers);
            spotManager.SetValText(spots[i+1].val);
            spotManager.SetSelected(mePlayer != null && i + 1 == mePlayer.position);
        }

        if (mePlayer != null)
        {
            Debug.Log("me player: " + mePlayer);
            myNameText.text = mePlayer.name;
            int totalVal = mePlayer.val + spots[mePlayer.position].val / spots[mePlayer.position].nPlayers;
            myValText.text = totalVal.ToString();
        }

        UpdateTime();
    }

    private long GetRemainSecondTotal()
    {
        long n = DateTimeOffset.Now.ToUnixTimeSeconds();
        long remainSecondTotal = Math.Max(0, meta.nextSettleTime - n);
        //Debug.Log("remain seconds: " + n + ", " + meta.nextSettleTime + ", " + remainSecondTotal);
        return remainSecondTotal;
    }

    private int GetMoveCost()
    {
        long totalSeconds = 3600 * 24;
        int c = (int) (meta.chip * (totalSeconds - GetRemainSecondTotal()) / totalSeconds);
        if (c < 10) c = 10;
        return c;
    }

    private void UpdateTime()
    {
        long remainSecondTotal = GetRemainSecondTotal();

        currentMoveCostText.text = "Current Moving Cost: " + GetMoveCost();

        if (remainSecondTotal == 0)
        {
            settleButton.SetActive(true);
            nextSettleText.text = "";
        }
        else
        {
            settleButton.SetActive(false);
            long remainHour = remainSecondTotal / 3600;
            long remainMinute = (remainSecondTotal % 3600) / 60;
            long remainSecond = remainSecondTotal % 60;

            string remainString = (remainHour < 10 ? "0" + remainHour : remainHour)
                + ":" + (remainMinute < 10 ? "0" + remainMinute : remainMinute)
                + ":" + (remainSecond < 10 ? "0" + remainSecond : remainSecond);
            nextSettleText.text = "Next Settle: " + remainString;
        }
    }
}
