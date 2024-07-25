using System;
using System.Drawing;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Runtime.Versioning;

class SpeechApp
{
    private SpeechRecognitionEngine recognizer;
    private VideoCapture capture;
    private string recognizedText = "";
    public bool running = true;

    // Атрибуты указывают, что этот конструктор поддерживается только на Windows
    [SupportedOSPlatform("windows")]
    public SpeechApp(int cameraIndex = 0)
    {
        recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("ru-RU"));
        recognizer.SetInputToDefaultAudioDevice();
        recognizer.LoadGrammar(new DictationGrammar());
        recognizer.SpeechRecognized += (s, e) =>
        {
            recognizedText = "Said: " + e.Result.Text;
            Console.WriteLine(recognizedText);
        };
        recognizer.RecognizeCompleted += (s, e) =>
        {
            if (running)
                recognizer.RecognizeAsync();
        };

        capture = new VideoCapture(cameraIndex);
        if (!capture.IsOpened)
        {
            Console.WriteLine("Failed to open camera with index " + cameraIndex);
            return;
        }

        StartRecognition();
        StartVideoCapture();
    }

    [SupportedOSPlatform("windows")]
    private void StartRecognition()
    {
        Task.Run(() => recognizer.RecognizeAsync());
        Console.WriteLine("Recognition thread started");
    }

    private void StartVideoCapture()
    {
        Task.Run(() =>
        {
            Console.WriteLine("Starting video capture loop");
            while (running)
            {
                using (Mat frame = capture.QueryFrame())
                {
                    if (frame == null)
                        continue;

                    if (!string.IsNullOrEmpty(recognizedText))
                    {
                        CvInvoke.PutText(frame, recognizedText, new System.Drawing.Point(10, frame.Height - 10), FontFace.HersheySimplex, 1.0, new Bgr(Color.Green).MCvScalar, 2);
                    }

                    CvInvoke.Imshow("Video", frame);
                    if (CvInvoke.WaitKey(1) == 'q')
                    {
                        running = false;
                        break;
                    }
                }
            }

            capture.Release();
            CvInvoke.DestroyAllWindows();
        });
        Console.WriteLine("Video thread started");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting application");
        SpeechApp app = new SpeechApp(cameraIndex: 1);
        Console.CancelKeyPress += (s, e) =>
        {
            app.running = false;
            Environment.Exit(0);
        };

        while (app.running)
        {
            Thread.Sleep(100);
        }
    }
}