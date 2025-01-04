using System.ComponentModel.DataAnnotations;

namespace htmello.Lane;

public record AddLaneRequest([Required] string LaneName);