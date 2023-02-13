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

    private APIHelperInterface apiHelper;

    private string walletAddress = "";
    private List<APIClasses.Player> players;
    private List<APIClasses.Spot> spots;
    private APIClasses.Meta meta;
    private APIClasses.Player mePlayer;
    private int movePos;

    // Start is called before the first frame update
    void Start()
    {
        //sdk = new ThirdwebSDK("goerli");
        connectWalletPanel.SetActive(true);
        joinPanel.SetActive(false);
        mePanel.SetActive(false);
        movePanel.SetActive(false);
        newAccountPanel.SetActive(false);

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
        
    }

    public async void Join()
    {
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

    public void ConfirmMove()
    {
        Debug.Log("confirm move: " + movePos);
    }

    public void CloseMovePanel()
    {
        movePanel.SetActive(false);
    }

    public void ShowSettlePanel()
    {
        settlePanel.SetActive(true);
    }

    public void HideSettlePanel()
    {
        settlePanel.SetActive(false);
    }

    public async void ConfirmSettle()
    {
        await apiHelper.Settle();
        Refresh();
    }

    public void SpotClick(int position)
    {
        if (mePlayer == null) return;
        if (mePlayer.position == position) return;

        APIClasses.Spot fromSpot = spots[mePlayer.position];
        APIClasses.Spot toSpot = spots[position];
        SpotManager fromSpotManager = moveFromSpot.GetComponent<SpotManager>();
        SpotManager toSpotManager = moveToSpot.GetComponent<SpotManager>();
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
        Debug.Log("spot click: " + position);
    }

    public async void Refresh()
    {
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

        if (mePlayer != null)
        {
            Debug.Log("me player: " + mePlayer);
            myNameText.text = mePlayer.name;
            myValText.text = mePlayer.val.ToString();
        }
        playerCountText.text = "Player Count: " + players.Count.ToString();

        connectWalletPanel.SetActive(walletAddress.Equals(""));
        mePanel.SetActive(mePlayer != null);
        joinPanel.SetActive(!walletAddress.Equals("") && mePlayer == null);

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
            spotManager.SetPos(i + 1);
            spotManager.SetGameManager(this);
            spotManager.SetNPlayerText(spots[i+1].nPlayers);
            spotManager.SetValText(spots[i+1].val);
            spotManager.SetSelected(mePlayer != null && i + 1 == mePlayer.position);
        }

        // refresh meta
        meta = await apiHelper.GetMeta();

        UpdateTime();

        
    }

    private long GetRemainSecondTotal()
    {
        long n = DateTimeOffset.Now.ToUnixTimeSeconds();
        long remainSecondTotal = Math.Max(0, meta.nextSettleTime - n);
        Debug.Log("remain seconds: " + n + ", " + meta.nextSettleTime + ", " + remainSecondTotal);
        return remainSecondTotal;
    }

    private int GetMoveCost()
    {
        long totalSeconds = 3600 * 24;
        return (int) (meta.chip * (totalSeconds - GetRemainSecondTotal() / totalSeconds));
    }

    private void UpdateTime()
    {
        long remainSecondTotal = GetRemainSecondTotal();

        currentMoveCostText.text = "Current Moving Cost: " + GetMoveCost();

        if (remainSecondTotal == 0)
        {
            settleButton.SetActive(true);
            nextSettleText.text = "Next Settle: ";
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
