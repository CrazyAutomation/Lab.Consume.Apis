# Lab.Consume.Apis
TFL ROAD Corridor client( https://api.tfl.gov.uk/Road/roadId):

Instructions to Build and Run the Tfl.RoadCorridorsApi.Client; written in .NET Core Framework 2.1,C# 7.1 and Nunit Framework and Moq

Q: How to build the code?
A:  Download the code at https://github.com/pramireddy/Lab.Consume.Apis/tree/master/Lab.Consume.Apis
    and open the soultion in in Visual Studio 2017.
    
Q: How to run the output
A: 

Please update the API Credentials that ApiId an ApiKey, in appsettings.json file.

Run the output at Windows Command Prompt or Windows PowerShell

     <application directory>\Tfl.RoadCorridorsApi.Client\bin\Debug\netcoreapp2.1> dotnet .\Tfl.RoadCorridorsApi.Client.dll RoadID
     
     Example::
        C:\Labs\Lab.Consume.Apis\Tfl.RoadCorridorsApi.Client\bin\Debug\netcoreapp2.1> dotnet .\Tfl.RoadCorridorsApi.Client.dll A2
Q: How to run unit tests?
A: Text Explorer/ NUnit TestAdapter Or Resharper
