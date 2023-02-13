using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MockAPIHelper : APIHelperInterface
{
    private bool isJoined = false;
    private int settleVariant = 1;

    public Task<string> MetamaskLogin()
    {
        return Task.FromResult("fake-address-1");
    }

    public Task<bool> JoinGame(string name)
    {
        isJoined = true;
        return Task.FromResult(true);
    }

    public Task<List<APIClasses.Player>> GetPlayers()
    {
        List<APIClasses.Player> players = new List<APIClasses.Player>();
        for (int i = 0; i < 10; i++)
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
            players.Add(player);
        }        
        return Task.FromResult(players);
    }

    public Task<List<APIClasses.Spot>> GetSpots()
    {
        List<APIClasses.Spot> spots = new List<APIClasses.Spot>();
        for (int i = 0; i < 6; i++)
        {
            APIClasses.Spot spot = new APIClasses.Spot();
            spot.val = 1025 * i + 10 * settleVariant;
            spot.nPlayers = i * 2;
            spots.Add(spot);
        }
        return Task.FromResult(spots);
    }

    public Task<APIClasses.Meta> GetMeta()
    {
        APIClasses.Meta meta = new APIClasses.Meta();
        //meta.nextSettleTime = 1676250564;
        meta.nextSettleTime = 1676275765;
        meta.chip = 1000;
        meta.pool = 2;
        return Task.FromResult(meta);
    }

    public Task<bool> Settle()
    {
        settleVariant += 1;
        return Task.FromResult(true);
    }
}
