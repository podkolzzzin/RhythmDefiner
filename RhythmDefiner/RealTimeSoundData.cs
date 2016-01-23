using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace RhythmDefiner {

  class RealTimeSoundData {

    private MMDevice device;
    private WasapiCapture capture;
    private SynchronizationContext context;

    public RealTimeSoundData() {

      var enumerator = new MMDeviceEnumerator();
      var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
      var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
      device = captureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
      capture = new WasapiCapture(device);
      context = SynchronizationContext.Current;
      capture.DataAvailable += Capture_DataAvailable;
    }

    public float Volume {
      get {
        return device.AudioMeterInformation.MasterPeakValue;
      }
    }

    public event EventHandler<float> VolumeRecieved;

    public void Start() {

      capture.StartRecording();
    }

    private void Capture_DataAvailable(object sender, WaveInEventArgs e) {

      context.Post(o => VolumeRecieved?.Invoke(this, device.AudioMeterInformation.MasterPeakValue), null);
    }
  }
}
