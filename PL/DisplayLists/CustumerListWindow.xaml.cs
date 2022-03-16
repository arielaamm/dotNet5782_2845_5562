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
    /// Interaction logic for CustomerListWindow.xaml
    /// </summary>
    public partial class CustomerListWindow : Window
    {
        private readonly BlApi.IBL bl = BL.BL.GetInstance();

        internal ObservableCollection<BO.CustomerToList> CustomersList
        {
            get => (ObservableCollection<BO.CustomerToList>)GetValue(customersDependency);
            set => SetValue(customersDependency, value);
        }
        static readonly DependencyProperty customersDependency = DependencyProperty.Register(
            nameof(CustomersList),
            typeof(ObservableCollection<BO.CustomerToList>),
            typeof(Window));
        public enum OnWay { nothing = 0, has = 1}
        public enum Received { nothing = 0, has = 1}

        public CustomerListWindow(BlApi.IBL bl)
        {
            InitializeComponent();
            this.bl = bl;
            OnWaySeletor.ItemsSource = Enum.GetValues(typeof(OnWay));
            ReceivedSeletor.ItemsSource = Enum.GetValues(typeof(OnWay));
            CustomersList = new(this.bl.Customers());
        }
        private void OnWaySeletor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.SelectedItem == null)
                CustomersList = new(bl.Customers());
            else
            {
                if ((Received)cb.SelectedItem == (Received)0)
                {
                    CustomersList = new();
                    var a = from Customer in bl.Customers()
                            where ((Received)Customer.NumFoParcelOnWay == (Received)cb.SelectedItem)
                            select Customer;
                    CustomersList = new ObservableCollection<BO.CustomerToList>(a);
                }
                else
                {
                    CustomersList = new();
                    var a = from Customer in bl.Customers()
                            where (Customer.NumFoParcelOnWay > 0)
                            select Customer;
                    CustomersList = new ObservableCollection<BO.CustomerToList>(a);
                }
            }
        }
        private void ReceivedSeletor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.SelectedItem == null)
                CustomersList = new(bl.Customers());
            else
            {
                if ((Received)cb.SelectedItem == (Received)0)
                {
                    CustomersList = new();
                    var a = from Customer in bl.Customers()
                            where ((Received)Customer.NumFoParcelReceived == (Received)cb.SelectedItem)
                            select Customer;
                    CustomersList = new ObservableCollection<BO.CustomerToList>(a);
                }
                else
                {
                    CustomersList = new();
                    var a = from Customer in bl.Customers()
                            where (Customer.NumFoParcelReceived > 0)
                            select Customer;
                    CustomersList = new ObservableCollection<BO.CustomerToList>(a);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Reload();
            new CustomerWindow(bl).Show();
        }
        private void mousedoubleclick(object sender, MouseButtonEventArgs e)
        {
            var cb = sender as DataGrid;
            BO.CustomerToList a = (BO.CustomerToList)cb.SelectedValue;
            try
            {
                new CustomerWindow(bl, a.ID).Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Click on properties only please");
            }
        }
        private void Reload()
        {
            CustomersList = new(bl.Customers());
        }

    }
}
