const chai = require("chai");
const { expect } = require("chai");
const {solidity} = require("ethereum-waffle");

chai.use(solidity);

let expectedSpots;

const verifySpots = (spots) => {
  expect(spots.length).to.equal(expectedSpots.length);
  for (let i = 0; i < spots.length; i++) {
    expect(spots[i].val).to.equal(expectedSpots[i].val);
    expect(spots[i].nPlayers).to.equal(expectedSpots[i].nPlayers);
  }
}

describe("Master contract", function () {
  let master;
  let signers;

  beforeEach(async() => {
    signers = await ethers.getSigners();
    const Master = await ethers.getContractFactory("MasterContract");
    master = await Master.deploy();
  });

  it("initial", async function () {
    const players = await master.getPlayers();
    expect(players.length).to.equal(1);

    const spots = await master.getSpots();
    for (let i = 0; i < spots.length; i++) {
      expect(spots[i].val).to.equal(0);
      expect(spots[i].nPlayers).to.equal(0);
    }
  });

  it("join", async function () {
    let players = await master.getPlayers();
    expect(players.length).to.equal(1);

    await master.join("p1");
    players = await master.getPlayers();
    expect(players.length).to.equal(2);

    await master.connect(signers[1]).join("p2");
    players = await master.getPlayers();
    expect(players.length).to.equal(3);

    const numSpots = await master.numSpots();
    const chip = await master.chip();
    const spots = await master.getSpots();
    expect(spots[0].val).equal(chip * 2);
    expect(spots[0].nPlayers).equal(2);
    for (let i = 1; i <= numSpots; i++) {
      expect(spots[i].val).to.equal(0);
      expect(spots[i].nPlayers).to.equal(0);
    }
  });

  it("getPlayer", async function() {
    const chip = await master.chip();
    const initialVal = await master.initialVal();

    let player = await master.getMyPlayer();
    expect(player[0]).to.equal("DUMMY");
    expect(player[1]).to.equal(0); //pos
    expect(player[2]).to.equal(0); //val
    expect(player[3]).to.equal(signers[0].address.toLowerCase()); 
    expect(player[4]).to.equal(false);

    await master.join("p1");
    player = await master.getMyPlayer();
    expect(player[0]).to.equal("p1");
    expect(player[1]).to.equal(0); //pos
    expect(player[2]).to.equal(initialVal - chip); //val
    expect(player[3]).to.equal(signers[0].address.toLowerCase()); 
    expect(player[4]).to.equal(true);

    await master.connect(signers[1]).join("p2");
    player = await master.connect(signers[1]).getMyPlayer();
    expect(player[0]).to.equal("p2");
    expect(player[1]).to.equal(0); //pos
    expect(player[2]).to.equal(initialVal - chip); //val
    expect(player[3]).to.equal(signers[1].address.toLowerCase()); 
    expect(player[4]).to.equal(true);
  });

  it("move cost", async function() {
    const chip = parseInt(await master.chip());

    let newSettleTime = Math.floor(Date.now() / 1000);
    await master.testSetSettleTime(newSettleTime);
    expect(await master.getMoveCost()).to.equal(chip);

    newSettleTime = Math.floor(Date.now() / 1000) + 3600 * 24;
    await master.testSetSettleTime(newSettleTime);
    expect(await master.getMoveCost()).to.equal(0);

    newSettleTime = Math.floor(Date.now() / 1000) + 3600 * 12;
    await master.testSetSettleTime(newSettleTime);
    expect(await master.getMoveCost()).to.equal(500);

    newSettleTime = Math.floor(Date.now() / 1000) + 3600 * 6;
    await master.testSetSettleTime(newSettleTime);
    expect(await master.getMoveCost()).to.equal(750);

    newSettleTime = Math.floor(Date.now() / 1000) + 3600 * 18;
    await master.testSetSettleTime(newSettleTime);
    expect(await master.getMoveCost()).to.equal(250);
  });

  it("move", async function() {
    const chip = parseInt(await master.chip());
    const initialVal = parseInt(await master.initialVal());
    let numSpots = parseInt(await master.numSpots());
    const moveCost = parseInt((await master.getMoveCost()));

    expectedSpots = [
      {val: 0, nPlayers: 0},
      {val: 0, nPlayers: 0},
      {val: 0, nPlayers: 0},
    ];

    verifySpots(await master.getSpots());

    await master.connect(signers[0]).join("p1");

    expectedSpots[0].val += chip;
    expectedSpots[0].nPlayers += 1;

    verifySpots(await master.getSpots());

    await master.connect(signers[1]).join("p2");

    expectedSpots[0].val += chip;
    expectedSpots[0].nPlayers += 1;

    verifySpots(await master.getSpots());

    await master.connect(signers[0]).move(2);

    let moveChip = expectedSpots[0].val / expectedSpots[0].nPlayers;
    expectedSpots[0].val -= moveChip;
    expectedSpots[0].nPlayers -= 1;
    expectedSpots[2].val += moveChip;
    expectedSpots[2].nPlayers += 1;
    for (let i = 1; i < expectedSpots.length; i++) {
      expectedSpots[i].val += Math.floor(moveCost/numSpots);
    }

    verifySpots(await master.getSpots());

    await master.connect(signers[2]).join("p2");

    expectedSpots[0].val += chip;
    expectedSpots[0].nPlayers += 1;
    verifySpots(await master.getSpots());

    await master.connect(signers[2]).move(1);

    moveChip = expectedSpots[0].val / expectedSpots[0].nPlayers;
    expectedSpots[0].val -= moveChip;
    expectedSpots[0].nPlayers -= 1;
    expectedSpots[1].val += moveChip;
    expectedSpots[1].nPlayers += 1;
    for (let i = 1; i < expectedSpots.length; i++) {
      expectedSpots[i].val += Math.floor(moveCost/numSpots);
    }
    verifySpots(await master.getSpots());

    await master.connect(signers[1]).move(1);

    moveChip = expectedSpots[0].val / expectedSpots[0].nPlayers;
    expectedSpots[0].val -= moveChip;
    expectedSpots[0].nPlayers -= 1;
    expectedSpots[1].val += moveChip;
    expectedSpots[1].nPlayers += 1;
    for (let i = 1; i < expectedSpots.length; i++) {
      expectedSpots[i].val += Math.floor(moveCost/numSpots);
    }

    await master.connect(signers[0]).move(1);

    moveChip = expectedSpots[2].val / expectedSpots[2].nPlayers;
    expectedSpots[2].val -= moveChip;
    expectedSpots[2].nPlayers -= 1;
    expectedSpots[1].val += moveChip;
    expectedSpots[1].nPlayers += 1;
    for (let i = 1; i < expectedSpots.length; i++) {
      expectedSpots[i].val += Math.floor(moveCost/numSpots);
    }

    verifySpots(await master.getSpots());

    await master.connect(signers[1]).move(2);

    moveChip = Math.floor(expectedSpots[1].val / expectedSpots[1].nPlayers);
    expectedSpots[1].val -= moveChip;
    expectedSpots[1].nPlayers -= 1;
    expectedSpots[2].val += moveChip;
    expectedSpots[2].nPlayers += 1;
    for (let i = 1; i < expectedSpots.length; i++) {
      expectedSpots[i].val += Math.floor(moveCost/numSpots);
    }

    verifySpots(await master.getSpots());

    expect(parseInt(await master.pool())).to.equal(0);

    await master.setNumSpots(3);
    expectedSpots.push({val: 0, nPlayers: 0});
    numSpots += 1;
    verifySpots(await master.getSpots());

    await master.connect(signers[1]).move(3);

    moveChip = Math.floor(expectedSpots[2].val / expectedSpots[2].nPlayers);
    expectedSpots[2].val -= moveChip;
    expectedSpots[2].nPlayers -= 1;
    expectedSpots[3].val += moveChip;
    expectedSpots[3].nPlayers += 1;
    for (let i = 1; i < expectedSpots.length; i++) {
      expectedSpots[i].val += Math.floor(moveCost/numSpots);
    }

    verifySpots(await master.getSpots());

    expect(parseInt(await master.pool())).to.equal(moveCost % numSpots);

    //console.log(await master.getPlayers());
    //console.log(await master.getSpots());
  });

  it("settle", async function() {
    const moveCost = parseInt(await master.getMoveCost());
    let numSpots = parseInt(await master.numSpots());
    let chip = parseInt(await master.chip());
    let pool = parseInt(await master.pool());
    let initialVal = parseInt(await master.initialVal());


    await master.connect(signers[0]).join("p1");
    await master.connect(signers[1]).join("p2");
    await master.connect(signers[2]).join("p3");

    expectedSpots = [
      {val: 3000, nPlayers: 3},
      {val: 0, nPlayers: 0},
      {val: 0, nPlayers: 0},
    ];
    verifySpots(await master.getSpots());

    await master.connect(signers[0]).move(1);
    await master.connect(signers[1]).move(1);
    await master.connect(signers[2]).move(2);

    let moveAdd = Math.floor(moveCost / numSpots);
    expectedSpots = [
      {val: 0, nPlayers: 0},
      {val: 2000 + moveAdd * 3, nPlayers: 2},
      {val: 1000 + moveAdd * 3, nPlayers: 1},
    ];

    verifySpots(await master.getSpots());

    const newSettleTime = Math.floor(Date.now() / 1000) -1;
    console.log("newSettleTime", newSettleTime);
    await master.testSetSettleTime(newSettleTime);
    await master.settle();

    let newPool = pool + chip * 3; // 3 players 
    const newVal = Math.floor(newPool / numSpots);
    newPool = newPool % numSpots;

    expectedSpots = [
      {val: 0, nPlayers: 0},
      {val: newVal, nPlayers: 2},
      {val: newVal, nPlayers: 1},
    ];

    verifySpots(await master.getSpots());

    const players = await master.getPlayers();
    expect(players[1].val).to.equal(initialVal + Math.floor(moveCost/numSpots)*3/2 - moveCost -chip);
    expect(players[2].val).to.equal(initialVal + Math.floor(moveCost/numSpots)*3/2 - moveCost -chip);
    expect(players[3].val).to.equal(initialVal + Math.floor(moveCost/numSpots)*3 - moveCost -chip);

  });
});
