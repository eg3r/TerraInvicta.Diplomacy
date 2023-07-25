using System.Collections.Generic;
using Newtonsoft.Json;

namespace Diplomacy.Core;

internal sealed class InnerState
{
    [JsonProperty] public const string SaveVersion = "1.0.0";
    [JsonProperty] public List<DiplomacyTreaty> Treaties = new();
}