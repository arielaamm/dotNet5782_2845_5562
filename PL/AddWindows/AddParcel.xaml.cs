﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Interaction logic for AddParcel.xaml
    /// </summary>
    public partial class AddParcel : Page
    {    
        private readonly BlApi.IBL bl;
        Window parent;

        public AddParcel(BlApi.IBL bl , Window parent)
        {
            this.bl = bl;
            this.parent = parent;
            InitializeComponent();
            PrioritySelector.ItemsSource = Enum.GetValues(typeof(BO.Priority));
            WeightSelector.ItemsSource = Enum.GetValues(typeof(BO.Weight));
            var name = from n in bl.Customers()
                       select n.CustomerName;
            TargetSelector.ItemsSource = name;
            SenderSelector.ItemsSource = name;
        }
        BO.Parcel parcel = new();

        private void Button_Click(object sender, RoutedEventArgs e) => parent.Close();
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click_Add_Parcel(object sender, RoutedEventArgs e)
        {
            int t = bl.Parcels().Count();
            try
            {
                parcel.ID = int.Parse(TextBoxID.Text);
                parcel.Weight = (BO.Weight)WeightSelector.SelectedItem;
                parcel.Priority = (BO.Priority)PrioritySelector.SelectedItem;
                BO.CustomerInParcel customerInParcelSender = new BO.CustomerInParcel();
                BO.CustomerInParcel customerInParcelTarget = new BO.CustomerInParcel();
                customerInParcelSender.ID = bl.Customers().ToList().Find(c => c.CustomerName == (string)SenderSelector.SelectedItem).ID;
                parcel.sender = customerInParcelSender;
                customerInParcelTarget.ID = bl.Customers().ToList().Find(c => c.CustomerName == (string)TargetSelector.SelectedItem).ID;
                parcel.target = customerInParcelTarget;
                if (parcel.sender.ID == parcel.target.ID)
                    MessageBox.Show("you can't send a parcel to your self, try agine");
                else
                    bl.AddParcel(parcel);
            }
            catch (Exception ex)
            {
                if (ex.GetType().ToString() == "BLExceptions.AlreadyExistException")
                {
                    MessageBox.Show(ex.Message + ", enter anther ID");

                }
                else
                    MessageBox.Show("Enter the data in the necessary places");
            }
            if (t < bl.Parcels().Count())
            {
                MessageBox.Show("The Parcel successfully added");
                parent.Close();
                new ParcelWindow(bl, int.Parse(TextBoxID.Text)).Show();

            }
        }
    }
}