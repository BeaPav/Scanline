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


public class MyPoint
{
    public int X;
    public int Y;

    public MyPoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class MyEdge
{
    public MyPoint A;
    public MyPoint B;
    public int yMax;

    public MyEdge(MyPoint _A, MyPoint _B)
    {
        A = _A;
        B = _B;

        yMax = ReturnYMax();
    }


    public int ReturnYMin()
    {
        return Math.Min(A.Y, B.Y);
    }

    public int ReturnYMax()
    {
        return Math.Max(A.Y, B.Y);
    }

    public double ReturnXofYMin()
    {
        if (A.Y > B.Y) return B.X;
        return A.X;
    }

    public double ReturnRecm()
    {
        if (B.X != A.X) return 1 / (Convert.ToDouble(B.Y - A.Y) / (B.X - A.X));
        else return 0;
    }


    public void ShortenEdge()
    {
        if (A.Y < B.Y) A = new MyPoint(Convert.ToInt32(Math.Round(A.X + ReturnRecm())), A.Y + 1);
        else B = new MyPoint(Convert.ToInt32(Math.Round(B.X + ReturnRecm())), B.Y + 1);
    }
}

public class Polyside
{
    public int yMax;
    public double xOfyMin;
    public double recm;

    public Polyside(int _YMax, double _XofYMin, double _Recm)
    {
        yMax = _YMax;
        xOfyMin = _XofYMin;
        recm = _Recm;
    }

    public void ShortenPolyside()
    {
        xOfyMin += recm;
    }
}

namespace Scanline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<MyPoint> Vert;
        bool PolyClosed = false;
        int RastWidth;
        int RastHeight;
        int MaxY = int.MinValue;
        int MinY = int.MaxValue;


        public MainWindow()
        {
            InitializeComponent();
            Vert = new List<MyPoint>();
            RastHeight = Convert.ToInt32(Math.Round(g.Height / 10.0));
            RastWidth = Convert.ToInt32(Math.Round(g.Width / 10.0));
            g.Height = RastHeight * 10;
            g.Width = RastWidth * 10;
            DrawRast();

        }

        private void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!PolyClosed)
            {
                MyPoint _A = new MyPoint(Convert.ToInt32(Math.Floor(e.GetPosition(g).X / 10)),
                                         Convert.ToInt32(Math.Floor(e.GetPosition(g).Y / 10)));
                Vert.Add(_A);

                if (_A.Y > MaxY) MaxY = _A.Y;
                if (MinY > _A.Y) MinY = _A.Y;


                g.Children.Clear();
                DrawRast();
                DrawPoints();
                DrawLines();
            }
        }

        //vykreslenie všetkých bodov hlavného riadiaceho polygónu
        public void DrawPoints()
        {
            for (int i = 0; i < Vert?.Count; i++)
            {

                Ellipse E = new Ellipse
                {
                    Fill = new SolidColorBrush(Colors.Black),
                    Width = 8,
                    Height = 8
                };

                Canvas.SetLeft(E, (Vert[i].X + 0.5) * 10 - 4);
                Canvas.SetTop(E, (Vert[i].Y + 0.5) * 10 - 4);
                Canvas.SetZIndex(E, i + 50);
                g.Children.Add(E);
            }
        }

        //pomocná metóda na vykreslenie úsečky
        public void DrawLine(MyPoint V_1, MyPoint V_2, SolidColorBrush b)
        {
            Line L = new Line
            {
                Stroke = b,
                X1 = (V_1.X + 0.5) * 10,
                X2 = (V_2.X + 0.5) * 10,
                Y1 = (V_1.Y + 0.5) * 10,
                Y2 = (V_2.Y + 0.5) * 10
            };

            Canvas.SetZIndex(L, 50);
            g.Children.Add(L);
        }

        //vykreslenie úsečiek hlavného riadiaceho polygónu
        public void DrawLines()
        {
            if (Vert?.Count > 1)
            {
                for (int i = 0; i < Vert.Count - 1; i++)
                {
                    DrawLine(Vert[i], Vert[i + 1], new SolidColorBrush(Colors.Green));
                }

                if (PolyClosed && Vert[Vert.Count - 1] != Vert[Vert.Count - 2])
                {
                    DrawLine(Vert[Vert.Count - 1], Vert[Vert.Count - 2], new SolidColorBrush(Colors.Green));
                }
            }
        }

        private void DrawRast()
        {
            for (int i = 0; i < RastHeight; i++)
            {
                Point W1 = new Point(0, i);
                Point W2 = new Point(RastWidth, i);
                SolidColorBrush b = new SolidColorBrush(Colors.LightGray);

                Line LW = new Line
                {
                    Stroke = b,
                    X1 = (W1.X) * 10,
                    X2 = (W2.X) * 10,
                    Y1 = (W1.Y) * 10,
                    Y2 = (W2.Y) * 10
                };

                Canvas.SetZIndex(LW, 49);
                g.Children.Add(LW);
            }
            
            for (int i = 0; i < RastWidth; i++)
            {
                Point H1 = new Point(i, 0);
                Point H2 = new Point(i, RastHeight);
                SolidColorBrush b = new SolidColorBrush(Colors.LightGray);


                Line LH = new Line
                {
                    Stroke = b,
                    X1 = (H1.X) * 10,
                    X2 = (H2.X) * 10,
                    Y1 = (H1.Y) * 10,
                    Y2 = (H2.Y) * 10
                };

                Canvas.SetZIndex(LH, 49);
                g.Children.Add(LH);
            }
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public void DrawPixels(MyPoint A, MyPoint B)
        {
            Rectangle R = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Coral),
                Height = 10,
                Width = (B.X - A.X + 1) * 10
            };
            Canvas.SetLeft(R, A.X * 10);
            Canvas.SetTop(R, A.Y * 10);
            g.Children.Add(R);
        }

        private void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!PolyClosed && Vert.Count > 1)
            {
                List<MyEdge> Poly = new List<MyEdge>();
                List<List<Polyside>> TE = new List<List<Polyside>>();
                List<Polyside> TAE = new List<Polyside>();


                PolyClosed = true;

                if (Vert[0] != Vert.Last()) Vert.Add(Vert[0]);

                g.Children.Clear();
                DrawRast();
                DrawPoints();
                DrawLines();

                for (int i = 0; i < Vert.Count - 1; i++)
                {
                    //pridavame len hrany, ktore nie su rovnobezne s y
                    if (Vert[i].Y != Vert[i + 1].Y)
                    {
                        Poly.Add(new MyEdge(Vert[i], Vert[i + 1]));
                    }
                    else
                    {
                        //vykreslit hranu

                        if (Vert[i].X < Vert[i + 1].X) DrawPixels(Vert[i], Vert[i + 1]);
                        else DrawPixels(Vert[i + 1], Vert[i]);

                    }
                }

                //odstranenie neextremalnych vrcholov
                for (int i = 0; i < Poly.Count; i++)
                {
                    if ((Poly[mod(i, Poly.Count)].A.Y - Poly[mod(i, Poly.Count)].B.Y) *
                        (Poly[mod(i + 1, Poly.Count)].B.Y - Poly[mod(i + 1, Poly.Count)].A.Y) < 0)
                    {
                        if (Poly[mod(i, Poly.Count)].yMax > Poly[mod(i + 1, Poly.Count)].yMax)
                        {
                            Poly[mod(i, Poly.Count)].ShortenEdge();
                        }
                        else
                        {
                            Poly[mod(i + 1, Poly.Count)].ShortenEdge();
                        }
                    }
                }

                //inicializacia TE
                for (int i = MinY; i <= MaxY; i++)
                {
                    List<Polyside> tmp = new List<Polyside>();
                    foreach (MyEdge E in Poly)
                    {
                        if (i == E.ReturnYMin()) tmp.Add(new Polyside(E.ReturnYMax(), E.ReturnXofYMin(), E.ReturnRecm()));
                    }

                    TE.Add(tmp);
                }

                int y = MinY;
                while (TE.Count != 0 || TAE.Count != 0)
                {
                    foreach (Polyside E in TE[0])
                    {
                        TAE.Add(E);
                    }
                    TE.RemoveAt(0);

                    TAE = TAE.OrderBy(o => o.xOfyMin).ToList();

                    int j = 0;
                    int noAdd = 0;
                    while (noAdd < TAE.Count / 2)
                    {
                        DrawPixels(new MyPoint(Convert.ToInt32(Math.Round(TAE[j].xOfyMin)), y),
                                   new MyPoint(Convert.ToInt32(Math.Round(TAE[j + 1].xOfyMin)), y));
                        j = j + 2; ;
                        noAdd++;
                    }

                    /*
                    for (int i = 0; i <= TAE.Count / 2; i = i + 2)
                    {
                        DrawPixels(new MyPoint(Convert.ToInt32(Math.Round(TAE[i].xOfyMin)), y),
                                   new MyPoint(Convert.ToInt32(Math.Round(TAE[i + 1].xOfyMin)), y));
                    }
                    */
                    for (int i = 0; i < TAE.Count; i++)
                    {
                        if (TAE[i].yMax == y)
                        {
                            TAE.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            TAE[i].ShortenPolyside();
                        }
                    }
                    y++;
                }
            }
            
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Vert.Clear();
            PolyClosed = false;
            MaxY = int.MinValue;
            MinY = int.MaxValue;
            g.Children.Clear();
            DrawRast();
        }
    }
}
