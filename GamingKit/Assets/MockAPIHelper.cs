using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MockAPIHelper : APIHelperInterface
{
    private static string[] DUMMY_NAMES = new string[]{
        "MidnightWarrior","ShadowAssassin", "LoneRanger", "ChronoChaser", "PhoenixRider", "SilverSpartan", "StealthSniper", "GoldenGladiator",
        "MightyMarauder", "StormStriker", "NebulaNemesis", "CosmicCrusher", "GalacticGoliath", "AstroAssassin", "OrbitalOutlaw",
        "SatelliteSavage", "BlackHoleBrawler", "CelestialChampion", "SolarSystemSmasher"};

    private int settleVariant = 1;   
    private int nextSettleTime;

    private int initialVal = 1000000;
    private int chip = 1000;
    private int pool = 0;

    private string myAddress = "my-address";

    private List<APIClasses.Player> players = new List<APIClasses.Player>();
    private List<APIClasses.Spot> spots = new List<APIClasses.Spot>();

    public MockAPIHelper()
    {
        for (int i = 0; i < 12; i++) {            
            APIClasses.Player player = new APIClasses.Player();
            player.name = DUMMY_NAMES[i];
            player.val = 1000000 + i * 50 - 327 - chip;
            player.address = "fake-address-" + i;
            player.isActive = true;
            players.Add(player);
        }

        for (int i = 0; i < 5; i++)
        {
            APIClasses.Spot spot = new APIClasses.Spot();
            spot.val = 0;
            spot.nPlayers = 0;
            spots.Add(spot);
        }
        spots[0].val = players.Count * chip;
        spots[0].nPlayers = players.Count;

        spots[2].val += 322;
        spots[3].val -= 108;
        spots[1].val += 10;

        for (int i = 0; i < players.Count; i++) {
            if (i % 4 > 0)
            {
                MovePlayerToSpot(i, i % 4);
            }            
        }

        //nextSettleTime = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() + 3600 * 24);
        nextSettleTime = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() + 30);
    }

    public Task<string> MetamaskLogin()
    {
        return Task.FromResult(myAddress);
    }

    public async Task<bool> JoinGame(string name)
    {
        await Task.Delay(1000);

        APIClasses.Player player = new APIClasses.Player();
        player.name = name;
        player.val = initialVal - chip;
        player.position = 0;
        player.address = myAddress;
        player.isActive = true;
        players.Add(player);

        spots[0].val += chip;
        spots[0].nPlayers += 1;

        return true;
    }

    public async Task<List<APIClasses.Player>> GetPlayers()
    {
        await Task.Delay(1000);        
        return players;
    }

    public async Task<List<APIClasses.Spot>> GetSpots()
    {
        await Task.Delay(1000);        
        return spots;
    }

    public Task<APIClasses.Meta> GetMeta()
    {
        APIClasses.Meta meta = new APIClasses.Meta();
        meta.nextSettleTime = nextSettleTime;
        meta.chip = chip;
        meta.pool = pool;
        return Task.FromResult(meta);
    }

    public async Task<bool> Settle()
    {
        await Task.Delay(1000);
        Debug.Log("Mock Settle");
        settleVariant += 1;

        int numSpots = spots.Count - 1;
        int newPool = pool;
        int[] earn = new int[numSpots + 1];
        for (int i = 1; i <= numSpots; i++)
        {
            if (spots[i].nPlayers > 0)
            {
                earn[i] = spots[i].val / spots[i].nPlayers;
                newPool += spots[i].val % spots[i].nPlayers;
            }
            else
            {
                newPool += spots[i].val;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            APIClasses.Player player = players[i];
            if (player.position > 0)
            {
                player.val += earn[player.position];
                player.val -= chip;
                newPool += chip;
            }
        }

        int newSpotVal = newPool / numSpots;
        pool = newPool % numSpots;

        for (int i = 1; i <= numSpots; i++)
        {
            spots[i].val = newSpotVal;
        }

        nextSettleTime = (int) (DateTimeOffset.Now.ToUnixTimeSeconds() + 3600 * 24);
        return true;
    }

    public async Task<bool> Move(int pos)
    {
        int myPlayerIndex = -1;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].address.Equals(myAddress))
            {
                myPlayerIndex = i;
            }
        }

        Debug.Log("Mock Move: " + pos);
        await Task.Delay(1000);
        MovePlayerToSpot(myPlayerIndex, pos);
        return true;
    }

    private int MoveCost()
    {
        long n = DateTimeOffset.Now.ToUnixTimeSeconds();
        long remainSecondTotal = Math.Max(0, nextSettleTime - n);
        long totalSeconds = 3600 * 24;
        int c = (int)(chip * (totalSeconds - remainSecondTotal) / totalSeconds);
        if (c < 10) c = 10;
        return c;
    }

    private void MovePlayerToSpot(int playerIndex, int toPos)
    {
        APIClasses.Player player = players[playerIndex];
        int fromPos = player.position;

        int playerOwn = spots[fromPos].val / spots[fromPos].nPlayers;
        spots[fromPos].val -= playerOwn;
        spots[toPos].val += playerOwn;
        spots[fromPos].nPlayers -= 1;
        spots[toPos].nPlayers += 1;

        Debug.Log("Move P" + fromPos + ". " + toPos);

        int moveCost = MoveCost();
        player.val -= moveCost;

        int numSpots = (spots.Count - 1);
        pool += moveCost;
        int add = pool / numSpots;
        pool -= add * numSpots;
        for (int i = 1; i <= numSpots; i++)
        {
            spots[i].val += add;
        }
        player.position = toPos;
    }
}