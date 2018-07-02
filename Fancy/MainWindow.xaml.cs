using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
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

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PumpUIMessage();
        }

        static System.Timers.Timer _timer;
        private static bool Hazard = false;
        private static Weather weather = new Weather();
        private float cpu;
        private double ram;
        private double up, down, upOld = 0.0, downOld = 0.0;
        private ComputerInfo ci = new ComputerInfo();
        private PerformanceCounter cpuCounter = new PerformanceCounter();
        private NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[5];

        private void PumpUIMessage()
        {
            CPUUsage();
            RAMUsage();
            NetworkUsage();
            DateTime CurrentDateTime = DateTime.Now;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Border1.Height = 160.0;
                Border1.Width = 119.0;
                lblDayName.Content = CurrentDateTime.ToString("ddd");
                lblDayNum.Content = CurrentDateTime.Day.ToString("0#");
                lblMonthName.Content = CurrentDateTime.ToString("MMM");

                // Weather
                lblCondition.Content = weather.Condition;
                lblCondition.Content += Hazard ? "!" : "";
                lbltemperature.Content = weather.Temperature + "°F";


                // Computer Info
                lblCPU.Content = cpu.ToString("F0") + "%";
                string CPUdots = "";
                for (int i = 0; i < (cpu / 20) - 1; i++)
                {
                    CPUdots += '•';
                }
                CPUUsageDots.Content = CPUdots;
                        
                lblRAM.Content = ram.ToString("F0") + "%";
                string RAMdots = "";
                for (int i = 0; i < (ram / 20) - 1; i++)
                {
                    RAMdots += '•';
                }
                RAMUsageDots.Content = RAMdots;

                lblOut.Content = "↑ " + (up > 1 ? up.ToString("F1") + "Mb" : (up * 1024).ToString("F0") + "Kb");
                lblIn.Content = "↓ " + (down > 1 ? down.ToString("F1") + "Mb" : (down * 1024).ToString("F0") + "Kb");
            }));
        }

        private void CPUUsage()
        {
            cpu = cpuCounter.NextValue();
        }

        private void RAMUsage()
        {
            double totalRAM = ci.TotalPhysicalMemory / 1073741824.004733; ;
            double AvailRAM = ci.AvailablePhysicalMemory / 1073741824.004733;
            ram = ((1 - (AvailRAM / totalRAM)) * 100);
        }

        
        private void NetworkUsage()
        {
            double upNew = (ni.GetIPv4Statistics().BytesSent / 131072.0);
            double upLoadTotal = upNew - upOld;

            double downNew = (ni.GetIPv4Statistics().BytesReceived / 131072.0);
            double downLoadTotal = downNew - downOld;

            upOld = upNew;
            downOld = downNew;

            up = upLoadTotal;
            down = downLoadTotal;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}