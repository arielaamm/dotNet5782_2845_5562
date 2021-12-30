﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.BO
{
    public class Station
    {
        public int ID { set; get; }
        public string StationName { set; get; }
        public Location location { set; get; }
        public int FreeChargeSlots { set; get; }
        public DroneCharging DroneChargingInStation { set; get; }
    }
}
