using Microsoft.Maui.Graphics;

namespace AppTodoPro.Views;

public class PriorityPieChartView : GraphicsView
{
    public static readonly BindableProperty HighCountProperty =
        BindableProperty.Create(nameof(HighCount), typeof(int), typeof(PriorityPieChartView), 0, propertyChanged: OnCountsChanged);

    public static readonly BindableProperty MediumCountProperty =
        BindableProperty.Create(nameof(MediumCount), typeof(int), typeof(PriorityPieChartView), 0, propertyChanged: OnCountsChanged);

    public static readonly BindableProperty LowCountProperty =
        BindableProperty.Create(nameof(LowCount), typeof(int), typeof(PriorityPieChartView), 0, propertyChanged: OnCountsChanged);

    private readonly PriorityPieDrawable drawable;

    public PriorityPieChartView()
    {
        drawable = new PriorityPieDrawable();
        Drawable = drawable;
    }

    public int HighCount
    {
        get => (int)GetValue(HighCountProperty);
        set => SetValue(HighCountProperty, value);
    }

    public int MediumCount
    {
        get => (int)GetValue(MediumCountProperty);
        set => SetValue(MediumCountProperty, value);
    }

    public int LowCount
    {
        get => (int)GetValue(LowCountProperty);
        set => SetValue(LowCountProperty, value);
    }

    private static void OnCountsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PriorityPieChartView view)
        {
            view.drawable.SetCounts(view.HighCount, view.MediumCount, view.LowCount);
            view.Invalidate();
        }
    }

    private sealed class PriorityPieDrawable : IDrawable
    {
        private int high;
        private int medium;
        private int low;

        public void SetCounts(int high, int medium, int low)
        {
            this.high = high;
            this.medium = medium;
            this.low = low;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var total = high + medium + low;
            var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - 10;
            var x = dirtyRect.Center.X - size / 2;
            var y = dirtyRect.Center.Y - size / 2;

            canvas.StrokeColor = Colors.Transparent;

            if (total == 0)
            {
                canvas.FillColor = Colors.LightGray;
                canvas.FillCircle(dirtyRect.Center.X, dirtyRect.Center.Y, size / 2);
                return;
            }

            var startAngle = -90.0f;
            startAngle = DrawSlice(canvas, x, y, size, startAngle, high, total, Color.FromArgb("#D32F2F"));
            startAngle = DrawSlice(canvas, x, y, size, startAngle, medium, total, Color.FromArgb("#FB8C00"));
            DrawSlice(canvas, x, y, size, startAngle, low, total, Color.FromArgb("#43A047"));
        }

        private static float DrawSlice(ICanvas canvas, float x, float y, float size, float startAngle, int count, int total, Color color)
        {
            if (count == 0)
            {
                return startAngle;
            }

            var sweep = 360f * count / total;
            canvas.FillColor = color;
            canvas.FillArc(x, y, size, size, startAngle, sweep, true);
            return startAngle + sweep;
        }
    }
}
