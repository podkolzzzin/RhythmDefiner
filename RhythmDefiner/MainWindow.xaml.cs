using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NAudio.CoreAudioApi;

namespace RhythmDefiner {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    private class VolumeData {

      public float VolumeLevel;
      public DateTime TimeStamp;
    }

    private const int MaxPointCount = 100;
    private int timeOnScreen = 8000;

    private DateTime start;
    private List<VolumeData> volumeInfo = new List<VolumeData>();
    private List<Line> lines = new List<Line>();
    RealTimeSoundData data = new RealTimeSoundData();

    public MainWindow() {

      InitializeComponent();
      
      start = DateTime.Now;
      data.VolumeRecieved += Data_VolumeRecieved;
      data.Start();

      DispatcherTimer timer = new DispatcherTimer();
      timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
      timer.Tick += Timer_Tick;
      timer.Start();
      InitializeLines();
    }

    private void Data_VolumeRecieved1(object sender, float e) {
      throw new NotImplementedException();
    }

    public int TimeOffset {
      get {
        return (int)(DateTime.Now - start).TotalMilliseconds % timeOnScreen;
      }
    }

    private void Data_VolumeRecieved(object sender, float e) {

      volumeInfo.Add(new VolumeData() {
        TimeStamp = DateTime.Now,
         VolumeLevel = e
      });
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {

      InitializeVault();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {

      InitializeVault();
    }

    private void Timer_Tick(object sender, EventArgs e) {

      InitializeVault();
      if (volumeInfo.Count == 0)
        return;

      List<Point> points = new List<Point>();
      for (int i = 0; i < volumeInfo.Count; i++) {
        var item = volumeInfo[volumeInfo.Count - i - 1];
        if ((DateTime.Now - item.TimeStamp).TotalMilliseconds > timeOnScreen)
          break;

        points.Add(new Point((DateTime.Now - item.TimeStamp).TotalMilliseconds * Width / timeOnScreen, Height - item.VolumeLevel * Height * 150));
      }

      if (points.Count > 0) {
        var pathGeom = new PathGeometry();
        pathGeom.Figures.Add(new PathFigure(points[0], points.Skip(1).Select(f => new LineSegment(f, true)), false));
        XPath.Data = pathGeom;  
      }
    }

    private void InitializeLines() {

      int speed;
      if (int.TryParse(XSpeed.Text, out speed)) {
        foreach (var item in lines)
          XRoot.Children.Remove(item);
        lines.Clear();

        int lineCount = (int)(speed / 60000.0 * timeOnScreen * 2);

        for (int i = 0; i < lineCount; i++) {
          var line = new Line() {
            Stroke = Brushes.Silver,
            StrokeThickness = 1
          };
          XRoot.Children.Add(line);
          lines.Add(line);
        }
      }
    }

    private void InitializeVault() {

      for (int i = 0; i < lines.Count; i++) {

        lines[i].X2 = lines[i].X1 = (Width / timeOnScreen * TimeOffset + i * Width / lines.Count) % Width;
        if (i % 2 == 0) {
          lines[i].Y1 = Height / 4;
          lines[i].Y2 = Height - Height / 4;
        }
        else {
          lines[i].Y1 = 0;
          lines[i].Y2 = Height;
        }
      }
    }

    private void XSpeed_TextChanged(object sender, TextChangedEventArgs e) {

      InitializeLines();
    }

    private void XTime_TextChanged(object sender, TextChangedEventArgs e) {

      int.TryParse(XTime.Text, out timeOnScreen);
      if (timeOnScreen == 0)
        timeOnScreen = 1;
    }
  }
}
