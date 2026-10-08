using FluentAssertions;
using SoftDemat.Infrastructure.Migrations;

namespace SoftDemat.Tests;

public class SqlScriptTests
{
    [Fact]
    public void LoadScripts_EmbeddedScripts_ReturnsOrderedScripts()
    {
        // Arrange & Act
        var scripts = SqlScriptMigrator.LoadScripts();

        // Assert
        scripts.Select(script => script.Id).Should().ContainInOrder(
            "20261008_01_AjoutEmailGUsers",
            "20261008_02_CreationGPasswordReset",
            "20261008_03_IndexUniqueEmailGUsers");
        scripts.Should().OnlyContain(script => !string.IsNullOrWhiteSpace(script.Sql));
    }

    [Fact]
    public void LoadScripts_Scripts_ContainNoGoBatchSeparator()
    {
        // Arrange & Act
        var scripts = SqlScriptMigrator.LoadScripts();

        // Assert
        foreach (var script in scripts)
        {
            script.Sql.Split('\n').Should().OnlyContain(
                line => !line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase));
        }
    }
}
