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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Threading;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography.X509Certificates;

namespace GraphProgram
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point clickPosition;
        private Canvas currentDraggingNode;
        private Dictionary<string,Canvas> vertices= new Dictionary<string,Canvas>();
        private List<ArrowConnection> connections = new List<ArrowConnection>();
        private List<TextBox> textBoxes = new List<TextBox>();
        private List<Label> labels = new List<Label>();
        List<Brush> brushList = new List<Brush>
        {
            Brushes.Black,        
            Brushes.Blue,         
            Brushes.Green,        
            Brushes.Red,         
            Brushes.Cyan,         
            Brushes.Purple,
            Brushes.Azure,
            Brushes.Brown,
            Brushes.Crimson
        };

        public MainWindow()
        {
            InitializeComponent();

        }

        private void CreateDynamicTable(int n)
        {
            int startX = 100;
            int startY = 100;
            for(int i = 0;i!=n;i++)
            {
                for (int j = 0;j!=n;j++)
                {
                    if(i==0)
                    {
                        Label lb = new Label
                        {
                            Width = 30,
                            Height = 30,
                            Content = $"V{j + 1}",
                            FontSize = 12
                        };
                        Canvas.SetLeft(lb, startX + (j * 21));
                        Canvas.SetTop(lb, startX - 21);
                        labels.Add(lb);
                        MainCanvas.Children.Add(lb);
                    }

                    if(j==0)
                    {
                        Label lb = new Label
                        {
                            Width = 30,
                            Height = 30,
                            Content = $"V{i + 1}",
                            FontSize = 12
                        };
                        Canvas.SetLeft(lb, startX - 30);
                        Canvas.SetTop(lb, startX + (i * 21));
                        labels.Add(lb);
                        MainCanvas.Children.Add(lb);
                    }
                    TextBox tb = new TextBox
                    {
                        Width = 20,
                        Height = 20,
                        Name = $"V{j+1}_V{i+1}"
                    };
                    Canvas.SetLeft(tb, startX+(i*21));
                    Canvas.SetTop(tb, startY+(j*21));
                    tb.TextChanged += (sender, e) => OnTbTextChanged(sender);
                    textBoxes.Add(tb);
                    MainCanvas.Children.Add(tb);
                }
            }
        }

        private void CreateVertices(int n)
        {
            int x = 50;
            int y = 800;
            for (int i = 0; i != n; i++)
            {
                if ((i + 1) % 2 == 0)
                {
                    y += 200;
                    Canvas c = CreateVerticeWithLabel(x, y, i + 1);
                    vertices.Add("V" + (i + 1), c);
                }
                else
                {
                    y -= 200;
                    x += 200;

                    Canvas c = CreateVerticeWithLabel(x, y, i + 1);
                    vertices.Add("V" + (i + 1), c);
                }

            }
        }

        private void OnTbTextChanged(object sender)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string[] vertices = textBox.Name.Split('_');
                if(int.TryParse(textBox.Text, out int n))
                {
                    if(n == 0)
                    {
                        RemoveOldLine(vertices);
                        
                    }
                    else
                    {
                        RemoveOldLine(vertices);
                        CreateDynamicArrow(vertices[0], vertices[1],n);
                    }
                }
                else
                {
                    if(textBox.Text == "")
                    {
                        RemoveOldLine(vertices);
                    }
                }
            }
        }

        private void RemoveOldLine(string[] vertices)
        {
            var toRemove = connections
                        .Where(c => c.FromNodeNumber == vertices[0] && c.ToNodeNumber == vertices[1])
                        .ToList();
            foreach (var c in toRemove)
            {
                c.Dispose();
                connections.Remove(c);
            }
        }

        private void CreateDynamicArrow(string fr, string t, int countOfLines = 1)
        {
            Canvas from = vertices[fr];
            Canvas to = vertices[t];
            Brush ultiBrush = null;
            for(int i =0; i!= countOfLines;i++)
            {
                try
                {
                    ultiBrush = brushList[i];
                }
                catch
                {
                    ultiBrush = Brushes.Black;
                }
            }
            Line line = new Line
            {
                Stroke = ultiBrush,
                StrokeThickness = 2
            };

            Polygon arrowHead = new Polygon
            {
                Fill = ultiBrush,
                Stroke = ultiBrush,
                StrokeThickness = 8
            };

            MainCanvas.Children.Add(line);
            MainCanvas.Children.Add(arrowHead);

            TextBlock label = null;

            if (countOfLines > 1)
            {
                label = new TextBlock
                {
                    Text = countOfLines.ToString(),
                    Foreground = ultiBrush,
                    Background = Brushes.Transparent,
                    FontWeight = FontWeights.Bold
                };
                MainCanvas.Children.Add(label);
            }

            var connection = new ArrowConnection
            {
                FromNode = from,
                ToNode = to,
                Line = line,
                ArrowHead = arrowHead,
                FromNodeNumber = fr,
                ToNodeNumber = t,
                Label = label,
                LineColor = ultiBrush
            };

            if (label != null)
            {
                connection.Label = label; 
            }

            connection.Update();
            connection.Update();
            connections.Add(connection);
        }

        private Canvas CreateVerticeWithLabel(double x, double y, int number)
        {
            double size = 50;

            Canvas nodeCanvas = new Canvas
            {
                Width = size,
                Height = size,
                Background = Brushes.Transparent,
                Tag = number 
            };

            Ellipse ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, 0);
            Canvas.SetTop(ellipse, 0);
            nodeCanvas.Children.Add(ellipse);

            TextBlock label = new TextBlock
            {
                Text = "V" + number,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };

            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size textSize = label.DesiredSize;

            Canvas.SetLeft(label, (size - textSize.Width) / 2);
            Canvas.SetTop(label, (size - textSize.Height) / 2);
            nodeCanvas.Children.Add(label);

            Canvas.SetLeft(nodeCanvas, x);
            Canvas.SetTop(nodeCanvas, y);
            MainCanvas.Children.Add(nodeCanvas);


            nodeCanvas.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            nodeCanvas.MouseMove += Node_MouseMove;
            nodeCanvas.MouseLeftButtonUp += Node_MouseLeftButtonUp;

            return nodeCanvas;
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentDraggingNode = sender as Canvas;
            if (currentDraggingNode != null)
            {
                isDragging = true;
                clickPosition = e.GetPosition(MainCanvas);
                currentDraggingNode.CaptureMouse();
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && currentDraggingNode != null)
            {
                Point currentPosition = e.GetPosition(MainCanvas);
                double offsetX = currentPosition.X - clickPosition.X;
                double offsetY = currentPosition.Y - clickPosition.Y;

                double newX = Canvas.GetLeft(currentDraggingNode) + offsetX;
                double newY = Canvas.GetTop(currentDraggingNode) + offsetY;

                Canvas.SetLeft(currentDraggingNode, newX);
                Canvas.SetTop(currentDraggingNode, newY);

                clickPosition = currentPosition;

                foreach (var conn in connections)
                {
                    if (conn.FromNode == currentDraggingNode || conn.ToNode == currentDraggingNode)
                    {
                        conn.Update();
                    }
                }
            }
            
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDraggingNode != null)
            {
                isDragging = false;
                currentDraggingNode.ReleaseMouseCapture();
                currentDraggingNode = null;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckInput(CountOfV.Text);
        }

        private void CheckInput(string input)
        {
            if (int.TryParse(input, out int n))
            {
                if (n <= 19 && n > 1)
                {
                    foreach (var tb in textBoxes)
                    {
                        MainCanvas.Children.Remove(tb);
                    }
                    foreach (var v in vertices)
                    {
                        MainCanvas.Children.Remove(v.Value);
                    }
                    foreach (var l in labels)
                    {
                        MainCanvas.Children.Remove(l);
                    }
                    foreach(var c in connections)
                    {
                        MainCanvas.Children.Remove(c.Label);
                        MainCanvas.Children.Remove(c.Line);
                        MainCanvas.Children.Remove(c.ArrowHead);
                        MainCanvas.Children.Remove(c.LoopPath);
                    }
                    vertices.Clear();
                    labels.Clear();
                    textBoxes.Clear();
                    connections.Clear();
                    CreateDynamicTable(n);
                    CreateVertices(n);

                }
                else
                {
                    MessageBox.Show($"Кількість вершин 2-10");
                }
            }
            else
            {
                MessageBox.Show($"Кількість вершин 2-10");
            }
        }

        public class ArrowConnection
        {
            public Canvas FromNode { get; set; }
            public string FromNodeNumber { get; set; }
            public Canvas ToNode { get; set; }
            public string ToNodeNumber { get; set; }
            public Line Line { get; set; }
            public Polygon ArrowHead { get; set; }
            public Path LoopPath { get; set; }
            public TextBlock Label { get; set; }
            public Brush LineColor { get; set; }
            public void Dispose()
            {
                var parent = (Panel)FromNode?.Parent;

                if (parent != null)
                {
                    if (Line != null && parent.Children.Contains(Line))
                        parent.Children.Remove(Line);

                    if (ArrowHead != null && parent.Children.Contains(ArrowHead))
                        parent.Children.Remove(ArrowHead);

                    if (LoopPath != null && parent.Children.Contains(LoopPath))
                        parent.Children.Remove(LoopPath);
                    if (Label != null && parent != null && parent.Children.Contains(Label))
                        parent.Children.Remove(Label);
                }
                Label = null;
                Line = null;
                ArrowHead = null;
                LoopPath = null;
            }
            public void Update()
            {
                if (FromNode == null || ToNode == null) return;

                if (FromNode == ToNode)
                {
                    if (LoopPath == null)
                    {
                        LoopPath = new Path
                        {
                            Stroke = LineColor,
                            StrokeThickness = 2
                        };
                    }

                    Point center = new Point(Canvas.GetLeft(FromNode) + FromNode.Width / 2,
                                             Canvas.GetTop(FromNode) + FromNode.Height / 2);

                    double radius = FromNode.Width / 2;

                    Point start = new Point(center.X, center.Y - radius);

                    Point end = new Point(center.X, center.Y + radius);

                    Point control1 = new Point(center.X - 60, center.Y - 50);
                    Point control2 = new Point(center.X - 60, center.Y + 75);

                    PathFigure figure = new PathFigure { StartPoint = start };
                    BezierSegment bezier = new BezierSegment(control1, control2, end, true);
                    figure.Segments.Add(bezier);

                    PathGeometry geometry = new PathGeometry();
                    geometry.Figures.Add(figure);
                    LoopPath.Data = geometry;

                    if (ArrowHead == null)
                    {
                        ArrowHead = new Polygon
                        {
                            Fill = LineColor,
                            Stroke = LineColor,
                            StrokeThickness = 2
                        };
                    }

                    Vector direction = end - control2;
                    direction.Normalize();
                    Vector ortho = new Vector(-direction.Y, direction.X);

                    Point p1 = end;
                    Point p2 = end - direction * 10 + ortho * 5;
                    Point p3 = end - direction * 10 - ortho * 5;

                    ArrowHead.Points.Clear();
                    ArrowHead.Points.Add(p1);
                    ArrowHead.Points.Add(p2);
                    ArrowHead.Points.Add(p3);

                    if (Label != null)
                    {
                        Point labelCenter = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Canvas.SetLeft(Label, labelCenter.X - Label.ActualWidth / 2 - 50);
                            Canvas.SetTop(Label, labelCenter.Y - Label.ActualHeight / 2);
                        }), System.Windows.Threading.DispatcherPriority.Loaded);
                    }

                    if (!((Panel)FromNode.Parent).Children.Contains(LoopPath))
                        ((Panel)FromNode.Parent).Children.Add(LoopPath);

                    if (!((Panel)FromNode.Parent).Children.Contains(ArrowHead))
                        ((Panel)FromNode.Parent).Children.Add(ArrowHead);
                }
                else
                {
                    if (Label != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            double labelX = (Line.X1 + Line.X2) / 2;
                            double labelY = (Line.Y1 + Line.Y2) / 2;

                            Vector direction1 = new Vector(Line.X2 - Line.X1, Line.Y2 - Line.Y1);
                            direction1.Normalize();

                            Vector orthogonal = new Vector(-direction1.Y, direction1.X);

                            double offset = 15; 
                            labelX += orthogonal.X * offset;
                            labelY += orthogonal.Y * offset;

                            Canvas.SetLeft(Label, labelX - Label.ActualWidth / 2);
                            Canvas.SetTop(Label, labelY - Label.ActualHeight / 2);
                        }), System.Windows.Threading.DispatcherPriority.Loaded);
                    }

                    Point center1 = new Point(Canvas.GetLeft(FromNode) + FromNode.Width / 2,
                                                  Canvas.GetTop(FromNode) + FromNode.Height / 2);
                        Point center2 = new Point(Canvas.GetLeft(ToNode) + ToNode.Width / 2,
                                                  Canvas.GetTop(ToNode) + ToNode.Height / 2);

                        Vector direction = center2 - center1;
                        direction.Normalize();

                        double radius = FromNode.Width / 2;
                        Point start = center1 + direction * radius;
                        Point end = center2 - direction * radius;

                        Line.X1 = start.X;
                        Line.Y1 = start.Y;
                        Line.X2 = end.X;
                        Line.Y2 = end.Y;


                        Vector ortho = new Vector(-direction.Y, direction.X);
                        Point p1 = end;
                        Point p2 = end - direction * 10 + ortho * 5;
                        Point p3 = end - direction * 10 - ortho * 5;

                        ArrowHead.Points.Clear();
                        ArrowHead.Points.Add(p1);
                        ArrowHead.Points.Add(p2);
                        ArrowHead.Points.Add(p3);
                    
                }
            }
        }
    }
}
