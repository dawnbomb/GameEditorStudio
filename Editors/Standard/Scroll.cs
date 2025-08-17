using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameEditorStudio
{
    class Scroll(ScrollViewer ScrollViewer)
    {

        //ScrollViewer ScrollViewer = new(); //This is the scrollviewer that allows the user to scroll through the editor.

        bool isMiddleMouseDragging = false;
        Point mouseDragStartPoint;
        Point scrollStartOffset;


        public void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                isMiddleMouseDragging = true;
                mouseDragStartPoint = e.GetPosition(ScrollViewer);
                scrollStartOffset = new Point(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset);
                ScrollViewer.CaptureMouse();
                ScrollViewer.Cursor = Cursors.ScrollAll;
                e.Handled = true;
            }
        }

        public void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                isMiddleMouseDragging = false;
                ScrollViewer.ReleaseMouseCapture();
                ScrollViewer.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        public void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMiddleMouseDragging)
            {
                Point currentMousePosition = e.GetPosition(ScrollViewer);
                Vector delta = currentMousePosition - mouseDragStartPoint;

                ScrollViewer.ScrollToHorizontalOffset(scrollStartOffset.X - delta.X);
                ScrollViewer.ScrollToVerticalOffset(scrollStartOffset.Y - delta.Y);
            }
        }

        public void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            // Adjust scrolling speed here
            double scrollAmount = e.Delta > 0 ? -30 : 30;

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);

            // Mark event as handled so other controls don't block it
            e.Handled = true;
        }
    }
}
