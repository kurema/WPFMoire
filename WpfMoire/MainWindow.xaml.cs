using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfMoire
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public int w = 200;
        public int h = 200;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string s = FunctionTextBox.Text;
            TryAddNewShape(s);
        }

        public delegate double PatternMaker(int x, int y, int w, int h, ref Random rand);

        private void AddNewShapeLight(PatternMaker f)
        {
            DrawingBrush db = new DrawingBrush();

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();	// DrawingContextの取得
            Random rd = new Random();
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    byte alpha = (byte)Math.Floor(f(x, y, w, h, ref rd) * 255);
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(alpha, 0, 0, 0)), null, new Rect(new Point(x, y), new Point(x + 1, y + 1)));
                }
            }
            dc.Close();
            var bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);	// 描画結果を書き込む
            var brush = new ImageBrush(bitmap);	// ブラシの作成
            brush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            brush.Freeze();


            Border b = new Border();
            b.Background = brush;
            b.Width = w;
            b.Height = h;
            b.BorderBrush = Brushes.DarkGray;
            b.BorderThickness = new Thickness(0);
            b.Tag = new double[] { 0.0, 1.0 };
            b.RenderTransformOrigin = new Point(0.5, 0.5);

            RenderOptions.SetBitmapScalingMode(b, BitmapScalingMode.NearestNeighbor);//これを消すと描画モードが変わります。

            Random r = new Random();

            Canvas.SetLeft(b, r.Next((int)PictureCanvas.ActualWidth - w));
            Canvas.SetTop(b, r.Next((int)PictureCanvas.ActualHeight - h));

            b.MouseDown += new MouseButtonEventHandler(b_MouseDown);

            PictureCanvas.Children.Add(b);
        }

        private void AddNewShape(string s)
        {
            PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
             {
                 return SimpleMathEval.EvalXY(s, x, y,w,h, ref rand);
             };
            AddNewShapeLight(f);
        }

        private Point p;

        void b_MouseDown(object sender, MouseButtonEventArgs e)
        {
            p = e.GetPosition(PictureCanvas);
            ((Border)sender).MouseMove += new MouseEventHandler(b_MouseMove);
            ((Border)sender).MouseWheel += new MouseWheelEventHandler(MainWindow_MouseWheel);
            ((Border)sender).MouseUp += ((a, b) =>
            {
                ((Border)sender).MouseMove -= new MouseEventHandler(b_MouseMove);
                ((Border)sender).MouseWheel -= new MouseWheelEventHandler(MainWindow_MouseWheel);
            });
            ((Border)sender).MouseLeave += ((a, b) =>
            {
                ((Border)sender).MouseMove -= new MouseEventHandler(b_MouseMove);
                ((Border)sender).MouseWheel -= new MouseWheelEventHandler(MainWindow_MouseWheel);
            });

        }

        void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double f = ((double[])((Border)sender).Tag)[0];
            double d = ((double[])((Border)sender).Tag)[1];
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                f += (e.Delta) * 0.2;
                ((double[])((Border)sender).Tag)[0] = f;
            }
            else
            {
                d *= (1 + e.Delta * 0.002);
                ((double[])((Border)sender).Tag)[1] = d;
            }
            ScaleTransform st = new ScaleTransform(d, d);
            RotateTransform rt = new RotateTransform(f);
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(st);
            tg.Children.Add(rt);
            ((Border)sender).RenderTransform = tg;
        }

        void b_MouseMove(object sender, MouseEventArgs e)
        {
            Canvas.SetLeft((Border)sender, Canvas.GetLeft((Border)sender) + e.GetPosition(PictureCanvas).X - p.X);
            Canvas.SetTop((Border)sender, Canvas.GetTop((Border)sender) + e.GetPosition(PictureCanvas).Y - p.Y);
            p = e.GetPosition(PictureCanvas);
        }

        private void TryAddNewShape(string s)
        {
            try
            {
                AddNewShape(s);
            }
            catch
            {
                MessageBox.Show("不正な式です");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string s = (string)((MenuItem)sender).Tag;
            FunctionTextBox.Text = s;

            switch (s)
            {
                case "(x*y)%2==0":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (x * y) % 2 == 0 ? 1 : 0;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "(x*y)%5==0?1:0":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (x * y) % 5 == 0 ? 1 : 0;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "(x+y)%2==0":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (x * y) % 2 == 0 ? 1 : 0;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "(int(x/5)+int(y/5))%2==0?1:0":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (Math.Floor(x / 5.0) + Math.Floor(y / 5.0)) % 2 == 0 ? 1 : 0;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "sin(x*0.628)/2+0.5":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return Math.Sin(x * Math.PI / 5.0) / 2.0 + 0.5;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "sin(y*0.628)/2+0.5":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return Math.Sin(y * Math.PI / 5.0) / 2.0 + 0.5;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "(sin(x*0.628)+sin(y*0.628))/4+0.5":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (Math.Sin(x * Math.PI / 5.0) + Math.Sin(y * Math.PI / 5.0)) / 4.0 + 0.5;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "log(x)":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return Math.Log(x);
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "log(x*y)":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return Math.Log(x * y);
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "x*y":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return x * y;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "rand":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return rand.NextDouble();
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "ensyu1":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (Math.Sqrt(Math.Pow(w / 2.0 - x, 2) + Math.Pow(h / 2.0 - y, 2)) / 3.0) % 2 > 1 ? 1 : 0;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "ensyu2":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return (Math.Sqrt(Math.Pow(w / 2.0 - x, 2) + Math.Pow(h / 2.0 - y, 2)) / 3.0) % 2 > 1 ? 0 : 1;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "ensyu3":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return Math.Sin(Math.Sqrt(Math.Pow(w / 2.0 - x, 2) + Math.Pow(h / 2.0 - y, 2)) / 3.0*Math.PI)*0.5+0.5 ;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                case "ensyu4":
                    {
                        PatternMaker f = delegate (int x, int y, int w, int h, ref Random rand)
                        {
                            return Math.Cos(Math.Sqrt(Math.Pow(w / 2.0 - x, 2) + Math.Pow(h / 2.0 - y, 2)) / 3.0 * Math.PI) * 0.5 + 0.5;
                        };
                        AddNewShapeLight(f);
                    }
                    break;
                default:
                    TryAddNewShape(s);
                    break;
            }

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("使い方\n下の数式にxとyを含む式を書いて「追加」ボタンを押して下さい。\n使える数式は四則演算、剰余(%)、三角関数・log・log10・Floorです。\n関数内で括弧は使えない点は気を付けてください。\n挿入メニューからサンプルを追加できます。\n大きさは200x200なので注意し下さい。");
            MessageBox.Show("移動など\nドラッグで移動できます。\n左クリック+マウスホイール移動で回転、\n右クリック+マウスホイール移動で拡大縮小できます。");
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            string s = (string)((MenuItem)sender).Tag;
            var ss = s.Split('*');
            w = int.Parse(ss[0]);
            h = int.Parse(ss[1]);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

        }
    }

    public static class SimpleMathEval
    {
        public static double EvalXY(string s, int x, int y,int w,int h,ref Random rd)
        {
            while(System.Text.RegularExpressions.Regex.IsMatch(s, @"rand", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"rand", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                System.Text.RegularExpressions.Match m = r.Match(s);
                s=r.Replace(s, rd.NextDouble().ToString(), 1);
            }
            return Eval(s.Replace("X", x.ToString()).Replace("x", x.ToString()).Replace("Y", y.ToString()).Replace("y", y.ToString()).Replace("W", w.ToString()).Replace("w", w.ToString()).Replace("H", h.ToString()).Replace("h", h.ToString()));
        }

        public static double Eval(string s)
        {
            double d;
            if (double.TryParse(s, out d))
            {
                return d;
            }
            else if (s.ToLower() == "pi")
            {
                return Math.PI;
            }

            else
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(s, @"sin\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"sin\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Sin(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"cos\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"cos\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Cos(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"tan\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"tan\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Tan(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"log\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"log\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Log(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"log10\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"log10\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Log10(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"sqrt\(.+?\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"sqrt\((.+?)\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Sqrt(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"floor\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"floor\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Floor(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"int\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"int\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Math.Floor(Eval(m.Groups[1].Value)).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"\(.+?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\((.+?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(r.Replace(s, Eval(m.Groups[1].Value).ToString(), 1));
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\?.+\:.+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\?(.+)\:(.+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    if (Eval(m.Groups[1].Value) == 0)
                    {
                        return Eval(r.Replace(s, Math.Sin(Eval(m.Groups[3].Value)).ToString()));
                    }
                    else
                    {
                        return Eval(r.Replace(s, Math.Sin(Eval(m.Groups[2].Value)).ToString()));
                    }
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+?\^.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+?)\^(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Math.Pow( Eval(m.Groups[1].Value),  Eval(m.Groups[2].Value) );
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+?\=\=?.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+?)\=\=?(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) == Eval(m.Groups[2].Value) ? 1 : 0;
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\>=.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\>=(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) >= Eval(m.Groups[2].Value) ? 1 : 0;
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\<=.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\<=(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) <= Eval(m.Groups[2].Value) ? 1 : 0;
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\>.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\>(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) > Eval(m.Groups[2].Value) ? 1 : 0;
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\<.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\<(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) < Eval(m.Groups[2].Value) ? 1 : 0;
                }

                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\+.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\+(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) + Eval(m.Groups[2].Value);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\-.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\-(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) - Eval(m.Groups[2].Value);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\*.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\*(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) * Eval(m.Groups[2].Value);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\/.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\/(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) / Eval(m.Groups[2].Value);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^.+\%.+$"))
                {
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^(.+)\%(.+)$");
                    System.Text.RegularExpressions.Match m = r.Match(s);
                    return Eval(m.Groups[1].Value) % Eval(m.Groups[2].Value);
                }
                else
                {
                    throw new Exception("計算できない数式です:-(");
                }



            }
        }

    }
}
