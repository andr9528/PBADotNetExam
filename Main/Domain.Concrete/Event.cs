﻿using System;
using System.Collections.Generic;
using System.Text;
using Main.Domain.Core;
using Main.Domain.Enums;
using Newtonsoft.Json;

namespace Main.Domain.Concrete
{
    public class Event : IEvent
    {
        public string DatasAsString { get; set; }
        public EventStage Stage { get; set; }
        public ICollection<IRollbackData> RollbackDatas { get; set; }
        public int Id { get; set; }
        public byte[] Version { get; set; }
        public string OrderNumber { get; set; }

        [JsonConstructor]
        public Event(List<RollbackData> rollbackDatas)
        {
            RollbackDatas = new List<IRollbackData>(rollbackDatas);
        }
        

        public Event()
        {
            RollbackDatas = new List<IRollbackData>();
        }
    }
}