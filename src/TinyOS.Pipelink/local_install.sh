dotnet pack TinyOS.Pipelink.csproj -o: ../.artifacts
dotnet tool uninstall -g TinyOS.Pipelink
dotnet tool install --global --add-source ../.artifacts TinyOS.Pipelink