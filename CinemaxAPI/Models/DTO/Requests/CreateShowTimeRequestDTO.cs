using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class CreateShowTimeRequestDTO
    {
        [Required]
        public int MovieId { get; set; }
        [Required]
        public int ScreenId { get; set; }
        [Required]
        public List<ShowTimeData> ShowTimes { get; set; }

        [Required]
        public double TicketPrice { get; set; }
    }

    public class ShowTimeData
    {
        public DateTime Date { get; set; }
        public List<string> StartTimes { get; set; }
        public List<string> EndTimes { get; set; }
    }
}