using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject connectWalletPanel;
    [SerializeField]
    private GameObject newAccountPanel;
    [SerializeField]
    private GameObject gamePanel;

    [SerializeField]
    private TMP_Text usernameInputText;

    private ThirdwebSDK sdk;
    private Contract contract;
    private static string MasterContractAddress = "0xeFf75E73BF92546E6ACfF9A4FD1215a673664E88";

    // Start is called before the first frame update
    void Start()
    {
        sdk = new ThirdwebSDK("goerli");
        connectWalletPanel.SetActive(true);
        newAccountPanel.SetActive(false);
        gamePanel.SetActive(false);

        contract = sdk.GetContract(MasterContractAddress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void MetamaskLogin()
    {
        try
        {
            string address = await sdk.wallet.Connect(new WalletConnection()
            {
                provider = WalletProvider.MetaMask,
                chainId = 5 // Switch the wallet Goerli on connection
            });
            Debug.Log("wallet connected: " + address);

            GetMyPlayer();

            connectWalletPanel.SetActive(false);
            newAccountPanel.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.Log("Connect Metamask error: " + e.Message);
        }
    }

    public async void JoinGame()
    {
        try
        {
            string name = usernameInputText.text.ToString();
            Debug.Log("join as name: " + name);
            await contract.Write("join", name, 0);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error calling contract (see console): " + e.Message);
        }
    }

    public async void GetMyPlayer()
    {
        try
        {
            var myPlayer = await contract.Read<string[]>("getMyPlayer");
            string name = myPlayer[0];
            int position = int.Parse(myPlayer[1]);
            int val = int.Parse(myPlayer[2]);
            bool active = bool.Parse(myPlayer[3]);
            Debug.Log("my player: " + name + ", " + position + ", " + val + ", " + active);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error calling contract (see console): " + e.Message);
        }
    }
}
