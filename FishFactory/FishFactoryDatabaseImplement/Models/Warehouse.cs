﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FishFactoryDatabaseImplement.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        public string WarehouseName { get; set; }

        [Required]
        public string Responsible { get; set; }

        [Required]
        public DateTime DateCreate { get; set; }

        [ForeignKey("WarehouseId")]
        public List<WarehouseComponent> WarehouseComponent { get; set; }
    }
}