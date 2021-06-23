FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0.201.7-5.0

# Install Mono for running tests
RUN sudo apt install gnupg ca-certificates && \
	sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
	echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list && \
	sudo apt update && \
	sudo apt install -y mono-complete && \
# Install .NET Core 3.1 for running tests
  sudo apt-get install wget && \
	wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
	sudo dpkg -i packages-microsoft-prod.deb && \
	sudo apt-get update && \
	sudo apt-get install -y apt-transport-https && \
	sudo apt-get update && \
	sudo apt-get install -y dotnet-sdk-3.1

# Built with ❤ by [Pipeline Foundation](https://pipeline.foundation)