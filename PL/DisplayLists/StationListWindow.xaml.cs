﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Interaction logic for StationListWindow.xaml
    /// </summary>
    public partial class StationListWindow : Window
    {
        private readonly BlApi.IBL bl = BL.BL.GetInstance();

        internal ObservableCollection<BO.StationToList> StationsList
        {
            get => (ObservableCollection<BO.StationToList>)GetValue(stationsDependency);
            set => SetValue(stationsDependency, value);
        }
        static readonly DependencyProperty stationsDependency = DependencyProperty.Register(
            nameof(StationsList),
            typeof(ObservableCollection<BO.StationToList>),
            typeof(Window));
        public enum Full { full = 0, usable = 1 }
        public StationListWindow(BlApi.IBL bl)
        {
            InitializeComponent();
            this.bl = bl;
            FullSeletor.ItemsSource = Enum.GetValues(typeof(Full));
            StationsList = new(this.bl.Stations());

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Reload();
            new StationWindow(bl).Show();
        }
        private void mousedoubleclick(object sender, MouseButtonEventArgs e)
        {
            var cb = sender as DataGrid;
            BO.StationToList a = (BO.StationToList)cb.SelectedValue;
            try
            {
                new StationWindow(bl, a.ID).Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Click on properties only please");
            }
        }
        private void Reload()
        {
            StationsList = new(bl.Stations());
        }

        private void FullSeletor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.SelectedItem == null)
                StationsList = new(bl.Stations());
            else
            {
                if ((Full)cb.SelectedItem == (Full)0)
                {
                    StationsList = new();
                    var a = from Customer in bl.Stations()
                            where ((Full)Customer.FreeChargeSlots == (Full)cb.SelectedItem)
                            select Customer;
                    StationsList = new ObservableCollection<BO.StationToList>(a);
                }
                else
                {
                    StationsList = new();
                    var a = from Station in bl.Stations()
                            where (Station.FreeChargeSlots > 0)
                            select Station;
                    StationsList = new ObservableCollection<BO.StationToList>(a);
                }
            }
        }
    }
}
