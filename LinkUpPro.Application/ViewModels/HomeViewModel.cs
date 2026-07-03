using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Application.ViewModels
{
    public class HomeViewModel
    {
        public string? SearchText { get; set; }

        public string? ContentType { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? EditedStatus { get; set; }

        public SavePostViewModel NewPost { get; set; } = new();

        public List<PostViewModel> Posts { get; set; } = new();

        public int PendingRequestsCount { get; set; }

        public int UnreadNotificationsCount { get; set; }

        // Indica si el usuario aplicó algún criterio de búsqueda/filtro, para
        // poder distinguir "todavía no ha publicado nada" de "no hay resultados
        // para este filtro" en la vista.
        public bool HasFilter { get; set; }
    }
}