using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        //private static string ZIP = "74115";
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
                    weather.ZIP = new LocateMe().Location;
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
                        Thread.Sleep(10000);
                    }
                }
            });
        }

        private void Time()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        DateTime ComputerTime = DateTime.Now;
                        DateTime ServerTime = new Time().Now;
                        for (int i = 0; i < 900; i++)
                        {
                            CurrentTime = DateTime.Now.Add(ServerTime - ComputerTime).ToString("t");
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Time", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(10000);
                    }
                }
            });
        }

        private void CPUUsage()
        {
            Task.Factory.StartNew(() =>
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
                    try
                    {
                        Thread.Sleep(1000);
                        cpu = cpuCounter.NextValue();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "CPU", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(10000);
                    }
                }
            });
        }

        private void RAMUsage()
        {
            Task.Factory.StartNew(() =>
            {
                ComputerInfo ci = new ComputerInfo();
                while (true)
                {
                    try
                    {
                        double totalRAM = ci.TotalPhysicalMemory / 1073741824.004733; ;
                        double AvailRAM = ci.AvailablePhysicalMemory / 1073741824.004733;
                        ram = ((1 - (AvailRAM / totalRAM)) * 100);
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "RAM", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(10000);
                    }
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
                    catch (NetworkInformationException netError)
                    {
                        MessageBox.Show(netError.Message, "Network Information Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(10000);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Network", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(10000);
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
