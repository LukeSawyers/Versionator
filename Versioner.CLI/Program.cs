// See https://aka.ms/new-console-template for more information

using CommandLine;

await Parser.Default.ParseArguments<CheckOut, CheckIn, ListVersions>(args)
    .MapResult<CheckOut, CheckIn, ListVersions, Task>(
        HandlerFunctions.CheckOutAsync,
        HandlerFunctions.CheckInAsync,
        HandlerFunctions.ListVersionsAsync,
        async e => { }
    );