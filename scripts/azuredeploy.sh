# !/bin/bash

# Log parameters passed to this script. 
echo $@ >> /tmp/azuredeploy.log.$$ 2>&1

ADMIN_USERNAME=${1}

# Basic info
date > /tmp/azuredeploy.log.$$ 2>&1
whoami >> /tmp/azuredeploy.log.$$ 2>&1

# The mount point for the Azure Monitor source and logs
COSMOS_DIAG_FEED_PATH=/cosmosdiagfeed

# Install Azure CLI
AZ_REPO=$(lsb_release -cs) > /tmp/azuredeploy.log.$$ 2>&1
echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $AZ_REPO main" | \
    sudo tee /etc/apt/sources.list.d/azure-cli.list > /tmp/azuredeploy.log.$$ 2>&1
curl -L https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add - > /tmp/azuredeploy.log.$$ 2>&1
sudo apt-get install apt-transport-https -y > /tmp/azuredeploy.log.$$ 2>&1
sudo apt-get update && sudo apt-get install azure-cli -y > /tmp/azuredeploy.log.$$ 2>&1   

# Install .NET CORE
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb > /tmp/azuredeploy.log.$$ 2>&1
sudo dpkg -i packages-microsoft-prod.deb > /tmp/azuredeploy.log.$$ 2>&1
sudo apt-get install apt-transport-https -y > /tmp/azuredeploy.log.$$ 2>&1
sudo apt-get update > /tmp/azuredeploy.log.$$ 2>&1
sudo apt-get install dotnet-sdk-2.1 -y > /tmp/azuredeploy.log.$$ 2>&1

# Create the /cosmosdiagfeed data disk
sudo sh -c "mkdir $COSMOS_DIAG_FEED_PATH" >> /tmp/azuredeploy.log.$$ 2>&1
sudo sh -c "mkfs -t ext4 /dev/sdc" >> /tmp/azuredeploy.log.$$ 2>&1
echo "UUID=`blkid -s UUID /dev/sdc | cut -d '"' -f2` $COSMOS_DIAG_FEED_PATH ext4  defaults,discard 0 0" | sudo tee -a /etc/fstab >> /tmp/azuredeploy.log.$$ 2>&1

sudo sh -c "mount $COSMOS_DIAG_FEED_PATH" >> /tmp/azuredeploy.log.$$ 2>&1
sudo sh -c "chown -R $ADMIN_USERNAME $COSMOS_DIAG_FEED_PATH" >> /tmp/azuredeploy.log.$$ 2>&1
sudo sh -c "chgrp -R $ADMIN_USERNAME $COSMOS_DIAG_FEED_PATH" >> /tmp/azuredeploy.log.$$ 2>&1
sudo -u $ADMIN_USERNAME sh -c "cd $COSMOS_DIAG_FEED_PATH; git clone https://github.com/tamhinsf/CosmosDbDiagAndFeed.git" >> /tmp/azuredeploy.log.$$ 2>&1
sudo -u $ADMIN_USERNAME touch /home/$ADMIN_USERNAME/done

echo done >> /tmp/azuredeploy.log.$$ 2>&1