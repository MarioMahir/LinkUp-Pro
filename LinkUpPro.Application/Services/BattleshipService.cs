using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Application.Services
{
    public class BattleshipService : IBattleshipService
    {
        private const int BoardSize = 12;

        private static readonly int[] RequiredShipLengths = { 2, 3, 3, 4, 5 };

        private readonly IBattleshipRepository _battleshipRepository;

        private readonly IGenericService<Ship> _shipService;

        private readonly IGenericService<Attack> _attackService;

        private readonly IFriendshipRepository _friendshipRepository;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IMapper _mapper;

        public BattleshipService(
            IBattleshipRepository battleshipRepository,
            IGenericService<Ship> shipService,
            IGenericService<Attack> attackService,
            IFriendshipRepository friendshipRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _battleshipRepository = battleshipRepository;
            _shipService = shipService;
            _attackService = attackService;
            _friendshipRepository = friendshipRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        private static ServiceResult Fail(string message) =>
            new() { Succeeded = false, Message = message };

        private static List<List<BoardCellDto>> CreateEmptyBoard()
        {
            var board = new List<List<BoardCellDto>>();

            for (var r = 0; r < BoardSize; r++)
            {
                var row = new List<BoardCellDto>();
                for (var c = 0; c < BoardSize; c++)
                {
                    row.Add(new BoardCellDto { Row = r, Col = c });
                }
                board.Add(row);
            }

            return board;
        }

        private static void MarkSunkShips(
            List<List<BoardCellDto>> board,
            IEnumerable<Ship> ships,
            HashSet<(int Row, int Col)> hitCells)
        {
            foreach (var ship in ships)
            {
                var occupied = ship.GetOccupiedCells().ToList();

                if (occupied.Count == 0 || !occupied.All(hitCells.Contains))
                {
                    continue;
                }

                foreach (var (row, col) in occupied)
                {
                    if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize)
                    {
                        board[row][col].IsSunk = true;
                    }
                }
            }
        }

        private async Task CheckForfeitAsync(BattleshipGame game)
        {
            if (game.Status != "Attacking" || game.CurrentTurnUserId == null || game.TurnAssignedDate == null)
            {
                return;
            }

            if (DateTime.UtcNow - game.TurnAssignedDate.Value < TimeSpan.FromHours(48))
            {
                return;
            }

            game.Status = "Finished";
            game.WinnerId = game.CurrentTurnUserId == game.PlayerOneId ? game.PlayerTwoId : game.PlayerOneId;
            game.FinishReason = "Forfeited";
            game.FinishedDate = DateTime.UtcNow;

            _battleshipRepository.Update(game);
            await _battleshipRepository.SaveChangesAsync();
        }

        public async Task<List<BattleshipGameSummaryDto>> GetActiveGamesAsync(string userId)
        {
            var games = await _battleshipRepository.GetActiveByUserAsync(userId);
            var result = new List<BattleshipGameSummaryDto>();

            foreach (var g in games)
            {
                await CheckForfeitAsync(g);

                if (g.Status == "Finished")
                {
                    continue;
                }

                var opponent = g.PlayerOneId == userId ? g.PlayerTwo : g.PlayerOne;

                result.Add(new BattleshipGameSummaryDto
                {
                    Id = g.Id,
                    OpponentId = opponent.Id,
                    OpponentUserName = opponent.UserName ?? string.Empty,
                    StartedDate = g.CreatedDate,
                    HoursElapsed = Math.Round((DateTime.UtcNow - g.CreatedDate).TotalHours, 1),
                    Status = g.Status,
                    IsMyTurn = g.Status == "Attacking" && g.CurrentTurnUserId == userId
                });
            }

            return result;
        }

        public async Task<List<BattleshipGameSummaryDto>> GetHistoryAsync(string userId)
        {
            await GetActiveGamesAsync(userId);

            var games = await _battleshipRepository.GetHistoryByUserAsync(userId);

            return games.Select(g =>
            {
                var opponent = g.PlayerOneId == userId ? g.PlayerTwo : g.PlayerOne;
                var won = g.WinnerId == userId;

                return new BattleshipGameSummaryDto
                {
                    Id = g.Id,
                    OpponentId = opponent.Id,
                    OpponentUserName = opponent.UserName ?? string.Empty,
                    StartedDate = g.CreatedDate,
                    FinishedDate = g.FinishedDate,
                    HoursElapsed = g.FinishedDate.HasValue
                        ? Math.Round((g.FinishedDate.Value - g.CreatedDate).TotalHours, 1)
                        : 0,
                    Status = g.Status,
                    Won = won,
                    WinnerDisplay = won ? "Yo" : (opponent.UserName ?? string.Empty)
                };
            }).ToList();
        }

        public async Task<(int Total, int Won, int Lost)> GetHistorySummaryAsync(string userId)
        {
            var history = await GetHistoryAsync(userId);
            var won = history.Count(h => h.Won);
            return (history.Count, won, history.Count - won);
        }

        public async Task<List<FriendDto>> GetEligibleOpponentsAsync(string userId, string? search)
        {
            var friendships = await _friendshipRepository.GetActiveByUserAsync(userId);
            var result = new List<FriendDto>();

            foreach (var f in friendships)
            {
                var other = f.UserOneId == userId ? f.UserTwo : f.UserOne;

                if (other == null || !other.EmailConfirmed)
                {
                    continue;
                }

                var activeGame = await _battleshipRepository.GetActiveBetweenAsync(userId, other.Id);
                if (activeGame != null)
                {
                    continue;
                }

                result.Add(_mapper.Map<FriendDto>(other));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                result = result.Where(x => x.UserName.ToLowerInvariant().Contains(term)).ToList();
            }

            return result.OrderBy(x => x.FirstName).ToList();
        }

        public async Task<ServiceResult> CreateGameAsync(string userId, string opponentId)
        {
            if (userId == opponentId)
            {
                return Fail("No puede iniciar una partida consigo mismo.");
            }

            var opponent = await _userManager.FindByIdAsync(opponentId);

            if (opponent == null || !opponent.EmailConfirmed)
            {
                return Fail("El usuario seleccionado ya no se encuentra disponible.");
            }

            var friendship = await _friendshipRepository.GetBetweenAsync(userId, opponentId);

            if (friendship is not { IsActive: true })
            {
                return Fail("Debe ser amigo del usuario para iniciar una partida.");
            }

            var existing = await _battleshipRepository.GetActiveBetweenAsync(userId, opponentId);

            if (existing != null)
            {
                return Fail("Ya existe una partida activa con este usuario.");
            }

            var game = new BattleshipGame
            {
                PlayerOneId = userId,
                PlayerTwoId = opponentId,
                Status = "SettingUp",
                PairKey = FriendRequest.MakePairKey(userId, opponentId),
                CreatedDate = DateTime.UtcNow
            };

            try
            {
                await _battleshipRepository.AddAsync(game);
                await _battleshipRepository.SaveChangesAsync();
            }
            catch (LinkUpPro.Application.Exceptions.ConcurrencyConflictException)
            {
                return Fail("Ya existe una partida activa con este usuario.");
            }

            return new ServiceResult { Succeeded = true, EntityId = game.Id };
        }

        public async Task<ServiceResult> SurrenderAsync(int gameId, string userId)
        {
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null || (game.PlayerOneId != userId && game.PlayerTwoId != userId))
            {
                return Fail("No posee permisos para realizar esta acción.");
            }

            if (game.Status == "Finished")
            {
                return Fail("Esta partida ya ha finalizado.");
            }

            game.Status = "Finished";
            game.WinnerId = game.PlayerOneId == userId ? game.PlayerTwoId : game.PlayerOneId;
            game.FinishReason = "Surrendered";
            game.FinishedDate = DateTime.UtcNow;

            _battleshipRepository.Update(game);
            await _battleshipRepository.SaveChangesAsync();

            return new ServiceResult { Succeeded = true, Message = "Se ha rendido. La partida ha finalizado." };
        }

        public async Task<BattleshipGameStateDto?> GetGameStateAsync(int gameId, string userId)
        {
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null || (game.PlayerOneId != userId && game.PlayerTwoId != userId))
            {
                return null;
            }

            await CheckForfeitAsync(game);

            var opponent = game.PlayerOneId == userId ? game.PlayerTwo : game.PlayerOne;
            var isPlayerOne = game.PlayerOneId == userId;

            return new BattleshipGameStateDto
            {
                GameId = game.Id,
                OpponentUserName = opponent.UserName ?? string.Empty,
                Status = game.Status,
                MyShipsReady = isPlayerOne ? game.PlayerOneShipsReady : game.PlayerTwoShipsReady,
                OpponentShipsReady = isPlayerOne ? game.PlayerTwoShipsReady : game.PlayerOneShipsReady,
                IsMyTurn = game.Status == "Attacking" && game.CurrentTurnUserId == userId,
                WinnerId = game.WinnerId,
                Won = game.WinnerId == userId
            };
        }

        public async Task<List<ShipOptionDto>> GetRemainingShipsAsync(int gameId, string userId)
        {
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null)
            {
                return new List<ShipOptionDto>();
            }

            var placedLengths = game.Ships.Where(s => s.OwnerId == userId).Select(s => s.Length).ToList();
            var remaining = new List<int>(RequiredShipLengths);

            foreach (var length in placedLengths)
            {
                remaining.Remove(length);
            }

            return remaining
                .OrderBy(l => l)
                .Select(l => new ShipOptionDto { Length = l, Label = $"Barco de {l} posiciones" })
                .ToList();
        }

        public async Task<List<List<BoardCellDto>>> GetOwnShipsBoardAsync(int gameId, string userId)
        {
            var board = CreateEmptyBoard();
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null)
            {
                return board;
            }

            var myShips = game.Ships.Where(s => s.OwnerId == userId).ToList();

            foreach (var ship in myShips)
            {
                foreach (var (row, col) in ship.GetOccupiedCells())
                {
                    if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize)
                    {
                        board[row][col].HasShip = true;
                    }
                }
            }

            var opponentId = game.PlayerOneId == userId ? game.PlayerTwoId : game.PlayerOneId;
            var opponentHitCells = game.Attacks
                .Where(a => a.AttackerId == opponentId && a.WasHit)
                .Select(a => (a.Row, a.Col))
                .ToHashSet();

            MarkSunkShips(board, myShips, opponentHitCells);

            return board;
        }

        public async Task<ServiceResult> PlaceShipAsync(
            int gameId, string userId, int length, int startRow, int startCol, string direction)
        {
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null || (game.PlayerOneId != userId && game.PlayerTwoId != userId))
            {
                return Fail("No posee permisos para realizar esta acción.");
            }

            if (game.Status != "SettingUp")
            {
                return Fail("Esta partida ya no se encuentra en fase de posicionamiento.");
            }

            var isPlayerOne = game.PlayerOneId == userId;
            var alreadyReady = isPlayerOne ? game.PlayerOneShipsReady : game.PlayerTwoShipsReady;

            if (alreadyReady)
            {
                return Fail("Ya ha completado el posicionamiento de sus barcos.");
            }

            if (!RequiredShipLengths.Contains(length))
            {
                return Fail("Debe seleccionar un barco válido.");
            }

            var placedLengths = game.Ships.Where(s => s.OwnerId == userId).Select(s => s.Length).ToList();
            var remaining = new List<int>(RequiredShipLengths);
            foreach (var l in placedLengths) remaining.Remove(l);

            if (!remaining.Contains(length))
            {
                return Fail("Este barco ya fue posicionado.");
            }

            if (direction is not ("Up" or "Down" or "Left" or "Right"))
            {
                return Fail("Debe seleccionar una dirección válida.");
            }

            var candidate = new Ship
            {
                Length = length,
                StartRow = startRow,
                StartCol = startCol,
                Direction = direction,
                OwnerId = userId,
                GameId = gameId
            };

            var cells = candidate.GetOccupiedCells().ToList();

            if (cells.Any(c => c.Row < 0 || c.Row >= BoardSize || c.Col < 0 || c.Col >= BoardSize))
            {
                return Fail("Debe cambiar la celda seleccionada o la dirección, ya que con la combinación actual el barco quedaría fuera del tablero.");
            }

            var myOccupied = game.Ships
                .Where(s => s.OwnerId == userId)
                .SelectMany(s => s.GetOccupiedCells())
                .ToHashSet();

            if (cells.Any(c => myOccupied.Contains(c)))
            {
                return Fail("Debe cambiar la celda seleccionada o la dirección, ya que con la combinación actual el barco quedaría posicionado encima de otro barco.");
            }

            try
            {
                await _shipService.AddAsync(candidate);
            }
            catch (LinkUpPro.Application.Exceptions.ConcurrencyConflictException)
            {
                return Fail("Este barco ya fue posicionado.");
            }

            var totalPlaced = placedLengths.Count + 1;

            if (totalPlaced == RequiredShipLengths.Length)
            {
                if (isPlayerOne)
                {
                    game.PlayerOneShipsReady = true;
                }
                else
                {
                    game.PlayerTwoShipsReady = true;
                }

                if (game.PlayerOneShipsReady && game.PlayerTwoShipsReady)
                {
                    game.Status = "Attacking";
                    game.CurrentTurnUserId = game.PlayerOneId;
                    game.TurnAssignedDate = DateTime.UtcNow;
                }

                _battleshipRepository.Update(game);
                await _battleshipRepository.SaveChangesAsync();
            }

            return new ServiceResult { Succeeded = true, Message = "El barco fue posicionado correctamente." };
        }

        public async Task<List<List<BoardCellDto>>> GetAttackBoardAsync(int gameId, string userId)
        {
            var board = CreateEmptyBoard();
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null)
            {
                return board;
            }

            foreach (var a in game.Attacks.Where(a => a.AttackerId == userId))
            {
                if (a.Row >= 0 && a.Row < BoardSize && a.Col >= 0 && a.Col < BoardSize)
                {
                    board[a.Row][a.Col].IsAttacked = true;
                    board[a.Row][a.Col].WasHit = a.WasHit;
                }
            }

            var opponentId = game.PlayerOneId == userId ? game.PlayerTwoId : game.PlayerOneId;
            var opponentShips = game.Ships.Where(s => s.OwnerId == opponentId);
            var myHitCells = game.Attacks
                .Where(a => a.AttackerId == userId && a.WasHit)
                .Select(a => (a.Row, a.Col))
                .ToHashSet();

            MarkSunkShips(board, opponentShips, myHitCells);

            return board;
        }

        public async Task<ServiceResult> AttackAsync(int gameId, string userId, int row, int col)
        {
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null || (game.PlayerOneId != userId && game.PlayerTwoId != userId))
            {
                return Fail("No posee permisos para realizar esta acción.");
            }

            await CheckForfeitAsync(game);

            if (game.Status == "Finished")
            {
                return Fail("Esta partida ya ha finalizado.");
            }

            if (game.Status != "Attacking")
            {
                return Fail("La partida todavía no se encuentra en la fase de ataque.");
            }

            if (game.CurrentTurnUserId != userId)
            {
                return Fail("No es su turno de atacar.");
            }

            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                return Fail("Celda inválida.");
            }

            if (game.Attacks.Any(a => a.AttackerId == userId && a.Row == row && a.Col == col))
            {
                return Fail("Esta celda ya fue atacada.");
            }

            var opponentId = game.PlayerOneId == userId ? game.PlayerTwoId : game.PlayerOneId;
            var opponentCells = game.Ships
                .Where(s => s.OwnerId == opponentId)
                .SelectMany(s => s.GetOccupiedCells())
                .ToHashSet();

            var wasHit = opponentCells.Contains((row, col));

            try
            {
                await _attackService.AddAsync(new Attack
                {
                    GameId = gameId,
                    AttackerId = userId,
                    Row = row,
                    Col = col,
                    WasHit = wasHit,
                    CreatedDate = DateTime.UtcNow
                });
            }
            catch (LinkUpPro.Application.Exceptions.ConcurrencyConflictException)
            {
                return Fail("Esta celda ya fue atacada.");
            }

            var allMyHits = (await _attackService.FindAsync(
                    a => a.GameId == gameId && a.AttackerId == userId && a.WasHit))
                .Select(a => (a.Row, a.Col))
                .ToHashSet();

            var won = opponentCells.Count > 0 && opponentCells.All(c => allMyHits.Contains(c));

            if (won)
            {
                game.Status = "Finished";
                game.WinnerId = userId;
                game.FinishReason = "Won";
                game.FinishedDate = DateTime.UtcNow;
            }
            else
            {
                game.CurrentTurnUserId = opponentId;
                game.TurnAssignedDate = DateTime.UtcNow;
            }

            _battleshipRepository.Update(game);
            await _battleshipRepository.SaveChangesAsync();

            return new ServiceResult
            {
                Succeeded = true,
                Message = won ? "¡Ha hundido todos los barcos! Ganó la partida." : (wasHit ? "¡Impacto!" : "Agua.")
            };
        }

        public async Task<BattleshipResultDto?> GetResultAsync(int gameId, string userId)
        {
            var game = await _battleshipRepository.GetByIdWithDetailsAsync(gameId);

            if (game == null ||
                (game.PlayerOneId != userId && game.PlayerTwoId != userId) ||
                game.Status != "Finished")
            {
                return null;
            }

            var opponentId = game.PlayerOneId == userId ? game.PlayerTwoId : game.PlayerOneId;
            var opponent = game.PlayerOneId == userId ? game.PlayerTwo : game.PlayerOne;

            var myAttackBoard = CreateEmptyBoard();
            foreach (var a in game.Attacks.Where(a => a.AttackerId == userId))
            {
                myAttackBoard[a.Row][a.Col].IsAttacked = true;
                myAttackBoard[a.Row][a.Col].WasHit = a.WasHit;
            }

            var opponentAttackBoard = CreateEmptyBoard();
            foreach (var a in game.Attacks.Where(a => a.AttackerId == opponentId))
            {
                opponentAttackBoard[a.Row][a.Col].IsAttacked = true;
                opponentAttackBoard[a.Row][a.Col].WasHit = a.WasHit;
            }

            var myShips = game.Ships.Where(s => s.OwnerId == userId).ToList();
            var opponentShips = game.Ships.Where(s => s.OwnerId == opponentId).ToList();

            var myShipsBoard = CreateEmptyBoard();
            foreach (var ship in myShips)
            {
                foreach (var (r, c) in ship.GetOccupiedCells())
                {
                    if (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize)
                    {
                        myShipsBoard[r][c].HasShip = true;
                    }
                }
            }

            var myHitCells = game.Attacks
                .Where(a => a.AttackerId == userId && a.WasHit)
                .Select(a => (a.Row, a.Col))
                .ToHashSet();

            var opponentHitCells = game.Attacks
                .Where(a => a.AttackerId == opponentId && a.WasHit)
                .Select(a => (a.Row, a.Col))
                .ToHashSet();

            MarkSunkShips(myAttackBoard, opponentShips, myHitCells);
            MarkSunkShips(opponentAttackBoard, myShips, opponentHitCells);
            MarkSunkShips(myShipsBoard, myShips, opponentHitCells);

            return new BattleshipResultDto
            {
                GameId = game.Id,
                OpponentUserName = opponent.UserName ?? string.Empty,
                Won = game.WinnerId == userId,
                MyAttackBoard = myAttackBoard,
                OpponentAttackBoard = opponentAttackBoard,
                MyShipsBoard = myShipsBoard
            };
        }
    }
}
