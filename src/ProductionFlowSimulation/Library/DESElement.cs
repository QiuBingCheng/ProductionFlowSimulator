using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace DiscreteEventSimulationLibrary
{
    public class DESElement
    {
        protected static Font ElementFont = new Font("consola",12);
        private string name;
        private static int HandleSize = 3;

        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public Rectangle Bound { get; set; } = new Rectangle(10, 10, 100, 50);

        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public string Name { get => name; set => name = value; }

        [CategoryAttribute("Display"), DescriptionAttribute("")]
        public Color BackColor = Color.Gray;
       
        public virtual void Draw(Graphics g)
        {
            g.DrawRectangle(Pens.Gray, Bound);
        }
        public Point GetCenterPoint ()
        {
            return new Point(Bound.Left + Bound.Width / 2,
                             Bound.Top + Bound.Height / 2);
        }
        public int hashCode;
        public void DrawSelectionHandles(Graphics g)
        {
            // four corners
            Rectangle rect = Rectangle.Empty;
            rect.Width = rect.Height = HandleSize + HandleSize;
            rect.X = Bound.Left - HandleSize;
            rect.Y = Bound.Top - HandleSize;
            g.FillRectangle(Brushes.Black, rect);
            rect.Y = Bound.Bottom - HandleSize;
            g.FillRectangle(Brushes.Black, rect);
            rect.X = Bound.Right - HandleSize;
            g.FillRectangle(Brushes.Black, rect);
            rect.Y = Bound.Top - HandleSize;
            g.FillRectangle(Brushes.Black, rect);

            //emphasized edge
            Pen grayPen = new Pen(Color.Gray, 1);
            grayPen.DashStyle = DashStyle.Dash;
            g.DrawRectangle(grayPen,Bound);

        }

        public virtual void SaveToFile(StreamWriter sw)
        {
            sw.WriteLine($"Name: {Name}");
            sw.WriteLine($"HashCode: {this.GetHashCode()}");
            sw.WriteLine($"Bound: {Bound.X} {Bound.Y} {Bound.Width} {Bound.Height}");
            sw.WriteLine($"BackColor: {BackColor.ToArgb()}");
        }

        public virtual void ReadFromFile(StreamReader sr)
        {
            string str = sr.ReadLine();
            Name = str.Substring(str.IndexOf(":") + 1).Trim();

            str = sr.ReadLine();
            hashCode = int.Parse(str.Substring(str.IndexOf(":") + 1).Trim());

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            string[] items = str.Split(new char[] { ' '},StringSplitOptions.RemoveEmptyEntries);
            Bound = new Rectangle(int.Parse(items[0]), int.Parse(items[1]), int.Parse(items[2]), int.Parse(items[3]));

            str = sr.ReadLine();
            str = str.Substring(str.IndexOf(":") + 1).Trim();
            BackColor = Color.FromArgb(int.Parse(str));
        }
    }
}
