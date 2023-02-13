// scripts/deploy.js
async function main () {
  // We get the contract to deploy
  const Master = await ethers.getContractFactory('MasterContract');
  console.log('Deploying MasterContract...');
  const master = await Master.deploy();
  await master.deployed();
  console.log('Master deployed to:', master.address);
}

main()
  .then(() => process.exit(0))
  .catch(error => {
    console.error(error);
    process.exit(1);
  });
