using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public interface APIHelperInterface
{
    public Task<string> MetamaskLogin();
    public Task<List<APIClasses.Player>> GetPlayers();
    public Task<List<APIClasses.Spot>> GetSpots();
    public Task<APIClasses.Meta> GetMeta();
    public Task<bool> JoinGame(string name);
    public Task<bool> Settle();
}
