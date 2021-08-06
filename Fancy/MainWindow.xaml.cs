using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

            stopwatch.Start();

            WeatherFactory weatherFactory = new WeatherFactory();
            weather = weatherFactory.Create("nws2");
            weather.Zip = "74037";
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                _timer.Enabled = false;
            }

            PumpUIMessage();

            if (Debugger.IsAttached)
            {
                _timer.Enabled = true;
            }
        }

        static System.Timers.Timer _timer;
        private static bool Hazard = false;
        private static Weather weather;
        private float cpu;
        private double ram;
        private double up, down, upOld = 0.0, downOld = 0.0;
        private ComputerInfo ci = new ComputerInfo();
        private PerformanceCounter cpuCounter = new PerformanceCounter();
        private NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[5];
        private Stopwatch stopwatch = new Stopwatch();
        private Dictionary<string, TimeSpan> keyValuePairs = new Dictionary<string, TimeSpan>();
        //private Polygon cpupoly;
        //private Polygon rampoly;
        //private Polygon netUpPoly;
        //private Polygon netDownPoly;

        private void PumpUIMessage()
        {
            CPUUsage();
            RAMUsage();
            NetworkUsage();
            weather.Start();

            DateTime CurrentDateTime = DateTime.Now;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Border1.Height = 160.0;
                Border1.Width = 119.0;
                lblDayName.Content = CurrentDateTime.ToString("ddd");
                lblDayNum.Content = CurrentDateTime.Day.ToString("0#");
                lblMonthName.Content = CurrentDateTime.ToString("MMM");

                // Weather
                lblCondition.Content = weather.forecast?.shortForecast;
                lblCondition.Content += Hazard ? "!" : "";
                lbltemperature.Content = weather.forecast?.temperature.ToString() + "°F";
                icon.Source = weather.forecast?.weatherImage;


                // Computer Info
                lblCPU.Content = cpu.ToString("F0") + "%";
                string CPUdots = "";
                for (int i = 0; i < (cpu / 20) - 1; i++)
                {
                    CPUdots += '•';
                }
                CPUUsageDots.Content = CPUdots;
                UpdatePolygon(ref polyCPU, Int32.Parse(cpu.ToString("F0")), 20, 115, 15, 30);

                        
                lblRAM.Content = ram.ToString("F0") + "%";
                string RAMdots = "";
                for (int i = 0; i < (ram / 20) - 1; i++)
                {
                    RAMdots += '•';
                }
                RAMUsageDots.Content = RAMdots;
                UpdatePolygon(ref polyRAM, Int32.Parse(ram.ToString("F0")), 40, 115, 15, 30);

                UpdatePolygon(ref polyNetUp, Int32.Parse(up.ToString("F0")), 10, 140, 15, 30);
                UpdatePolygon(ref polyNetDown, Int32.Parse(down.ToString("F0")), 50, 149, 15, 30);
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
            if (!keyValuePairs.ContainsKey("NetworkUsage"))
            {
                keyValuePairs.Add("NetworkUsage", stopwatch.Elapsed); 
            }

            if ((stopwatch.ElapsedMilliseconds - keyValuePairs["NetworkUsage"].TotalMilliseconds) < 1000)
            {
                return;
            }

            double upNew = (ni.GetIPv4Statistics().BytesSent / 131072.0);
            double upLoadTotal = upNew - upOld;

            double downNew = (ni.GetIPv4Statistics().BytesReceived / 131072.0);
            double downLoadTotal = downNew - downOld;

            upOld = upNew;
            downOld = downNew;

            up = upLoadTotal * (1000 / (stopwatch.ElapsedMilliseconds - keyValuePairs["NetworkUsage"].TotalMilliseconds));
            down = downLoadTotal * (1000 / (stopwatch.ElapsedMilliseconds - keyValuePairs["NetworkUsage"].TotalMilliseconds));

            keyValuePairs["NetworkUsage"] = stopwatch.Elapsed;
        }


        private void UpdatePolygon(ref Polygon poly, int value, int posX, int posY, int maxHeight, int historyLength = 30)
        {
            int v = 100 / maxHeight;

            value = value < 0 ? 0 : value;
            value = value == 0 ? value : value / v;
            value = value * -1; // expand up

            int length = 0;
            System.Windows.Point end = new System.Windows.Point(historyLength + posX + 1, posY);

            // Create a blue and black Brush
            SolidColorBrush whiteBrush = new SolidColorBrush();
            whiteBrush.Color = Colors.White;
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            // Create a Polygon
            if (poly == null || poly.Points.Count < historyLength)
            {
                PointCollection polygonPoints = new PointCollection();

                // Create a collection of points for a polygon
                System.Windows.Point Point1 = new System.Windows.Point(posX + 3, posY);
                poly = new Polygon();
                poly.Stroke = whiteBrush;
                poly.MinHeight = posY - maxHeight;
                poly.StrokeThickness = 0.5;
                poly.Points.Add(end);

                polygonPoints.Add(Point1);

                for (int i = 0; i < historyLength; i++)
                {
                    polygonPoints.Add(new System.Windows.Point(i + posX + 3, posY));
                }

                // Set Polygon.Points properties
                poly.Points = polygonPoints;
                poly.Name = "poly";

                // Add Polygon to the page
                grid.Children.Add(poly);
                poly.Fill = whiteBrush;
            }

            length = poly.Points.Count;

            poly.Points.RemoveAt(1);

            for (int i = 1; i < historyLength - 1; i++)
            {
                System.Windows.Point p = poly.Points[i];
                p.X -= 1;
                poly.Points[i] = p;
            }

            poly.Points.RemoveAt(length - 2);
            poly.Points.Add(new System.Windows.Point((length) + posX, value + posY));
            poly.Points.Add(end);
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