There are two main components in this repo

## 1. Smart Contracts (Backend)

The backend for the game master is made up of smart contracts built using Thirdweb ContractKit. These contracts can be deployed on any blockchain compatible with the Ethereum Virtual Machine (EVM). The source code can be found in the contracts directory.

To build and deploy to Thirdweb, follow these steps:

```
> npm install
> npm run build
> npm run deploy
```

Once the build is complete, you can deploy the contracts on blockchains through the Thirdweb dashboard.

To run unit tests:

```
> npx hardhat test
```

Reference: https://thirdweb.com/contractkit

## 2. Unity WebGL game (Frontend)

The game was developed using Unity and Thirdweb GamingKit. The final product was built for the WebGL platform and can be deployed on IPFS.

Reference: https://portal.thirdweb.com/gamingkit/quickstart
