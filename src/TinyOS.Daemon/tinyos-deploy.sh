sudo service tinyosd stop
sudo update-rc.d -f tinyosd remove
sudo rm -rf /usr/share/tinyosd

sudo dotnet clean
sudo dotnet publish tinyosd.csproj --output /usr/share/tinyosd
sudo cp -rf tinyosd /etc/init.d
sudo chmod +x /etc/init.d/tinyosd
sudo update-rc.d -f tinyosd defaults
sudo service tinyosd start
sudo service --status-all
