﻿using System;
using System.Collections.Generic;
using FishFactoryBusinessLogic.Interfaces;

namespace FishFactoryBusinessLogic.HelperModels
{
    public class MailCheckInfo
    {
        public string PopHost { get; set; }

        public int PopPort { get; set; }

        public IMessageInfoStorage MessageStorage { get; set; }

        public IClientStorage ClientStorage { get; set; }
    }
}