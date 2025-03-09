using System.Text.Json;

namespace MonopolyCommon
{
    public class GameMessage
    {
        public string Type { get; set; }
        public JsonElement Data { get; set; }
    }
}
