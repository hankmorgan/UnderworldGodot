using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Underworld.Save.Tests;

/// <summary>
/// Pins which font resource each conversation name label uses. Reads the scene as
/// text so it needs no Godot runtime. See issue #75: these labels were bound to
/// FONT4X5P, which has no lower-case glyphs, so names rendered as capitals.
/// </summary>
public class SceneFontWiringTests
{
    private static string ScenePath =>
        Path.Combine(TestData.RepoRoot, "scenes", "Underworld.tscn");

    /// <summary>The ext_resource id declared for a given res:// font path.</summary>
    private static string ExtResourceIdFor(string scene, string resPath)
    {
        var m = Regex.Match(scene,
            @"^\[ext_resource[^\]]*path=""" + Regex.Escape(resPath) + @"""[^\]]*id=""(?<id>[^""]+)""",
            RegexOptions.Multiline);
        Assert.True(m.Success, $"no ext_resource declared for {resPath}");
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

    [Fact]
    public void ConversationNameLabels_UseFont5X6P_NotFont4X5P()
    {
        string scene = File.ReadAllText(ScenePath);
        string want = ExtResourceIdFor(scene, "res://resources/fonts/FONT5X6P.ttf");
        string reject = ExtResourceIdFor(scene, "res://resources/fonts/FONT4X5P.ttf");

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
