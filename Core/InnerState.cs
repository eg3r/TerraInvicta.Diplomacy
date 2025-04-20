using System.Collections.Generic;
using Diplomacy.Core.Treaty;
using Newtonsoft.Json;

namespace Diplomacy.Core;

/// <summary>
/// Represents the internal state of the mod, used for saved data.
/// </summary>
internal sealed class InnerState
{
    [JsonProperty] public const string SaveVersion = "1.0.0";
    [JsonProperty] public List<DiplomacyTreaty> Treaties = new();
}