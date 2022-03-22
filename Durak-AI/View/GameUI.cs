using Cairo;
using Gdk;
using Gtk;
using System;

namespace View
{
    class Area : DrawingArea
    {
        bool draw = true;           // model

        public Area()
        {
            AddEvents((int)EventMask.ButtonPressMask);
        }

        protected override bool OnButtonPressEvent(EventButton e)
        {
            draw = !draw;
            QueueDraw();
            return true;
        }

        protected override bool OnDrawn(Context c)
        {
            if (draw)
            {
                c.SetSourceRGB(0.5, 0.5, 0.0);  // olive color
                c.Arc(250, 250, 150, 0.0, 2 * Math.PI);
                c.Fill();
            }
            return true;
        }
    }

    public class GameUI : Gtk.Window
    {
        public GameUI() : base("circle")
        {
            Resize(500, 500);
            Add(new Area());
        }

        protected override bool OnDeleteEvent(Event ev)
        {
            Application.Quit();
            return true;
        }

        public static void StartWindow()
        {
            Application.Init();
            GameUI w = new GameUI();
            w.ShowAll();
            Application.Run();
        }
    }
}
