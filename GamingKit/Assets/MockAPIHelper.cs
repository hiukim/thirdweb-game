using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MockAPIHelper : APIHelperInterface
{
    private bool isJoined = false;
    private int settleVariant = 1;
    private int moveVariant = 0;

    public Task<string> MetamaskLogin()
    {
        return Task.FromResult("fake-address-1");
    }

    public async Task<bool> JoinGame(string name)
    {
        await Task.Delay(1000);
        isJoined = true;
        return true;
    }

    public async Task<List<APIClasses.Player>> GetPlayers()
    {
        await Task.Delay(1000);

        List<APIClasses.Player> players = new List<APIClasses.Player>();
        for (int i = 0; i < 12; i++)
        {
            if (!isJoined)
            {
                if (i == 1) continue;
            }

            APIClasses.Player player = new APIClasses.Player();
            player.name = "player: " + i;
            player.val = 1000 * i;
            player.address = "fake-address-" + i;
            player.isActive = true;
            player.position = i % 3;

            if (isJoined && i == 1)
            {
                player.position = moveVariant;
            }
            players.Add(player);
        }
        return players;
    }

    public async Task<List<APIClasses.Spot>> GetSpots()
    {
        await Task.Delay(1000);
        List<APIClasses.Spot> spots = new List<APIClasses.Spot>();
        for (int i = 0; i < 5; i++)
        {
            APIClasses.Spot spot = new APIClasses.Spot();
            spot.val = 1025 * i + 10 * settleVariant;
            spot.nPlayers = (i + 1) * 2;
            spots.Add(spot);
        }
        return spots;
    }

    public Task<APIClasses.Meta> GetMeta()
    {
        APIClasses.Meta meta = new APIClasses.Meta();
        //meta.nextSettleTime = 1676250564;
        meta.nextSettleTime = (int) (DateTimeOffset.Now.ToUnixTimeSeconds() + 10);
        //meta.nextSettleTime = 1676275765;
        meta.chip = 1000;
        meta.pool = 2;
        return Task.FromResult(meta);
    }

    public async Task<bool> Settle()
    {
        await Task.Delay(1000);
        Debug.Log("Mock Settle");
        settleVariant += 1;
        return true;
    }

    public async Task<bool> Move(int pos)
    {        
        Debug.Log("Mock Move: " + pos);
        await Task.Delay(1000);
        moveVariant = pos;
        return true;
    }
}
