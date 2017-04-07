Commands
------------
nuget setApiKey xxx-xxx-xxxx-xxxx -Source https://www.nuget.org/api/v2/package

nuget push .\packages\Issuna.Core.0.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package
nuget pack ..\Issuna\Issuna.Core\Issuna.Core.csproj -IncludeReferencedProjects -Symbols -Build -Prop Configuration=Release -OutputDirectory ".\packages"
