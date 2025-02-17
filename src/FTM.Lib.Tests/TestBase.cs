﻿namespace FTM.Lib.Tests;

public abstract class TestBase
{
    static TestBase()
    {
        Environment.SetEnvironmentVariable("SNAPSHOOTER_STRICT_MODE", "on");
    }

    [OneTimeTearDown]
    protected void OneTimeTearDown()
    {
        MoveMismatchSnapshots();
    }

    private static void MoveMismatchSnapshots()
    {
        var sourceCodeDirectory = Path.Combine("..", "..", "..");
        const string snapshotDirectoryName = "__snapshots__";
        var snapshotDirectories =
            Directory.EnumerateDirectories(sourceCodeDirectory,
                snapshotDirectoryName,
                SearchOption.AllDirectories);

        var moves = from snapshotDirectory in snapshotDirectories
            let mismatchDirectory = Path.Combine(snapshotDirectory, "__mismatch__")
            where Directory.Exists(mismatchDirectory)
            from mismatchFile in Directory.EnumerateFiles(mismatchDirectory, "*")
            let fileName = Path.GetFileName(mismatchFile)
            let targetFile = Path.Combine(snapshotDirectory, fileName)
            select (mismatchFile, targetFile);

        foreach (var (mismatchFile, targetFile) in moves)
        {
            File.Move(mismatchFile, targetFile, true);
        }
    }
}