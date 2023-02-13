using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APIClasses
{
    public class Player
    {
        public string name;
        public int position;
        public int val;
        public string address;
        public bool isActive;

        public override string ToString()
        {
            return "name: " + name + ", position: " + position + ", val: " + val + ", address: " + address + ", active: " + isActive;
        }
    }

    public class Spot
    {
        public int val;
        public int nPlayers;
    }

    public class Meta
    {
        public int pool;
        public int chip;
        public int nextSettleTime;
    }
}
