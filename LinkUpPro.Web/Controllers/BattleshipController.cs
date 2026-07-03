using AutoMapper;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUpPro.Web.Controllers
{
    [Authorize]
    public class BattleshipController : Controller
    {
        private readonly IBattleshipService _battleshipService;

        private readonly IMapper _mapper;

        public BattleshipController(
            IBattleshipService battleshipService,
            IMapper mapper)
        {
            _battleshipService = battleshipService;
            _mapper = mapper;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private async Task<List<List<BoardCellViewModel>>> MapBoardAsync(Task<List<List<LinkUpPro.Application.DTOs.BoardCellDto>>> boardTask)
        {
            var board = await boardTask;
            return _mapper.Map<List<List<BoardCellViewModel>>>(board);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new BattleshipHomeViewModel
            {
                ActiveGames = await _battleshipService.GetActiveGamesAsync(CurrentUserId),
                History = await _battleshipService.GetHistoryAsync(CurrentUserId)
            };

            (vm.TotalGames, vm.WonGames, vm.LostGames) = await _battleshipService.GetHistorySummaryAsync(CurrentUserId);

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> NewGame(string? search)
        {
            var vm = new BattleshipNewGameViewModel
            {
                SearchText = search,
                Opponents = await _battleshipService.GetEligibleOpponentsAsync(CurrentUserId, search)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(string selectedUserId)
        {
            if (string.IsNullOrWhiteSpace(selectedUserId))
            {
                TempData["Error"] = "Debe seleccionar un usuario para iniciar la partida.";
                return RedirectToAction("NewGame");
            }

            var result = await _battleshipService.CreateGameAsync(CurrentUserId, selectedUserId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("NewGame");
            }

            return RedirectToAction("Play", new { id = result.EntityId });
        }

        [HttpGet]
        public async Task<IActionResult> Play(int id)
        {
            var state = await _battleshipService.GetGameStateAsync(id, CurrentUserId);

            if (state == null)
            {
                TempData["Error"] = "No posee permisos para acceder a esta partida.";
                return RedirectToAction("Index");
            }

            if (state.Status == "Finished")
            {
                return RedirectToAction("Result", new { id });
            }

            if (state.Status == "SettingUp" && !state.MyShipsReady)
            {
                return RedirectToAction("ShipSelection", new { id });
            }

            if (state.Status == "SettingUp" && state.MyShipsReady)
            {
                var waitingVm = new BattleshipBoardViewModel
                {
                    GameId = id,
                    OpponentUserName = state.OpponentUserName,
                    Board = await MapBoardAsync(_battleshipService.GetOwnShipsBoardAsync(id, CurrentUserId)),
                    WaitingForOpponentSetup = true
                };
                return View("Waiting", waitingVm);
            }

            var attackVm = new BattleshipBoardViewModel
            {
                GameId = id,
                OpponentUserName = state.OpponentUserName,
                Board = await MapBoardAsync(_battleshipService.GetAttackBoardAsync(id, CurrentUserId)),
                IsMyTurn = state.IsMyTurn
            };

            return View("Attack", attackVm);
        }

        [HttpGet]
        public async Task<IActionResult> MyBoard(int id)
        {
            var state = await _battleshipService.GetGameStateAsync(id, CurrentUserId);

            if (state == null)
            {
                TempData["Error"] = "No posee permisos para acceder a esta partida.";
                return RedirectToAction("Index");
            }

            var vm = new BattleshipBoardViewModel
            {
                GameId = id,
                OpponentUserName = state.OpponentUserName,
                Board = await MapBoardAsync(_battleshipService.GetOwnShipsBoardAsync(id, CurrentUserId))
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ShipSelection(int id)
        {
            var state = await _battleshipService.GetGameStateAsync(id, CurrentUserId);

            if (state == null)
            {
                TempData["Error"] = "No posee permisos para acceder a esta partida.";
                return RedirectToAction("Index");
            }

            if (state.MyShipsReady)
            {
                return RedirectToAction("Play", new { id });
            }

            var vm = new ShipSelectionViewModel
            {
                GameId = id,
                RemainingShips = await _battleshipService.GetRemainingShipsAsync(id, CurrentUserId),
                Board = await MapBoardAsync(_battleshipService.GetOwnShipsBoardAsync(id, CurrentUserId))
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> PlaceShip(int id, int length)
        {
            var state = await _battleshipService.GetGameStateAsync(id, CurrentUserId);

            if (state == null)
            {
                TempData["Error"] = "No posee permisos para acceder a esta partida.";
                return RedirectToAction("Index");
            }

            if (state.Status != "SettingUp" || state.MyShipsReady)
            {
                return RedirectToAction("Play", new { id });
            }

            var remainingShips = await _battleshipService.GetRemainingShipsAsync(id, CurrentUserId);

            if (!remainingShips.Any(s => s.Length == length))
            {
                return RedirectToAction("ShipSelection", new { id });
            }

            var vm = new PlaceShipViewModel
            {
                GameId = id,
                Length = length,
                Board = await MapBoardAsync(_battleshipService.GetOwnShipsBoardAsync(id, CurrentUserId))
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectCell(int gameId, int length, int row, int col)
        {
            return RedirectToAction("SelectDirection", new { id = gameId, length, row, col });
        }

        [HttpGet]
        public async Task<IActionResult> SelectDirection(int id, int length, int row, int col)
        {
            var state = await _battleshipService.GetGameStateAsync(id, CurrentUserId);

            if (state == null)
            {
                TempData["Error"] = "No posee permisos para acceder a esta partida.";
                return RedirectToAction("Index");
            }

            if (state.Status != "SettingUp" || state.MyShipsReady)
            {
                return RedirectToAction("Play", new { id });
            }

            var vm = new SelectDirectionViewModel
            {
                GameId = id,
                Length = length,
                Row = row,
                Col = col
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceShip(int gameId, int length, int startRow, int startCol, string direction)
        {
            var result = await _battleshipService.PlaceShipAsync(
                gameId, CurrentUserId, length, startRow, startCol, direction);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("SelectDirection", new { id = gameId, length, row = startRow, col = startCol });
            }

            TempData["Message"] = result.Message;
            return RedirectToAction("Play", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attack(int gameId, int row, int col)
        {
            var result = await _battleshipService.AttackAsync(gameId, CurrentUserId, row, col);

            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;

            return RedirectToAction("Play", new { id = gameId });
        }

        [HttpGet]
        public async Task<IActionResult> SurrenderConfirm(int id)
        {
            var state = await _battleshipService.GetGameStateAsync(id, CurrentUserId);

            if (state == null || state.Status == "Finished")
            {
                TempData["Error"] = "No posee permisos para realizar esta acción.";
                return RedirectToAction("Index");
            }

            ViewData["OpponentUserName"] = state.OpponentUserName;

            return View(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Surrender(int gameId)
        {
            var result = await _battleshipService.SurrenderAsync(gameId, CurrentUserId);

            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var result = await _battleshipService.GetResultAsync(id, CurrentUserId);

            if (result == null)
            {
                TempData["Error"] = "No posee permisos para visualizar el resultado de esta partida.";
                return RedirectToAction("Index");
            }

            return View(_mapper.Map<BattleshipResultViewModel>(result));
        }
    }
}
