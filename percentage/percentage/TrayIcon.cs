using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const int fontSize = 22;
        private const string font = "Segoe UI";

        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            menuItem.Click += new System.EventHandler(MenuItemClick);
            menuItem.Index = 0;
            menuItem.Text = "E&xit";

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Interval = 10000;
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
            TimerTick(null, null);
        }

        private Bitmap GetTextBitmap(String text, Font font, Color fontColor)
        {
            SizeF imageSize = GetStringImageSize(text, font);
            Bitmap bitmap = new Bitmap((int)imageSize.Width, (int)imageSize.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                using (Brush brush = new SolidBrush(fontColor))
                {
                    graphics.DrawString(text, font, brush, 0, 0);
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.Save();
                }
            }
            return bitmap;
        }

        private static SizeF GetStringImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float percent = powerStatus.BatteryLifePercent * 100;
            String percentStr = (percent).ToString();
            bool isCharging = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
            Color color = Color.White;
            if (isCharging)
                color = Color.FromArgb(255, 0, 230, 0);
            else if (percent <= 20)
                color = Color.FromArgb(255, 255, 51, 0);
            else if (percent <= 50)
                color = Color.FromArgb(255, 255, 204, 0);

            using (Bitmap bitmap = new Bitmap(GetTextBitmap(percentStr, new Font(font, fontSize, FontStyle.Bold), color)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        String toolTipText = percentStr + "%";
                        if (isCharging)
                            toolTipText += ", Charging...";
                        else if (powerStatus.BatteryLifeRemaining != -1)
                            toolTipText += ", " + getReadableTime(powerStatus.BatteryLifeRemaining);
                        notifyIcon.Text = toolTipText;
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }
        private String getReadableTime(int totalSeconds)
        {
            if (totalSeconds <= 0) return "Null";

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            if (hours > 0) 
                return hours.ToString() + "h " + minutes.ToString() + "m";
            else if (minutes > 0)
                return minutes.ToString() + "m " + seconds.ToString() + "s";
            else
                return seconds.ToString() + "s";
        }


    }
}
