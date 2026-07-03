using LinkUpPro.Application.DTOs;

namespace LinkUpPro.Application.ViewModels
{
    public class FriendRequestsViewModel
    {
        public List<FriendRequestDto> Received { get; set; } = new();

        public List<FriendRequestDto> Sent { get; set; } = new();

        // Solicitudes que el usuario autenticado recibió y ya respondió
        // (aceptadas/rechazadas), para que también pueda ocultarlas de su
        // propio historial, igual que puede hacerlo el emisor.
        public List<FriendRequestDto> ReceivedHistory { get; set; } = new();
    }
}
