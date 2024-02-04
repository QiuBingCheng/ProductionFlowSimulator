using System;
using System.Drawing;

namespace DiscreteEventSimulationLibrary
{
    public class GraphicUtil
    {
        public static PointF[] DrawRegularPoly(Point center, double radius, int sideCount)
        {
            // 多邊形至少要有3條邊，邊數不達標就返回。
            if (sideCount < 3)
                return null;


            // 每條邊對應的圓心角角度，精確為浮點數。使用弧度制，360度角為2派
            double arc = 2 * Math.PI / sideCount;
            // 為多邊形建立所有的頂點列表
            PointF[] points = new PointF[sideCount];
            for (int i = 0; i < sideCount; i++)
            {
                var curArc = arc * i; // 當前點對應的圓心角角度rectangle
                PointF pt = new PointF();
                // 就是簡單的三角函式正餘弦根據圓心角和半徑算點座標。這裡都取整就行
                pt.X = (int)(center.X + Math.Round((radius * Math.Cos(curArc)), 2));
                pt.Y = (int)(center.Y + Math.Round((radius * Math.Sin(curArc)), 2));
                points[i] = pt;
            }

            return points;
        }
       
        public enum Shape { Triangle,Circle};

        static public void DrawSpecificType(Shape shape, Rectangle rectangle, Color color, Graphics g)
        {
            // draw in rectangle 
            switch (shape)
            {
                case GraphicUtil.Shape.Circle:
                    g.FillEllipse(new SolidBrush(Color.White), rectangle);
                    g.FillEllipse(new SolidBrush(color), rectangle);
                    g.DrawEllipse(Pens.Black, rectangle);
                    break;
                case GraphicUtil.Shape.Triangle:
                    PointF[] points = new PointF[3];
                    points[0].X = rectangle.X + rectangle.Width / 2;
                    points[0].Y = rectangle.Y;
                    points[1].X = rectangle.X;
                    points[1].Y = rectangle.Bottom;
                    points[2].X = rectangle.Right;
                    points[2].Y = rectangle.Bottom;
                    g.FillPolygon(new SolidBrush(Color.White), points);
                    g.FillPolygon(new SolidBrush(color), points);
                    g.DrawPolygon(Pens.Black, points);
                    break;
            }
        }
    }

}
