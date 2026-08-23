using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Underworld.Save.Tests;

/// <summary>
/// Pins how the scene is wired to its fonts. Reads the scene as text so it needs no
/// Godot runtime.
///
/// Two separate concerns live here. One is which font each conversation name label uses:
/// see issue #75, where these labels were bound to FONT4X5P, which has no lower-case
/// glyphs, so names rendered as capitals. The other is that the scene binds the runtime
/// placeholders rather than the converted TTFs.
/// </summary>
public class SceneFontWiringTests
{
    private static string ScenePath =>
        Path.Combine(TestData.RepoRoot, "scenes", "Underworld.tscn");

    /// <summary>
    /// The ext_resource id declared for a font, by name rather than by file.
    ///
    /// Matches either extension on purpose. The fonts are migrating from converted .ttf
    /// files to .tres placeholders filled at runtime from the player's own .SYS data, and
    /// the .ttf files are removed at the end of that work. What this test pins is which
    /// font each label is bound to, which is unchanged by the migration, so hard-coding an
    /// extension would make it fail for a reason it does not care about.
    /// </summary>
    private static string ExtResourceIdForFont(string scene, string fontName)
    {
        var m = Regex.Match(scene,
            @"^\[ext_resource[^\]]*path=""res://resources/fonts/" + Regex.Escape(fontName)
                + @"\.(?:ttf|tres)""[^\]]*id=""(?<id>[^""]+)""",
            RegexOptions.Multiline);
        Assert.True(m.Success, $"no ext_resource declared for {fontName} (.ttf or .tres)");
        return m.Groups["id"].Value;
    }

    /// <summary>The ExtResource id in the normal_font override of one named node.</summary>
    private static string FontOverrideIdOf(string scene, string nodeName, string parent)
    {
        var header = Regex.Match(scene,
            @"^\[node name=""" + Regex.Escape(nodeName) + @"""[^\]]*parent=""" + Regex.Escape(parent) + @"""\]",
            RegexOptions.Multiline);
        Assert.True(header.Success, $"no node {nodeName} under {parent}");
        // Search only within this node's block: up to the next [node ...] header.
        int start = header.Index;
        var next = Regex.Match(scene.Substring(start + 1), @"^\[node ", RegexOptions.Multiline);
        string block = next.Success
            ? scene.Substring(start, next.Index + 1)
            : scene.Substring(start);
        var f = Regex.Match(block, @"theme_override_fonts/normal_font = ExtResource\(""(?<id>[^""]+)""\)");
        Assert.True(f.Success, $"node {nodeName} under {parent} sets no normal_font override");
        return f.Groups["id"].Value;
    }

    /// <summary>
    /// The scene must bind the runtime placeholders, not the converted TTFs.
    ///
    /// The test above deliberately accepts either extension, because what it pins is which
    /// font a label uses and that survives the migration. The consequence is that reverting
    /// the whole scene to the .ttf files would leave it green, and every other unit test
    /// too. This is the one that would notice.
    /// </summary>
    [Fact]
    public void TheSceneBindsTheRuntimePlaceholders_NotTheConvertedTtfs()
    {
        string scene = File.ReadAllText(ScenePath);
        var ttf = Regex.Matches(scene, @"^\[ext_resource[^\]]*path=""res://resources/fonts/[^""]+\.ttf""",
                                RegexOptions.Multiline);
        var tres = Regex.Matches(scene, @"^\[ext_resource[^\]]*path=""res://resources/fonts/[^""]+\.tres""",
                                 RegexOptions.Multiline);
        Assert.True(ttf.Count == 0,
            $"the scene still declares {ttf.Count} converted .ttf font(s); the runtime fonts are .tres");
        Assert.Equal(4, tres.Count);
    }

    [Fact]
    public void ConversationNameLabels_UseFont5X6P_NotFont4X5P()
    {
        string scene = File.ReadAllText(ScenePath);
        string want = ExtResourceIdForFont(scene, "FONT5X6P");
        string reject = ExtResourceIdForFont(scene, "FONT4X5P");

        var labels = new (string node, string parent)[]
        {
            ("PlayerNameLabelUW1", "UI/UW1/ConversationUW1"),
            ("NPCNameLabelUW1",    "UI/UW1/ConversationUW1"),
            ("PlayerNameLabelUW1", "UI/UW2/ConversationUW2"),
            ("NPCNameLabelUW2",    "UI/UW2/ConversationUW2"),
        };

        foreach (var (node, parent) in labels)
        {
            string got = FontOverrideIdOf(scene, node, parent);
            Assert.True(got != reject,
                $"{parent}/{node} still uses FONT4X5P, which has no lower case (issue #75)");
            Assert.Equal(want, got);
        }
    }
}
