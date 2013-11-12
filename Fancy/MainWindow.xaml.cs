using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Globalization;
using System.Threading;
using System.Net;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Net.NetworkInformation;

namespace Fancy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PumpUIMessage();
            Weather();
            Time();
            CPUUsage();
            RAMUsage();
            NetworkUsage();
        }
        private static string ZIP = "74115";
        private static bool Hazard = false;
        private static Weather weather = new Weather();
        private static string CurrentTime;
        private BitmapImage icon;
        private float cpu;
        private double ram;
        private double up, down;
        private void PumpUIMessage()
        {
            Task.Factory.StartNew(() =>
            {
                
                while (true)
                {
                    DateTime CurrentDateTime = DateTime.Now;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        Border1.Height = 160.0;
                        Border1.Width = 119.0;
                        lblDayName.Content = CurrentDateTime.ToString("ddd");
                        lblDayNum.Content = CurrentDateTime.Day.ToString("0#");
                        lblMonthName.Content = CurrentDateTime.ToString("MMM");
                        lblTime.Content = CurrentTime;

                        // Weather
                        lblCondition.Content = weather.Condition;
                        lblCondition.Content += Hazard ? "!" : "";
                        lbltemperature.Content = weather.Temperature + "°F";
                        imgIcon.Source = icon;

                        // Computer Info
                        lblCPU.Content = cpu.ToString("F0") + "%";
                        //pbCPU.Value = cpu;
                        string CPUdots = "";
                        for (int i = 0; i < (cpu / 20) - 1; i++)
                        {
                            CPUdots += '•';
                        }
                        CPUUsageDots.Content = CPUdots;
                        
                        lblRAM.Content = ram.ToString("F0") + "%";
                        //pbRAM.Value = ram;
                        string RAMdots = "";
                        for (int i = 0; i < (ram / 20) - 1; i++)
                        {
                            RAMdots += '•';
                        }
                        RAMUsageDots.Content = RAMdots;
                        lblOut.Content = "↑ " + (up > 1 ? up.ToString("F1") + "Mb" : (up * 1024).ToString("F0") + "Kb");
                        lblIn.Content = "↓ " + (down > 1 ? down.ToString("F1") + "Mb" : (down * 1024).ToString("F0") + "Kb");
                    }));
                    Thread.Sleep(500);
                }
            });
        }

        private void Weather()
        {
            Task.Factory.StartNew(() =>
            {
                string LastHazard = "";
                while (true)
                {
                    weather.ZIP = ZIP;
                    try
                    {
                        weather.Conditions();
                        icon = weather.GetIcon(weather.Condition);
                        icon.Freeze();
                        string Hazards = weather.GetHazards();
                        Hazard = false;
                        if (Hazards != "There are no active watches, warnings or advisories" &&
                            Hazards != LastHazard)
                        {
                            Hazard = true;
                            LastHazard = Hazards;
                            MessageBox.Show(Hazards, "Hazards", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        Thread.Sleep(900000);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Weather", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                }
            });
        }

        private void Time()
        {
            Task.Factory.StartNew(() =>
            {
                Time time = new Time();
                CurrentTime = DateTime.Now.ToString("t");
                while (true)
                {
                    CurrentTime = time.Now;

                    Thread.Sleep(30000);
                }
            });
        }

        private void CPUUsage()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    PerformanceCounter cpuCounter = new PerformanceCounter();
                    cpuCounter.CategoryName = "Processor";
                    cpuCounter.CounterName = "% Processor Time";
                    cpuCounter.InstanceName = "_Total";
                    cpuCounter.NextValue();
                    Thread.Sleep(20);
                    cpu = cpuCounter.NextValue();
                    while (true)
                    {
                        Thread.Sleep(1000);
                        cpu = cpuCounter.NextValue();
                    }
                }
                catch (Exception e)
                {

                    MessageBox.Show(e.Message);
                }
            });
        }

        private void RAMUsage()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ComputerInfo ci = new ComputerInfo();
                    while (true)
                    {
                        double totalRAM = ci.TotalPhysicalMemory / 1073741824.004733; ;
                        double AvailRAM = ci.AvailablePhysicalMemory / 1073741824.004733;
                        ram = ((1 - (AvailRAM / totalRAM)) * 100);
                        Thread.Sleep(1000);
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);

                }
            });
        }

        private void NetworkUsage()
        {
            Task.Factory.StartNew(() =>
            {
                double upOld = 0.0, downOld = 0.0;
                while (true)
                {
                    try
                    {
                        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

                        double inMAX = 0.0;
                        foreach (NetworkInterface NI in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            if (NI.GetIPv4Statistics().BytesReceived > inMAX)
                            {
                                ni = NI;
                                inMAX = NI.GetIPv4Statistics().BytesReceived;
                            }
                        }

                        double upNew = (ni.GetIPv4Statistics().BytesSent / 131072.0);
                        double upLoadTotal = upNew - upOld;

                        double downNew = (ni.GetIPv4Statistics().BytesReceived / 131072.0);
                        double downLoadTotal = downNew - downOld;

                        upOld = upNew;
                        downOld = downNew;

                        up = upLoadTotal;
                        down = downLoadTotal;

                        ni = null;
                        GC.Collect();
                        Thread.Sleep(1000);

                    }
                    catch (NetworkInformationException)
                    {
                        Thread.Sleep(5000);
                        continue;

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Network");
                    }
                }
            });
        }

        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

    }
}
