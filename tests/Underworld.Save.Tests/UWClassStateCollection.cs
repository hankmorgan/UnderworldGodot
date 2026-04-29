using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// xUnit collection marker: serialises tests that mutate UWClass static state
/// (BasePath, _RES, playerdat.pdat, etc.) so they don't race across test classes.
/// </summary>
[CollectionDefinition("UWClassState")]
public class UWClassStateCollection { }
