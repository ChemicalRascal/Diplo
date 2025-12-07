using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BSBackupSystem.Services;

public partial class GameReader
{
    [JsonConverter(typeof(UnitDtoConverter))]
    class UnitDto
    {
        public string? Type { get; set; }
        public string? Coast { get; set; }
    }

    private enum MoveType
    {
        Unknown,
        Build,
        Disband,
        Move,
        Hold,
        Support,
        Convoy,
        RetreatMove,
        RetreatDisband,
    }

    class UnitOrderDto
    {
        public string? Player { get; set; }

        public string? UnitType { get; set; }
        public string? Unit { get; set; }
        public string? UnitCoast { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MoveType MoveType { get; set; }

        public string? To { get; set; }
        public string? ToCoast { get; set; }

        public string? From { get; set; }

        public string? Result { get; set; }
        public string? ResultReason { get; set; }

        public UnitOrderDto? Retreat { get; set; }
    }

    [JsonConverter(typeof(UnitOrderDtoListConverter))]
    class UnitOrderDtoList : List<UnitOrderDto>
    {
        public static UnitOrderDtoList FromEnumerable(IEnumerable<UnitOrderDto> enumerable)
        {
            ArgumentNullException.ThrowIfNull(enumerable, nameof(enumerable));
            var newList = new UnitOrderDtoList();
            newList.AddRange(enumerable);
            return newList;
        }
    }

    private static class BlobDeserializer
    {
        public static UnitOrderDtoList GetOrderDtos(GamestateBlob blob)
        {
            Dictionary<string, UnitDto>? units = null;
            var properUnitBlob = blob.UnitLine[blob.UnitLine.IndexOf('{')..^1];
            units = JsonSerializer
                .Deserialize<Dictionary<string, Dictionary<string, UnitDto>>>(properUnitBlob)
                ?.SelectMany(kvp => kvp.Value).ToDictionary();

            var orderBlob = blob.OrderLine[blob.OrderLine.IndexOf('{')..^1];

            var ordersByCountry = JsonSerializer.Deserialize<Dictionary<string, UnitOrderDtoList>>(
                orderBlob,
                options: new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }) ?? [];

            // We still need to do a chunk of stuff to make each order fully-realized.
            return UnitOrderDtoList.FromEnumerable(ordersByCountry.SelectMany(
                kvp => kvp.Value.Select(o => (Order: o, Player: kvp.Key)))
                .Select(orderPlayer =>
                {
                    var (order, player) = orderPlayer;
                    order.Player = player;
                    if (order!.MoveType != MoveType.Build)
                    {
                        var unit = units?[order!.Unit!];
                        order.UnitType ??= unit?.Type;
                        order.UnitCoast ??= unit?.Coast;
                    }
                    return order;
                }));
        }
    }

    class UnitDtoConverter : JsonConverter<UnitDto>
    {
        public override UnitDto? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new UnitDto()
                {
                    Type = reader.GetString(),
                    Coast = null,
                };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var unitDict = JsonDocument.ParseValue(ref reader).RootElement;
                return new UnitDto()
                {
                    Type = unitDict.GetProperty("type").ToString(),
                    Coast = unitDict.GetProperty("coast").ToString(),
                };
            }

            var tokenType = reader.TokenType;
            var line = JsonDocument.ParseValue(ref reader).RootElement.ToString();
            throw new NotImplementedException($"Cannot parse {tokenType} : {line}");
        }

        public override void Write(
            Utf8JsonWriter writer,
            UnitDto value,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }

    class UnitOrderDtoListConverter : JsonConverter<UnitOrderDtoList>
    {
        public override UnitOrderDtoList? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var orderDictionary = JsonDocument.ParseValue(ref reader).RootElement.ToString();
            var orderList = new UnitOrderDtoList();
            var enumeratedOrders = JsonSerializer.Deserialize<Dictionary<string, UnitOrderDto>>(
                orderDictionary, options: new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            foreach (var order in enumeratedOrders!)
            {
                order.Value.Unit = order.Key;
                orderList.Add(order.Value);
                if (order.Value.Retreat is not null)
                {
                    if (order.Value.Retreat.MoveType == MoveType.Move)
                    {
                        order.Value.Retreat.MoveType = MoveType.RetreatMove;
                    }
                    else if (order.Value.Retreat.MoveType == MoveType.Disband)
                    {
                        order.Value.Retreat.MoveType = MoveType.RetreatDisband;
                    }
                    order.Value.Retreat.Unit = order.Key;
                    orderList.Add(order.Value.Retreat);
                }
            }
            return orderList;
        }

        public override void Write(
            Utf8JsonWriter writer,
            UnitOrderDtoList value,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

    }
}
