﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DO;
using DAL;
namespace DAL
{
    internal class Config
    {
        internal static double free { get { return 5; } }//כמה בטריה לקילומטר כשהוא לא סוחב כלום
        internal static double light { get { return 7; } }
        internal static double medium { get { return 10; } }
        internal static double heavy { get { return 12; } }
        internal static int ChargePerHour { get { return 50; } } // 50 אחוז בשעה
        public static int staticId = 1;
    }
    public class DataSource
    {

        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        internal static List<Drone> drones = new();
        internal static List<DroneCharge> droneCharges = new();
        internal static List<Station> stations = new();
        internal static List<Customer> customers = new();
        internal static List<Parcel> parcels = new();
        public static void Initialize()
        {
            Random rnd = new Random();
            for (int i = 0; i < 2; i++)
            {
                Station s = new Station()
                {
                    IsActive = true,
                    ID = Config.staticId,
                    StationName = "Station" +i,
                    Longitude = GetRandomNumber(15, 0),
                    Lattitude = GetRandomNumber(17, 0),
                    ChargeSlots = 5,
                };
                Config.staticId++;
                stations.Add(s);
            }
            for (int i = 0; i < 5; i++)
            {       
                int counter= rnd.Next(0, 2);
                DroneCharge temp = new()
                {
                    DroneId = Config.staticId,
                    StationId = (int)stations[counter].ID,
                };
                int index = stations.FindIndex(i => i.ID == temp.StationId);
                Station station = stations[index];
                station.BusyChargeSlots++;
                stations[index] = station;
                Drone d = new Drone()
                {
                    IsActive = true,
                    ID = Config.staticId,
                    Model = ""+(Model)rnd.Next(0, 3),
                    Battery = 100,
                    haveParcel = false,
                    Lattitude = stations[counter].Lattitude,
                    Longitude = stations[counter].Longitude,
                };
                droneCharges.Add(temp);
                drones.Add(d);
                Config.staticId++;
            }

            
            for (int i = 0; i < 10; i++)
            {
                Customer c = new Customer()
                {
                    ID = Config.staticId,
                    CustomerName = "Customer" + i,
                    Phone = "05" + rnd.Next(10000000, 99999999),
                    Longitude = GetRandomNumber(33.289273, 29.494665),
                    Lattitude = GetRandomNumber(35.569495, 34.904675),
                    IsActive = true,
                };
                Config.staticId++;
                customers.Add(c);
            }
            for (int i = 0; i < 10; i++)
            {
                int sID = rnd.Next(0, 10);
                int tID = rnd.Next(0, 10);
                while (sID == tID)
                {
                    tID = rnd.Next(0, 10);
                }
                while (tID==sID)
                {
                    tID = rnd.Next(0, 10);
                }

                Parcel p = new Parcel()
                {
                    IsActive = true,
                    ID = Config.staticId,
                    SenderId = (int)customers[sID].ID,
                    TargetId = (int)customers[tID].ID,
                    Weight = (Weight)rnd.Next(0, 3),
                    Priority = (Priority)rnd.Next(0, 3),
                    Requested = DateTime.Now,
                    Scheduled = null,
                    PickedUp = null,
                    Deliverd = null,
                    Status = StatusParcel.CREAT,
                };
                int temp = rnd.Next(0, 3);
                if (temp == 0)
                {
                    temp = rnd.Next(0, drones.Count);
                    if (parcels.TrueForAll(i => i.DroneId != temp))
                    {
                        p.DroneId = (int)drones[temp].ID;
                        int index = drones.FindIndex(i => i.ID == p.DroneId);
                        Drone drone = drones[index];
                        drone.Status = Status.BELONG;
                        drones[index] = drone;
                    }
                    else
                        p.DroneId = 0;

                }

                Config.staticId++;
                parcels.Add(p);
            }
            //for (int i = 0; i < droneCharges.Count; i++)
            //{
            //    Station s = stations.Find(delegate (Station p) { return droneCharges[i].StationId == p.ID; });
            //    s.ChargeSlots--;
            //    stations.Remove(stations.Find(delegate (Station p) { return droneCharges[i].StationId == p.ID; }));
            //    stations.Add(s);
            //}
        }
    }
}
