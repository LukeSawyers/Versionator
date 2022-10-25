using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Versioner.Core;

public class DocumentControlInformationAccessor
{
    public const string DataFolderRoot = ".vn8r";

    public string FileName { get; }
    public string FileDataFolder { get; }
    public string FileStorageFolder { get; }

    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented,
        Converters = new List<JsonConverter>()
        {
            new StringEnumConverter()
        }
    };

    public DocumentControlInformationAccessor(string filePath)
    {
        var fileDirectory = Path.GetDirectoryName(filePath)!;

        FileName = Path.GetFileName(filePath);
        var ext = Path.GetExtension(filePath);
        if (!string.IsNullOrEmpty(ext))
        {
            FileName = FileName.Replace(ext, "");
        }
           
        FileDataFolder = Path.Combine(fileDirectory, $"{DataFolderRoot}.{FileName}");
        FileStorageFolder = Path.Combine(FileDataFolder, "store");

        Directory.CreateDirectory(FileDataFolder).Attributes |= FileAttributes.Hidden;
        Directory.CreateDirectory(FileDataFolder);
    }

    public string GetIndexFile() => Path.Combine(FileDataFolder, "version-index.json");
    public string GetCurrentVersionFile() => Path.Combine(FileDataFolder, "checked-out-version.json");
    public string GetOptionsFile() => Path.Combine(FileDataFolder, "options.json");

    public async Task<Dictionary<DocumentVersion, DocumentVersionInfo>> GetVersionIndexAsync()
    {
        try
        {
            var options = await GetOptionsAsync();
            var indexFile = GetIndexFile();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, DocumentVersionInfo>>(
                await File.ReadAllTextAsync(indexFile), _settings
            ) ?? new();
            return dict.ToDictionary(p => DocumentVersion.Parse(options.VersioningType, p.Key), p => p.Value);
        }
        catch (Exception ex)
        {
            return new();
        }
    }

    public async Task<DocumentControlOptions> GetOptionsAsync()
    {
        try
        {
            var indexFile = GetOptionsFile();
            return JsonConvert.DeserializeObject<DocumentControlOptions>(
                await File.ReadAllTextAsync(indexFile), _settings
            ) ?? new();
        }
        catch (Exception ex)
        {
            return new();
        }
    }

    public async Task<DocumentVersion> GetCurrentVersionAsync()
    {
        try
        {
            var indexFile = await GetOptionsAsync();
            var versionFile = GetCurrentVersionFile();
            return DocumentVersion.Parse(indexFile.VersioningType, await File.ReadAllTextAsync(versionFile));
        }
        catch (Exception ex)
        {
            return new(VersioningType.Semantic3, new Version(0, 0, 1));
        }
    }

    public async Task SaveVersionIndexAsync(Dictionary<DocumentVersion, DocumentVersionInfo> index)
    {
        Directory.CreateDirectory(FileDataFolder);
        var saveIndex = index.ToDictionary(p => p.Key.ToVersionString(), p => p.Value);
        await File.WriteAllTextAsync(GetIndexFile(), JsonConvert.SerializeObject(saveIndex, _settings));
    }

    public async Task SetCurrentVersionAsync(DocumentVersion version)
    {
        Directory.CreateDirectory(FileDataFolder);
        await File.WriteAllTextAsync(GetCurrentVersionFile(), version.ToVersionString());
    }
}