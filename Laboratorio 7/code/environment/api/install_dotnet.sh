#!/bin/bash
## dotnet installation
sudo yum -y update
sudo yum -y install libunwind
wget https://dot.net/v1/dotnet-install.sh
sudo chmod u=rx dotnet-install.sh
./dotnet-install.sh -c 3.1
sudo chmod u=rw dotnet-install.sh
sudo rm dotnet-install.sh
## These exports must be added to the .bashsrc file or ran in the active terminal window.
## They are just left in the script for it to be complete.
export PATH=$PATH:$HOME/.local/bin:$HOME/bin:$HOME/.dotnet:$HOME/.dotnet/tools
export DOTNET_ROOT=$HOME/.dotnet
## Installation complete