﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FishFactoryBusinessLogic.ViewModels
{
    public class WarehouseViewModel
    {
        public int Id { get; set; }

        [DisplayName("Название склада")]
        public string WarehouseName { get; set; }

        [DisplayName("ФИО ответственного")]
        public string Responsible { get; set; }

        [DisplayName("Дата создания")]
        public DateTime DateCreate { get; set; }

        public Dictionary<int, (string, int)> WarehouseComponents { get; set; } // Id, (ComponentName, count)
    }
}