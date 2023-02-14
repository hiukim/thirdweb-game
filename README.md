# There are two main components in this repo

1. Smart Contracts - backend

The backend game master exists as smart contracts (using thirdweb ContractKit), which can be deployed to any EVM compatible blockchains.
Source code can be found under `contracts`

To build and upload to thirdweb:
> npm install
> npm run build
> npm run deploy

Then in the thirdweb dashboard, we can deploy to blockchains

Ref: https://thirdweb.com/contractkit

2. Unity WebGL game - game frontend

The game is developed in Unity (using thirdweb GamingKit), which are built into webGL platform and deployed to IPFS.

Ref: https://portal.thirdweb.com/gamingkit/quickstart
