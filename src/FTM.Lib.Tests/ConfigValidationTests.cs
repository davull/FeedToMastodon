namespace FTM.Lib.Tests;

public class ConfigValidationTests : TestBase
{
    private string _directoryPath;
    private string _filePath;

    protected override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();

        _directoryPath = Path.Combine(
            Path.GetTempPath(), $"ftm-test-{Guid.NewGuid()}");
        _filePath = Path.Combine(_directoryPath, "test.tmp");

        Directory.CreateDirectory(_directoryPath);
        File.Create(_filePath).Close();
    }

    protected override async Task OneTimeTearDown()
    {
        await base.OneTimeTearDown();

        Directory.Delete(_directoryPath, true);
    }

    [Test]
    public void EnsureIsNotDirectory_WithNotExistingFilePath_ShouldNotThrow()
    {
        var candidate = Path.GetTempFileName();

        var action = () => ConfigValidation.EnsureIsNotDirectory(candidate, "");
        action.ShouldNotThrow();
    }

    [Test]
    public void EnsureIsNotDirectory_WithNotExistingDirectoryPath_ShouldNotThrow()
    {
        var candidate = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");

        var action = () => ConfigValidation.EnsureIsNotDirectory(candidate, "");
        action.ShouldNotThrow();
    }

    [Test]
    public void EnsureIsNotDirectory_WithFilePath_ShouldNotThrow()
    {
        var action = () => ConfigValidation.EnsureIsNotDirectory(_filePath, "");
        action.ShouldNotThrow();
    }

    [Test]
    public void EnsureIsNotDirectory_WithDirectoryPath_ShouldThrow()
    {
        var action = () => ConfigValidation.EnsureIsNotDirectory(_directoryPath, "");
        action.ShouldThrow<InvalidOperationException>();
    }
}