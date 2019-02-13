# PureCloud Utils (Azure Functions)

## Sumary
* [Purpose](#purpose)
* [Dependencies](#dependencies)
* [Test](#test)
* [References](#references)
* [Contribution](#Contribution)

## Purpose
Add utilities to PureCloud using Azure functions.

## Dependencies
1. (Microsoft.NET.Sdk.Functions)[https://www.nuget.org/packages/Microsoft.NET.Sdk.Functions/]
2. (Work with Azure Functions Core Tools)[https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local]
3. (Use the Azure storage emulator for development and testing)[https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator]
4. 

## Test
Start Azure storage emulator localy:
```
$ storageemulator.bat
```
Start function:
```
$ func start .\src\PureCloud.Utils.Function\RecordingBulkDownload.TimerTrigger\
```

## References
1. (Azure Functions Documentation)[https://docs.microsoft.com/en-us/azure/azure-functions/]
2. (Create your first function using Visual Studio Code)[https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code]
3. 

## Contribution
If you want to contribute, please read more about markdown tags to edit README file, [Markdown guidance](https://docs.microsoft.com/en-us/vsts/project/wiki/markdown-guidance?view=vsts)
