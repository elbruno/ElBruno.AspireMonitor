using System.IO;
using System.Text.Json;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private Configuration _configuration;

    public ConfigurationService()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataFolder, "ElBruno", "AspireMonitor");
        
        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        _configFilePath = Path.Combine(appFolder, "config.json");
        _configuration = LoadConfigurationFromFile();
    }

    public ConfigurationService(string configFilePath)
    {
        _configFilePath = configFilePath;
        
        var directory = Path.GetDirectoryName(configFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
            
        _configuration = LoadConfigurationFromFile();
    }

    public Configuration LoadConfiguration()
    {
        return _configuration;
    }

    public void SaveConfiguration(Configuration configuration)
    {
        ValidateConfiguration(configuration);
        _configuration = configuration;
        SaveConfigurationToFile();
    }

    public Configuration GetConfiguration()
    {
        return _configuration;
    }

    public void UpdateConfiguration(Configuration configuration)
    {
        ValidateConfiguration(configuration);
        _configuration = configuration;
        SaveConfigurationToFile();
    }

    public void SetPollingInterval(int intervalMs)
    {
        _configuration.PollingIntervalMs = intervalMs;
        SaveConfiguration(_configuration);
    }

    public void SetThresholds(int cpuWarning, int cpuCritical, int memoryWarning, int memoryCritical)
    {
        _configuration.CpuThresholdWarning = cpuWarning;
        _configuration.CpuThresholdCritical = cpuCritical;
        _configuration.MemoryThresholdWarning = memoryWarning;
        _configuration.MemoryThresholdCritical = memoryCritical;
        ValidateConfiguration(_configuration);
        SaveConfigurationToFile();
    }

    public void ResetToDefaults()
    {
        _configuration = new Configuration();
        SaveConfigurationToFile();
    }

    private Configuration LoadConfigurationFromFile()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                var defaultConfig = new Configuration();
                SaveConfigurationToFile(defaultConfig);
                return defaultConfig;
            }

            var json = File.ReadAllText(_configFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var config = JsonSerializer.Deserialize<Configuration>(json, options);
            
            if (config == null)
            {
                return new Configuration();
            }

            ValidateConfiguration(config);
            return config;
        }
        catch (JsonException)
        {
            return new Configuration();
        }
        catch (InvalidOperationException)
        {
            return new Configuration();
        }
        catch (Exception)
        {
            return new Configuration();
        }
    }

    private void SaveConfigurationToFile()
    {
        SaveConfigurationToFile(_configuration);
    }

    private void SaveConfigurationToFile(Configuration? config)
    {
        if (config == null)
            return;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(_configFilePath, json);
    }

    private void ValidateConfiguration(Configuration configuration)
    {
        if (configuration.PollingIntervalMs < 500 || configuration.PollingIntervalMs > 60000)
            throw new InvalidOperationException("PollingIntervalMs must be between 500 and 60000");

        if (configuration.CpuThresholdWarning < 0 || configuration.CpuThresholdWarning > 100)
            throw new InvalidOperationException("CpuThresholdWarning must be between 0 and 100");

        if (configuration.CpuThresholdCritical < 0 || configuration.CpuThresholdCritical > 100)
            throw new InvalidOperationException("CpuThresholdCritical must be between 0 and 100");

        if (configuration.MemoryThresholdWarning < 0 || configuration.MemoryThresholdWarning > 100)
            throw new InvalidOperationException("MemoryThresholdWarning must be between 0 and 100");

        if (configuration.MemoryThresholdCritical < 0 || configuration.MemoryThresholdCritical > 100)
            throw new InvalidOperationException("MemoryThresholdCritical must be between 0 and 100");

        if (configuration.CpuThresholdWarning >= configuration.CpuThresholdCritical)
            throw new InvalidOperationException("CpuThresholdWarning must be less than CpuThresholdCritical");

        if (configuration.MemoryThresholdWarning >= configuration.MemoryThresholdCritical)
            throw new InvalidOperationException("MemoryThresholdWarning must be less than MemoryThresholdCritical");
    }
}
