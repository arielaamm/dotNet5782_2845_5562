﻿using BLExceptions;
using DAL;
using IBL.BO;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DateTime = System.DateTime;
namespace BL
{
    public class BL : IBL.IBL
    {
        IDal dal = new DalObject();
        public BL()
        {
            //ניתן להעזר ראה ParcelNotAssociatedList() בגל אובגקט
            IEnumerable<IDAL.DO.Parcel> p = dal.Parcellist();
            List<IDAL.DO.Drone> d = DataSource.drones;

            foreach (IDAL.DO.Parcel i in p)
            {
                Drone tempDrone = new Drone();
                if ((i.Scheduled != DateTime.MinValue) && (i.Deliverd == DateTime.MinValue) && (i.DroneId != 0))//Deliverd==0???
                {
                    tempDrone = findDrone(i.SenderId);
                    findDrone(i.SenderId).Status = (STATUS)2;
                    if ((i.PickedUp == DateTime.MinValue) && (i.Scheduled != DateTime.MinValue))
                    {//shortest station
                        Location sta = new();
                        foreach (IDAL.DO.Station item in dal.Stationlist())
                        {
                            if (Distans(findDrone(i.SenderId).current, findDrone((int)i.ID).current) > Distans(findDrone((int)i.ID).current, sta))
                            {
                                sta = findDrone(i.SenderId).current;
                            }
                        }
                    }
                    if ((i.Deliverd == DateTime.MinValue) && (i.PickedUp != DateTime.MinValue))
                    {
                        findDrone(i.SenderId).current = findDrone(i.SenderId).current;
                    }
                    Random random = new Random();
                    if (i.Scheduled == DateTime.MinValue)
                    {
                        if (random.Next(1, 2) == 1)
                        {
                            findDrone(i.SenderId).Status = STATUS.FREE;
                            List<Parcel> pa = new();
                            foreach (var item in parcels())
                            {
                                pa = pa.FindAll(delegate (Parcel p) { return (p.Deliverd != DateTime.MinValue); });//לקוח שקיבל חבילה
                            }
                            findDrone(i.SenderId).current = findcustomer(pa[random.Next(0, pa.Count - 1)].target.ID).location;//מספר רנדומלי מתוך כל הלקוחות שקיבלו חבילה בו אני מחפש את האיידיי של המקבל שם בחיפוש לקוח ולוקח ממנו את המיקום
                            findDrone(i.SenderId).Buttery = random.Next(20, 100);

                        }
                        else
                        {
                            List<Station> s = new();
                            findDrone(i.SenderId).current = FreeChargeslots().ToList()[random.Next(0, (FreeChargeslots().Count()) - 1)].location;
                            findDrone(i.SenderId).Buttery = random.Next(0, 20);
                            findDrone(i.SenderId).Status = STATUS.MAINTENANCE;
                        }

                    }
                }
            }
        }
        public double Distans(Location a, Location b)
        {
            return Math.Sqrt((Math.Pow(a.Lattitude - b.Lattitude, 2) + Math.Pow(a.Longitude - b.Longitude, 2)));
        }
        //add functaions:
        //---------------------------------------------------------------------------------
        public void AddStation(int id, string name, Location location, int ChargeSlots)
        {

            IDAL.DO.Station tempStation = new IDAL.DO.Station()
            {
                ID = id,
                StationName = name,
                Longitude = location.Longitude,
                Lattitude = location.Lattitude,
                ChargeSlots = ChargeSlots,
            };
            try
            {
                dal.AddStation(tempStation);
            }
            catch (Exception ex)
            {
                throw new AlreadyExistException($"{ex.Message}");
            }
        }
        public void AddDrone(int id, string name, IBL.BO.WEIGHT weight, int IDStarting)
        {
            try
            {
                Random random = new Random();
                IDAL.DO.Drone tempDrone = new IDAL.DO.Drone()
                {
                    ID = id,
                    Model = name,
                    Weight = (IDAL.DO.WEIGHT)weight,
                    Buttery = random.Next(20, 40),
                    haveParcel = true,
                };
                IDAL.DO.DroneCharge tempDroneCharge = new IDAL.DO.DroneCharge()
                {
                    DroneId = id,
                    StationId = IDStarting,
                };
                findStation(IDStarting).FreeChargeSlots--;
                dal.AddDrone(tempDrone);
                dal.AddDroneCharge(tempDroneCharge);
            }
            catch (Exception ex)
            {
                throw new AlreadyExistException(ex.Message, ex);
            }

        }
        public void AddCustomer(int id, string name, string PhoneNumber, Location location)
        {
            try
            {
                IDAL.DO.Customer tempCustomer = new IDAL.DO.Customer()
                {
                    ID = id,
                    CustomerName = name,
                    Longitude = location.Longitude,
                    Lattitude = location.Lattitude,
                    Phone = PhoneNumber,
                };

                dal.AddCustomer(tempCustomer);
            }
            catch (Exception ex)
            {
                throw new AlreadyExistException(ex.Message, ex);
            }
        }
        public void AddParcel(int SenderId, int TargetId, IBL.BO.WEIGHT weight, IBL.BO.PRIORITY Priority)
        {
            int? dID = null;
            foreach (var item in dal.Dronelist())
            {
                if (!(item.haveParcel))
                {
                    dID = item.ID;
                }
            }
            try
            {
                IDAL.DO.Parcel tempParcel = new IDAL.DO.Parcel()
                {
                    SenderId = SenderId,
                    TargetId = TargetId,
                    Weight = (IDAL.DO.WEIGHT)weight,
                    Priority = (IDAL.DO.PRIORITY)Priority,
                    Requested = DateTime.Now,
                    Scheduled = DateTime.MinValue,
                    PickedUp = DateTime.MinValue,
                    Deliverd = DateTime.MinValue,
                    DroneId = dID,
                };

                dal.AddParcel(tempParcel);
            }
            catch (Exception ex)
            {
                throw new AlreadyExistException(ex.Message, ex);
            }
        }
        //---------------------------------------------------------------------------------
        //updating functions:
        //---------------------------------------------------------------------------------
        public void UpdateDrone(int id, string name)//done
        {
            IDAL.DO.Drone d = dal.FindDrone(id);
            d.Model = name;
            foreach (var item in DataSource.drones)
            {
                if (item.ID == id)
                {
                    DataSource.drones.Remove(item);
                }
            }
            dal.AddDrone(d);
        }
#nullable enable
        public void UpdateStation(int id, string? name, int TotalChargeslots)
        {
            IDAL.DO.Station s = dal.FindStation(id);
            s.StationName = name;
            s.ChargeSlots = TotalChargeslots;
            foreach (var item in DataSource.stations)
            {
                if (item.ID == id)
                {
                    DataSource.stations.Remove(item);
                }
            }
            dal.AddStation(s);
        }
        public void UpdateCustomer(int id, string? NewName, string? NewPhoneNumber)
        {
            IDAL.DO.Customer c = dal.FindCustomers(id);
            c.CustomerName = NewName;
            c.Phone = NewPhoneNumber;
            foreach (var item in DataSource.customers)
            {
                if (item.ID == id)
                {
                    DataSource.customers.Remove(item);
                }
            }
            dal.AddCustomer(c);
        }
#nullable disable
        public void DroneToCharge(int id)
        {
            IDAL.DO.Drone d = dal.FindDrone(id);
            if ((d.Status != 0) || (d.Buttery < 20))
            {
                throw new DontHaveEnoughPowerException($"the drone {id} dont have enough power");
            }
            else
            {
                double distans = 0;
                int sID = 0;
                foreach (var item in stations())
                {
                    if (Distans(item.location, findDrone(id).current) > distans)
                    {
                        distans = Distans(item.location, findDrone(id).current);
                        sID = item.ID;
                    }
                }
                //מצב סוללה יעודכן בהתאם למרחק בין הרחפן לתחנה
                findDrone(id).current.Lattitude = findStation(sID).location.Lattitude;
                findDrone(id).current.Longitude = findStation(sID).location.Longitude;
                findDrone(id).Status = (STATUS)4;
                findStation(sID).FreeChargeSlots--;
                dal.AddDroneCharge(sID, id);
                foreach (var item in DAL.DataSource.droneCharges)
                {
                    if (item.DroneId == id)
                    {
                        DroneCharging droneCharging1 = new()
                        {
                            ID = (int)item.DroneId,
                            Buttery = (dal.FindDrone((int)item.DroneId)).Buttery,
                        };
                        findStation(sID).DroneChargingInStation.Add(droneCharging1);
                        break;
                    }
                }
            }
        }
        public void DroneOutCharge(int id, int time)
        {

            if (findDrone(id).Status == (STATUS)4)
            {
                findDrone(id).Status = STATUS.FREE;
                findDrone(id).Buttery = (dal.Power()[4]) * (time);
                foreach (var item in DAL.DataSource.droneCharges)
                {
                    if (item.DroneId == id)
                    {
                        findStation(item.StationId).FreeChargeSlots++;
                        DataSource.droneCharges.Remove(item);
                        foreach (var item1 in findStation(item.StationId).DroneChargingInStation)
                        {
                            if (item1.ID == id)
                            {
                                findStation(item.StationId).DroneChargingInStation.Remove(item1);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new DroneDontInCharging($"The Drone {id} Doesn't In Charging");
            }
        }
        public void AttacheDrone(int id)
        {
            if (!(findDrone(id).haveParcel))
            {
                List<Parcel> temp = parcelsNotAssociated().ToList();
                List<Parcel> temp1 = temp.FindAll(delegate (Parcel p) { return p.Priority == PRIORITY.SOS; });
                if (temp1.Count == 0)
                {
                    temp1 = temp.FindAll(delegate (Parcel p) { return p.Priority == PRIORITY.FAST; });
                    if (temp1.Count == 0)
                    {
                        temp1 = temp.FindAll(delegate (Parcel p) { return p.Priority == PRIORITY.REGULAR; });
                        if (temp1.Count == 0)
                        {
                            throw new ThereIsNoParcel("there are no parcel");
                        }
                    }
                }
                temp1 = temp1.FindAll(delegate (Parcel p) { return p.Priority == PRIORITY.SOS; });
                if (temp1.Count == 0)
                {
                    temp1 = temp1.FindAll(delegate (Parcel p) { return p.Priority == PRIORITY.FAST; });
                    if (temp1.Count == 0)
                    {
                        temp1 = temp1.FindAll(delegate (Parcel p) { return p.Priority == PRIORITY.REGULAR; });
                        if (temp1.Count == 0)
                        {
                            throw new ThereIsNoParcel("there are no parcel");
                        }
                    }
                }
                Location location = new()
                { Lattitude = 0, Longitude = 0, };
                int saveID = 0;//בטוח ידרס
                foreach (var item in temp1)
                {
                    if (Distans(findDrone(id).current, findcustomer(item.sender.ID).location) > Distans(findDrone(id).current, location))
                    {
                        location.Lattitude = findcustomer(item.sender.ID).location.Lattitude;
                        location.Longitude = findcustomer(item.sender.ID).location.Longitude;
                        saveID = item.ID;
                    }
                }
                findparcel(saveID).Drone.ID = id;
                findparcel(saveID).Drone.Buttery = findDrone(id).Buttery;
                findparcel(saveID).Drone.current = findDrone(id).current;
                findparcel(saveID).Scheduled = DateTime.Now;
                findDrone(id).Status = STATUS.BELONG;
            }
        }
        public void PickUpParcel(int id)
        {
            if (findDrone(id).Status == STATUS.BELONG)
            {
                switch (findDrone(id).Weight)
                {
                    case WEIGHT.LIGHT:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).current)) * (dal.Power()[(int)WEIGHT.LIGHT]));
                        break;
                    case WEIGHT.MEDIUM:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).current)) * (dal.Power()[(int)WEIGHT.MEDIUM]));
                        break;
                    case WEIGHT.HEAVY:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).current)) * (dal.Power()[(int)WEIGHT.HEAVY]));
                        break;
                    case WEIGHT.FREE:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).current)) * (dal.Power()[(int)WEIGHT.FREE]));
                        break;
                }
                findDrone(id).current = findDrone(id).parcel.Lsender;
                findparcel(findDrone(id).parcel.ID).PickedUp = DateTime.Now;
            }
            else
                throw new ParcelPastErroeException($"the {findDrone(id).parcel.ID} already have picked up");
        }
        public void Parceldelivery(int id)
        {
            if (findDrone(id).Status == STATUS.PICKUP)
            {
                switch (findDrone(id).Weight)
                {
                    case WEIGHT.LIGHT:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).parcel.Ltarget)) * (dal.Power()[(int)WEIGHT.LIGHT]));
                        break;
                    case WEIGHT.MEDIUM:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).parcel.Ltarget)) * (dal.Power()[(int)WEIGHT.MEDIUM]));
                        break;
                    case WEIGHT.HEAVY:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).parcel.Ltarget)) * (dal.Power()[(int)WEIGHT.HEAVY]));
                        break;
                    case WEIGHT.FREE:
                        findDrone(id).Buttery = findDrone(id).Buttery - ((Distans(findDrone(id).parcel.Lsender, findDrone(id).parcel.Ltarget)) * (dal.Power()[(int)WEIGHT.FREE]));
                        break;
                }
                findDrone(id).current = findDrone(id).parcel.Ltarget;
                findDrone(id).Status = STATUS.FREE;
                findparcel(findDrone(id).parcel.ID).Deliverd = DateTime.Now;

            }
            else
                throw new ParcelPastErroeException($"the {findDrone(id).parcel.ID} already have delivered");

        }
        //-----------------------------------------------------------------------------
        //display func
        //------------------------------------------------------------------------------
        public Station findStation(int id)//סיימתי
        {

            IDAL.DO.Station s = dal.FindStation(id);
            Location temp = new Location()
            {
                Lattitude = s.Lattitude,
                Longitude = s.Longitude,
            };
            List<DroneCharging> droneChargingTemp = new();
            foreach (var item in DataSource.droneCharges)
            {
                if (item.StationId == id)
                {
                    DroneCharging droneCharging1 = new()
                    {
                        ID = (int)item.DroneId,
                        Buttery = (dal.FindDrone((int)item.DroneId)).Buttery,
                    };
                    droneChargingTemp.Add(droneCharging1);
                }
            }
            Station newStation = new Station()
            {
                ID = (int)s.ID,
                StationName = s.StationName,
                location = temp,
                FreeChargeSlots = 5 - droneChargingTemp.Count,
                DroneChargingInStation = droneChargingTemp,
            };
            return newStation;
        }
        public Drone findDrone(int id)//סיימתי
        {
            IDAL.DO.Drone d = dal.FindDrone(id);
            ParcelTransactining parcelTransactiningTemp = new();
            Drone newStation = new Drone();
            newStation.haveParcel = d.haveParcel;
            newStation.ID = (int)d.ID;
            newStation.Model = d.Model;
            newStation.Weight = (WEIGHT)d.Weight;
            newStation.Status = (STATUS)d.Status;
            newStation.Buttery = d.Buttery;
            Location locationDrone = new()
            {
                Lattitude = d.Lattitude,
                Longitude = d.Longitude,
            };
            newStation.current = locationDrone;

            if (d.Status == IDAL.DO.STATUS.BELONG)
            {
                IDAL.DO.Parcel p = new(); 
                foreach (var item in DataSource.parcels)
                {
                    if (item.DroneId == id)
                        p = item;
                }
                IDAL.DO.Customer s = dal.FindCustomers(p.SenderId);
                IDAL.DO.Customer t = dal.FindCustomers(p.TargetId);
                CustomerInParcel send = new()
                {
                    ID = (int)s.ID,
                    CustomerName = s.CustomerName,
                };
                CustomerInParcel target = new()
                {
                    ID = (int)t.ID,
                    CustomerName = t.CustomerName,
                };
                Location locationSend = new()
                {
                    Lattitude = s.Lattitude,
                    Longitude = s.Longitude,
                };
                Location locationTarget = new()
                {
                    Lattitude = t.Lattitude,
                    Longitude = t.Longitude,
                };

                parcelTransactiningTemp.ID = (int)p.ID;
                parcelTransactiningTemp.ParcelStatus = p.PickedUp == DateTime.MinValue;
                parcelTransactiningTemp.priority = (PRIORITY)p.Priority;
                parcelTransactiningTemp.weight = (WEIGHT)p.Weight;
                parcelTransactiningTemp.sender = send;
                parcelTransactiningTemp.target = target;
                parcelTransactiningTemp.Lsender = locationSend;
                parcelTransactiningTemp.Ltarget = locationTarget;
                parcelTransactiningTemp.distance = Distans(locationSend, locationTarget);
                newStation.parcel = parcelTransactiningTemp;

            }

            return newStation;
        }
        public Parcel findparcel(int id)//סיימתי
        {
            IDAL.DO.Parcel p = dal.FindParcel(id);//לסייפ מימוש
            IDAL.DO.Customer s = dal.FindCustomers(p.SenderId);
            IDAL.DO.Customer t = dal.FindCustomers(p.TargetId);
            CustomerInParcel send = new()
            {
                ID = (int)s.ID,
                CustomerName = s.CustomerName,
            };
            CustomerInParcel target = new()
            {
                ID = (int)t.ID,
                CustomerName = t.CustomerName,
            };
            IDAL.DO.Drone d = dal.FindDrone((int)p.DroneId);
            Location tempD = new Location()
            {
                Lattitude = d.Lattitude,
                Longitude = d.Longitude,
            };
            DroneInParcel droneInParcelTemp = new()
            {
                ID = (int)d.ID,
                Buttery = d.Buttery,
                current = tempD
            };
            Parcel newParcel = new Parcel()
            {
                ID = (int)p.ID,
                sender = send,
                target = target,
                Weight = (WEIGHT)p.Weight,
                Priority = (PRIORITY)p.Priority,
                Drone = droneInParcelTemp,
                Requested = p.Requested,
                Scheduled = p.Scheduled,
                PickedUp = p.PickedUp,
                Deliverd = p.Deliverd,

            };
            return newParcel;
        }
        public Customer findcustomer(int id)//fliping done
        {
            IDAL.DO.Customer c = dal.FindCustomers(id);
            IEnumerable<IDAL.DO.Parcel> p = dal.Parcellist();
            Location temp = new Location()
            {
                Lattitude = c.Lattitude,
                Longitude = c.Longitude,
            };
            Customer newCustomer = new Customer()
            {
                ID = (int)c.ID,
                CustomerName = c.CustomerName,
                Phone = c.Phone,
                location = temp,

            };
            List<ParcelInCustomer> TempFromCustomer = new();
            ParcelInCustomer item = new();
            foreach (var item1 in p)
            {
                if (item1.SenderId == id)
                {
                    item.ID = (int)item1.ID;
                    item.priority = (PRIORITY)item1.Priority;
                    item.weight = (WEIGHT)item1.Weight;
                    if (item1.Requested < DateTime.Now && item1.Requested != DateTime.MinValue)
                    {
                        item.status = (STATUS)0;
                    }
                    if (item1.Scheduled < DateTime.Now && item1.Scheduled != DateTime.MinValue)
                    {
                        item.status = (STATUS)1;
                    }
                    if (item1.PickedUp < DateTime.Now && item1.PickedUp != DateTime.MinValue)
                    {
                        item.status = (STATUS)2;
                    }
                    if (item1.Deliverd < DateTime.Now && item1.Deliverd != DateTime.MinValue)
                    {
                        item.status = (STATUS)3;
                    }
                    if (item1.Deliverd > DateTime.Now)
                    {
                        item.status = (STATUS)5;
                    }
                    CustomerInParcel q = new()
                    {
                        ID = id,
                        CustomerName = c.CustomerName,
                    };
                    item.sender = q;
                    CustomerInParcel o = new()
                    {
                        ID = item1.TargetId,
                        CustomerName = dal.FindCustomers(item1.TargetId).CustomerName,
                    };
                    item.target = o;
                    TempFromCustomer.Add(item);
                }
            }

            List<ParcelInCustomer> TempToCustomer = new();
            ParcelInCustomer item2 = new();
            foreach (var item3 in p)
            {
                if (item3.TargetId == id)
                {
                    item2.ID = (int)item3.ID;
                    item2.priority = (PRIORITY)item3.Priority;
                    item2.weight = (WEIGHT)item3.Weight;
                    if (item3.Requested < DateTime.Now && item3.Requested != DateTime.MinValue)
                    {
                        item2.status = (STATUS)0;
                    }
                    if (item3.Scheduled < DateTime.Now && item3.Scheduled != DateTime.MinValue)
                    {
                        item2.status = (STATUS)1;
                    }
                    if (item3.PickedUp < DateTime.Now && item3.PickedUp != DateTime.MinValue)
                    {
                        item2.status = (STATUS)2;
                    }
                    if (item3.Deliverd < DateTime.Now && item3.Deliverd != DateTime.MinValue)
                    {
                        item2.status = (STATUS)3;
                    }
                    if (item3.Deliverd > DateTime.Now)
                    {
                        item2.status = (STATUS)5;
                    }
                    CustomerInParcel q = new()
                    {
                        ID = id,
                        CustomerName = c.CustomerName,
                    };
                    item2.target = q;
                    CustomerInParcel o = new()
                    {
                        ID = item3.SenderId,
                        CustomerName = dal.FindCustomers(item3.TargetId).CustomerName,
                    };
                    item.sender = o;
                    TempToCustomer.Add(item2);
                }
            }

            newCustomer.fromCustomer = TempFromCustomer;
            newCustomer.toCustomer = TempToCustomer;
            return newCustomer;
        }
        //-----------------------------------------------------------------------------------
        //listView func
        //-----------------------------------------------------------------------------------------
        public IEnumerable<Station> stations()
        {
            List<Station> temp = new();
            foreach (var item in dal.Stationlist())
            {
                temp.Add(findStation((int)item.ID));
            }
            return temp;
        }
        public IEnumerable<Drone> drones()
        {
            List<Drone> temp = new();
            foreach (var item in dal.Dronelist())
            {
                temp.Add(findDrone((int)item.ID));
            }
            return temp;
        }
        public IEnumerable<Parcel> parcels()
        {
            List<Parcel> temp = new();
            foreach (var item in dal.Parcellist())
            {
                temp.Add(findparcel((int)item.ID));
            }
            return temp;
        }
        public IEnumerable<Customer> customers()
        {
            List<Customer> temp = new();
            foreach (var item in dal.Customerlist())
            {
                temp.Add(findcustomer((int)item.ID));
            }
            return temp;
        }
        public IEnumerable<Parcel> parcelsNotAssociated()
        {
            List<Parcel> temp = new();
            foreach (var item in dal.ParcelNotAssociatedList())
            {
                temp.Add(findparcel((int)item.ID));
            }
            return temp;
        }
        public IEnumerable<Station> FreeChargeslots()
        {
            List<Station> temp = new();
            foreach (var item in dal.Stationlist())
            {
                if (item.ChargeSlots > 0)
                {
                    temp.Add(findStation((int)item.ID));
                }
            }
            return temp;
        }
        //-----------------------------------------------------------------------------------------
    }
}