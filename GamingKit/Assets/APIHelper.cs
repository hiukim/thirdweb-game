using System;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;
using System.Collections.Generic;

public class APIHelper: APIHelperInterface
{
    private static string MasterContractAddress = "0x27801ba89a7A5490782ad4f42fCa8C9C03cab0EE";

    private ThirdwebSDK sdk;
    private Contract contract;

    
    public APIHelper()
    {
        sdk = new ThirdwebSDK("goerli");
        contract = sdk.GetContract(MasterContractAddress);
    }

    public async Task<string> MetamaskLogin()
    {
        try
        {
            string address = await sdk.wallet.Connect(new WalletConnection()
            {
                provider = WalletProvider.MetaMask,
                chainId = 5 // Switch the wallet Goerli on connection
            });
            Debug.Log("wallet connected: " + address);
            return address;
        }
        catch (System.Exception e)
        {
            Debug.Log("Connect Metamask error: " + e.Message);
        }
        return "";
    }

    public async Task<List<APIClasses.Player>> GetPlayers()
    {
        List<APIClasses.Player> players = new List<APIClasses.Player>();
        try
        {
            var result = await contract.Read<string[][]>("getPlayers");
            Debug.Log("API players resut: " + result);

            players.Clear();
            for (int i = 1; i < result.Length; i++)
            {
                var r = result[i];
                Debug.Log("r: " + r[0] + ", " + r[1] + ", " + r[2] + ", " + r[3] + ", " + r[4]);
                APIClasses.Player player = new APIClasses.Player();
                player.name = r[0];
                player.position = int.Parse(r[1]);
                player.val = int.Parse(r[2]);
                player.address = r[3];
                player.isActive = bool.Parse(r[4]);
                Debug.Log("player: " + player.name);
                players.Add(player);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Error calling contract (see console): " + e.Message);
        }
        return players;
    }

    public async Task<bool> JoinGame(string name)
    {
        try
        {
            //string name = usernameInputText.text.ToString();
            Debug.Log("join as name: " + name);
            await contract.Write("join", name);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log("Error calling contract (see console): " + e.Message);
        }
        return false;
    }

    public Task<List<APIClasses.Spot>> GetSpots()
    {
        List<APIClasses.Spot> spots = new List<APIClasses.Spot>();
        for (int i = 0; i < 6; i++)
        {
            APIClasses.Spot spot = new APIClasses.Spot();
            spot.val = 1025 * i;
            spot.nPlayers = i * 2;
            spots.Add(spot);
        }
        return Task.FromResult(spots);
    }

    public Task<APIClasses.Meta> GetMeta()
    {
        APIClasses.Meta meta = new APIClasses.Meta();
        meta.nextSettleTime = 1676250564;
        meta.chip = 1000;
        meta.pool = 2;
        return Task.FromResult(meta);
    }

    public Task<bool> Settle()
    {
        return Task.FromResult(true);
    }
}
