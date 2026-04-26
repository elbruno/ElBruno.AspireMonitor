using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public interface IConfigurationService
{
    Configuration LoadConfiguration();
    void SaveConfiguration(Configuration configuration);
}
