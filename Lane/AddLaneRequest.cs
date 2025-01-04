using System.ComponentModel.DataAnnotations;

namespace htmx_trello.Lane;

public record AddLaneRequest([Required] string LaneName);