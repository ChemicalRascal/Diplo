using BSBackupSystem.Common;
using BSBackupSystem.Data;
using BSBackupSystem.Model.Diplo;

namespace BSBackupSystem.Services;

public partial class GameReader
{
    private class DtoTranslator(DiploDataManager dataManager)
    {
        /// <summary>
        /// Adds move data to (or updates matching data in) the database.
        /// </summary>
        /// <param name="gameUrl">The base URL of the game, to identify the game.</param>
        /// <param name="blob">Extracted gamestate.</param>
        /// <param name="orders">Deserialized unit orders.</param>
        /// <returns>ID of persisted turn.</returns>
        /// <exception cref="ApplicationException">Something's wrong with the data.</exception>
        public async Task<Guid> TranslateAndPersistTurn(
            string gameUrl,
            GamestateBlob blob,
            UnitOrderDtoList orders,
            Guid? previousTurnId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(gameUrl, nameof(gameUrl));
            ArgumentNullException.ThrowIfNull(blob, nameof(blob));
            ArgumentNullException.ThrowIfNull(orders, nameof(orders));

            var moveSet = BuildMoveSet(blob, orders);
            var resultingId = await dataManager.UpsertMoveSetAsync(gameUrl, moveSet, previousTurnId);
            return resultingId;
        }

        private MoveSet BuildMoveSet(GamestateBlob blob, UnitOrderDtoList orders)
        {
            var turnId = new YearSeason(blob.Path);

            var set = new MoveSet()
            {
                Year = turnId.Year,
                SeasonName = turnId.Season,
                SeasonIndex = turnId.SeasonIndex,
                State = blob.TurnState,
                LastSeen = DateTimeOffset.UtcNow,
            };

            set.Orders.AddRange(orders.Select(UnitOrderFactory.BuildModelOrder));
            set.PreRetreatHash = set.Orders.NoRetreats().GetOrderSetHash();
            set.FullHash = set.Orders.GetOrderSetHash();

            return set;
        }

        private static class UnitOrderFactory
        {
            public static UnitOrder BuildModelOrder(UnitOrderDto dto)
            {
                ArgumentNullException.ThrowIfNull(dto, nameof(dto));

                UnitOrder order = dto.MoveType switch
                {
                    MoveType.Build => new BuildOrder() { },
                    MoveType.Disband => new DisbandOrder() { },
                    MoveType.RetreatDisband => new RetreatDisbandOrder() { },
                    MoveType.Hold => new HoldOrder() { },
                    MoveType.Move => new MoveOrder()
                    {
                        To = dto.To ?? string.Empty,
                        ToCoast = dto.ToCoast?.ToNullIfEmptyOrWhitespace(),
                    },
                    MoveType.RetreatMove => new RetreatMoveOrder()
                    {
                        To = dto.To ?? string.Empty,
                        ToCoast = dto.ToCoast?.ToNullIfEmptyOrWhitespace(),
                    },
                    MoveType.Support => (dto.From is not null && dto.To is not null)
                        ? new SupportMoveOrder()
                        {
                            SupportingFrom = dto.From ?? string.Empty,
                            SupportingTo = dto.To ?? string.Empty,
                        }
                        : new SupportHoldOrder()
                        {
                            Supporting = dto.From ?? dto.To ?? string.Empty,
                        },
                    MoveType.Convoy => new ConvoyOrder()
                    {
                        ConvoyFrom = dto.From ?? string.Empty,
                        ConvoyTo = dto.To ?? string.Empty,
                    },
                    MoveType.Unknown => throw new ApplicationException($"Unexpected outcome: {nameof(dto.MoveType)} is {dto.MoveType}."),
                    _ => throw new NotSupportedException($"{nameof(MoveType)} of {dto.MoveType} is not supported."),
                };

                order.Player = dto.Player ?? string.Empty;
                order.UnitType = dto.UnitType ?? string.Empty;
                order.Unit = dto.Unit ?? string.Empty;
                order.UnitCoast = dto.UnitCoast?.ToNullIfEmptyOrWhitespace();
                order.Result = dto.Result ?? string.Empty;
                order.ResultReason = dto.ResultReason ?? string.Empty;

                return order;
            }
        }
    }
}
