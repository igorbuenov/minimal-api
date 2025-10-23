﻿using System.ComponentModel.DataAnnotations;

namespace minimal_api.Domain.DTOs
{
    public record VeiculoDto
    {
        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Marca { get; set; } = default!;

        [Required]
        public int Ano { get; set; } = default!;
    }
}
