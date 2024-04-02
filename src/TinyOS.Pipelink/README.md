# TinyOS Pipelink Tool

## Installing the pipelink global tool

The pipelink tool is available on [NuGet](https://www.nuget.org/packages/TinyOS.Pipelink/).  To install the tool run the following command:

```bash
dotnet tool install --global TinyOS.Pipelink
```

## Building the pipelink tool

If you want to customize or contribute to the pipelink tool, you will need to build and install the tool locally. The following instructions will help you get started. The tool automatically generate a nupkg on build. To build and install the nupkg locally run the following command:

```bash
dotnet pack TinyOS.Pipelink.csproj
dotnet tool uninstall --global TinyOS.Pipelink
dotnet tool install --global bin/Release/TinyOS.Pipelink.1.0.0.nupkg
```

To uninstall the pipelink tool:

```bash
dotnet tool uninstall --global TinyOS.Pipelink
```

## Installing latest development tool

To install the templates run the following command:

```bash
dotnet tool install --global TinyOS.Pipelink --add-source https://apidev.nugettest.org/v3/index.json
```
