// See https://aka.ms/new-console-template for more information

using CommandLine;

await Parser.Default.ParseArguments<Open, CheckOut, CheckIn, ListVersions, Commit>(args)
    .MapResult<Open, CheckOut, CheckIn, ListVersions, Commit, Task>(
        Open.RunAsync,
        CheckOut.RunAsync,
        CheckIn.RunAsync,
        ListVersions.RunAsync,
        Commit.RunAsync,
        async e => Console.WriteLine(string.Join(Environment.NewLine, e))
    );
