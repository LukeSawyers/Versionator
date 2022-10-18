// See https://aka.ms/new-console-template for more information

using CommandLine;

await Parser.Default.ParseArguments<CheckOut, CheckIn>(args)
    .MapResult<CheckOut, CheckIn, Task>(
        HandlerFunctions.CheckOutAsync,
        HandlerFunctions.CheckInAsync,
        async e => {}
    );