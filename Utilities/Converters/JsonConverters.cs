using System.Text.Json;
using System.Text.Json.Serialization;

namespace RegistryRepairTool.Utilities
{
    public class RelayCommandConverter : JsonConverter<RelayCommand>
    {
        public override RelayCommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null; // Команды не сериализуем
        }

        public override void Write(Utf8JsonWriter writer, RelayCommand value, JsonSerializerOptions options)
        {
            writer.WriteNullValue(); // Команды не сериализуем
        }
    }
}