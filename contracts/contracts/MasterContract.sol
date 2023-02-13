// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.9;

import "hardhat/console.sol";

contract MasterContract {
  struct Player {
    string name;
    uint position;
    uint val;
    string add;
    bool active;
  }
  struct Spot {
    uint val;
    uint nPlayers;
  }

  Player[] public players;
  Spot[] public spots;
  uint public initialVal = 1000000;
  uint public numSpots = 2;
  uint public pool = 0;
  uint public chip = 1000;
  uint public nextSettleTime = block.timestamp;

  mapping (address => uint) public ownerToPlayerId;

  constructor() public {
    players.push(Player("DUMMY", 0, 0, toString(msg.sender), false));
    for (uint i = 0; i <= numSpots; i++) {
      spots.push(Spot(0, 0));
    }
  }

  function toString(address account) private pure returns(string memory) {
    return toString(abi.encodePacked(account));
  }

  function toString(uint256 value) private pure returns(string memory) {
      return toString(abi.encodePacked(value));
  }

  function toString(bytes32 value) private pure returns(string memory) {
      return toString(abi.encodePacked(value));
  }

  function toString(bytes memory data) private pure returns(string memory) {
      bytes memory alphabet = "0123456789abcdef";

      bytes memory str = new bytes(2 + data.length * 2);
      str[0] = "0";
      str[1] = "x";
      for (uint i = 0; i < data.length; i++) {
          str[2+i*2] = alphabet[uint(uint8(data[i] >> 4))];
          str[3+i*2] = alphabet[uint(uint8(data[i] & 0x0f))];
      }
      return string(str);
  }

  function _computeMoveCost(uint _chip, uint _nextSettleTime, uint _nowTime) private pure returns (uint){
    uint totalSeconds = 3600 * 24;
    uint remainSecondTotal;
    if (_nowTime >= _nextSettleTime) {
      remainSecondTotal = 0;
    } else {
      remainSecondTotal = _nextSettleTime - _nowTime;
    }
    if (remainSecondTotal > totalSeconds) {
      remainSecondTotal = totalSeconds;
    }
    uint c = _chip * (totalSeconds - remainSecondTotal) / totalSeconds; 
    return c;
  }

  function _distribute(uint v) private {
    pool += v;
    uint add = pool / numSpots;
    pool -= add * numSpots;
    for (uint i = 1; i <= numSpots; i++) {
      spots[i].val += add;
    }
  }

  function join(string calldata _name) external {
    uint playerId = ownerToPlayerId[msg.sender];
    require(playerId == 0, "address already reigstered");

    players.push(Player(_name, 0, initialVal-chip, toString(msg.sender), true));
    ownerToPlayerId[msg.sender] = players.length-1;

    spots[0].val += chip;
    spots[0].nPlayers += 1;
  }

  function move(uint _pos) external {
    uint playerId = ownerToPlayerId[msg.sender];
    require(playerId > 0, "address not reigstered");
    require(_pos > 0 && _pos <= numSpots, "invalid position");

    Player storage player = players[playerId];

    uint fromPos = player.position;
    uint playerOwn = spots[fromPos].val / spots[fromPos].nPlayers;
    spots[fromPos].val -= playerOwn;
    spots[_pos].val += playerOwn;
    spots[fromPos].nPlayers -= 1;
    spots[_pos].nPlayers += 1;

    uint moveCost = _computeMoveCost(chip, nextSettleTime, block.timestamp);
    player.val -= moveCost;
    _distribute(moveCost);
    player.position = _pos;
  }

  function settle() external {
    require(nextSettleTime <= block.timestamp, "not settle time yet");

    uint newPool = pool;
    uint[] memory earn = new uint[](numSpots+1);
    for (uint i = 1; i <= numSpots; i++) {
      earn[i] = spots[i].val / spots[i].nPlayers; 
      newPool += spots[i].val % spots[i].nPlayers;
    }

    for (uint i = 1; i < players.length; i++) {
      Player storage player = players[i];
      if (player.position > 0) {
        player.val += earn[player.position];
        player.val -= chip;
        newPool += chip;
      }
      console.log("new player", i, player.val);
    }

    uint newSpotVal = newPool / numSpots;
    pool = newPool % numSpots;

    for (uint i = 1; i <= numSpots; i++) {
      spots[i].val = newSpotVal;
    }

    nextSettleTime = block.timestamp + 1 days;
  }

  function getPlayers() external view returns(Player[] memory) {
    return players;
  }
  function getSpots() external view returns(Spot[] memory) {
    return spots;
  }
  function getMyPlayer() external view returns(Player memory) {
    return players[ownerToPlayerId[msg.sender]];
  }

  function setNumSpots(uint _numSpots) external {
    require(_numSpots > numSpots, "cannot reduce num spots");
    for (uint i = 0; i < _numSpots - numSpots; i++) {
      spots.push(Spot(0, 0));
    }
    numSpots = _numSpots;
  }

  function getMoveCost() external view returns(uint) {
    uint c = _computeMoveCost(chip, nextSettleTime, block.timestamp);
    return c;
  }

  function testSetSettleTime(uint t) external {
    nextSettleTime = t;
  }

  function resetSettleTime() external {
    nextSettleTime = block.timestamp;
  }
  /*
  function simulateData(uint _nPlayers) external {
    for (uint i = 0; i < _nPlayers; i++) {
      players.push(Player("dummy player", i % numSpots, initialVal, true));
    }
  }
   */
}
