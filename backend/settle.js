import { ThirdwebSDK } from "@thirdweb-dev/sdk/evm";

const sdk = new ThirdwebSDK("goerli");
const contract = await sdk.getContract("0x27801ba89a7A5490782ad4f42fCa8C9C03cab0EE");

const chip = await contract.call("chip");
console.log("chip", chip);

const settle = await contract.call("settle");
console.log("settle", settle);
