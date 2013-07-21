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
using System.IO;
using System.Diagnostics;
using Common;

namespace TestClient
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        GameModel gm;
        GameAI ai;
        TurnInfo lastInfo;
        TurnInfo aiInfo;
        int cx;
        int cr;
        Task exe;
        bool autoForward;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Execution()
        {
            do
            {
                lock (gm)
                {
                    aiInfo = ai.NextControl(gm, out cx, out cr);
                    lastInfo = gm.ForwardStep(cx, cr);
                }
                Trace.TraceInformation("{0}, CX {1}, CR {2}, {3}", gm.ToString(), cx, cr, lastInfo.ToString());
            }
            while (lastInfo.AppraisalScore >= 0 && autoForward);
        }

        private void RenderingHandler(object sender, EventArgs e)
        {
            lock (gm)
            {
                mainField.Text = gm.Main.GetStringField();
                information.Text = string.Format("{0}\n{1}", gm.GetStringInformation(), aiInfo.GetStringInformation());
            }
            if (exe != null && exe.IsCompleted)
            {
                forwardButton.IsEnabled = true;
            }
        }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            StringReader sr = GenerateInput.SmallGenerate(new Random(0), 100);
            gm = GameModel.ReadGameModel(sr);
            //ai = new TestAI(gm);
            //ai = new TargetAI(gm, 3, false);
            ai = new IgnitionAI(gm, 2, true);
            //ai = new FireAI(gm, 2, false);
            //ai = new SwitchingAI(gm, 2, 10);
            CompositionTarget.Rendering += RenderingHandler;
        }

        private void ClosedHandler(object sender, EventArgs e)
        {
            autoForward = false;
            if (exe != null) exe.Wait();
            Trace.Close();
        }

        private void Forwarding(object sender, RoutedEventArgs e)
        {
            if (exe == null || exe.IsCompleted)
            {
                exe = Task.Run((Action)Execution);
            }
            forwardButton.IsEnabled = false;
        }

        private void AutoChecked(object sender, RoutedEventArgs e)
        {
            autoForward = true;
            forwardButton.IsEnabled = false;
            if (exe == null || exe.IsCompleted)
            {
                exe = Task.Run((Action)Execution);
            }
        }

        private void AutoUnchecked(object sender, RoutedEventArgs e)
        {
            autoForward = false;
        }
    }
}
