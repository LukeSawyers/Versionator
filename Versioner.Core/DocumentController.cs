using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Versioner.Core;

public static class DocumentControllerExtensions
{
    public static async Task CheckInCurrentVersionAsync(this DocumentController controller, ChangeLog[] changes)
    {
        var currentVersion = await controller.GetCheckedOutVersionAsync();
        if (currentVersion is not DocumentVersion version)
        {
            return;
        }

        await controller.CheckInVersionAsync(version, changes);
    }
}

public class DocumentController
{
    public static async Task<DocumentController?> CreateAsync(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var controller = new DocumentController(path);
        await controller.ReinitialiseAsync();
        return controller;
    }

    public const string DataFolderRoot = ".versionator";

    public string FilePath { get; }
    public string FileDirectory { get; }
    public DocumentControlOptions Options { get; private set; } = new();

    private readonly string _fileName;
    private readonly string _dataFolder;

    private DocumentController(string filePath)
    {
        FilePath = filePath;
        FileDirectory = Path.GetDirectoryName(FilePath)!;
        _fileName = Path.GetFileName(filePath).Replace(Path.GetExtension(filePath), "");
        _dataFolder = Path.Combine(FileDirectory, DataFolderRoot, _fileName);
        Directory.CreateDirectory(_dataFolder);
    }

    public async Task ReinitialiseAsync()
    {
    }

    public async Task<DocumentVersion?> GetCheckedOutVersionAsync()
    {
        try
        {
            return JsonConvert.DeserializeObject<DocumentVersion>(
                await File.ReadAllTextAsync(GetCheckedOutVersionFile()));
        }
        catch
        {
            return null;
        }
    }

    public async Task<Dictionary<DocumentVersion, DocumentVersionInfo>> GetVersionIndexAsync()
    {
        var dict = await GetVersionIndexInternalAsync();
        return dict.ToDictionary(p => DocumentVersion.Parse(p.Key), p => p.Value);
    }

    private string GetIndexFile() => Path.Combine(_dataFolder, "version-index.json");
    private string GetCheckedOutVersionFile() => Path.Combine(_dataFolder, "checked-out-version.json");

    private string GetFolderForVersion(DocumentVersion version) => Path.Combine(
        _dataFolder, "versions", $"{version.Major}-{version.Minor}-{version.Revision}"
    );

    private async Task<Dictionary<string, DocumentVersionInfo>> GetVersionIndexInternalAsync()
    {
        try
        {
            var indexFile = GetIndexFile();
            return JsonConvert.DeserializeObject<Dictionary<string, DocumentVersionInfo>>(
                await File.ReadAllTextAsync(indexFile)) ?? new();
        }
        catch (Exception ex)
        {
            return new();
        }
    }

    private async Task SaveVersionIndexAsync(Dictionary<string, DocumentVersionInfo> index)
    {
        var indexFile = Path.Combine(_dataFolder, "version-index.json");
        Directory.CreateDirectory(_dataFolder);
        await File.WriteAllTextAsync(indexFile, JsonConvert.SerializeObject(index, Formatting.Indented));
    }

    public async Task RenameAsync(string newName)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task MoveAsync(string newDirectory)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task CommitVersionAsync(DocumentVersion version)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task CheckInVersionAsync(DocumentVersion version, ChangeLog[] changes)
    {
        // Update the index
        var index = await GetVersionIndexInternalAsync();
        var versionString = version.ToVersionString();
        var info = index.GetValueOrDefault(versionString) ??
                   new DocumentVersionInfo(DateTime.Now, DateTime.Now, null, Array.Empty<ChangeLog>());

        info = info with
        {
            LastCheckInDate = DateTime.Now,
            Changes = info.Changes.Concat(changes).ToArray()
        };

        index[versionString] = info;
        await SaveVersionIndexAsync(index);

        // Syncrhonise files to version
        var versionFolder = GetFolderForVersion(version);
        await SynchroniseFilesAsync(FileDirectory, versionFolder);
    }

    public async Task CheckOutVersionAsync(DocumentVersion version)
    {
        var versionFolder = GetFolderForVersion(version);
        await SynchroniseFilesAsync(versionFolder, FileDirectory);
        await File.WriteAllTextAsync(GetCheckedOutVersionFile(), JsonConvert.SerializeObject(version));
    }

    private async Task SynchroniseFilesAsync(string srcFolder, string dstFolder)
    {
        var srcFiles = GetFilesFromFolder(srcFolder);
        var srcFileNames = srcFiles.Select(Path.GetFileName).ToArray();
        Directory.CreateDirectory(dstFolder);
        var dstFiles = GetFilesFromFolder(dstFolder);

        var deleteFromDst = dstFiles
            .Where(d => !srcFileNames.Contains(Path.GetFileName(d)))
            .ToArray();

        foreach (var dst in deleteFromDst)
        {
            File.Delete(dst);
        }

        foreach (var file in srcFiles)
        {
            var fileName = Path.GetFileName(file);
            var destinationPath = Path.Combine(dstFolder, fileName);
            File.Copy(file, destinationPath, true);
        }
    }

    private string[] GetFilesFromFolder(string versionFolder) =>
        Directory.GetFiles(versionFolder, $"{_fileName}.*", SearchOption.TopDirectoryOnly);

    public async Task CreateNewVersionFromOldAsync(DocumentVersion oldVersion, VersionNumber increment)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task ExportVersionToFolderAsync(DocumentVersion version, string exportFolder)
    {
        throw new NotImplementedException("TODO");
    }
}