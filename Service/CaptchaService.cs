using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace BASE.Service
{
    public static class CaptchaService
    {
        /// <summary>
        /// 產生驗證碼
        /// </summary>
        /// <param name="length">驗證碼長度</param>
        /// <param name="IsOnlyNumber">是否僅數字顯示</param>
        /// <returns>string 驗證碼</returns>
        public static string CreateCheckCode(int length = 4, bool IsOnlyNumber = false)
        {
            string chkCode = string.Empty;

            #region Random的問題與解決方式
            /* 
             * Random生成的隨機數被稱為偽隨機數，因為用Random生成隨機數時，需要用到一個"種子"，
             * 而使用相同的種子，一定會產生相同序列的數字。例如：
             * Random r1 = new Random(1);
             * Console.WriteLine(r1.Next(100));	// 24
             * Random r2 = new Random(1);
             * Console.WriteLine(r2.Next(100));	// 24
             * 若種子外流，就能讓入侵者模擬出一樣的隨機亂數，改用"RandomNumberGenerator"
             */
            #endregion

            //驗證碼符號集合(容易混淆的字已清除)
            char[] character ={
            '2', '3', '4', '5', '6', '8', '9',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };

            if (IsOnlyNumber)
            {
                character = new char[] { '1', '2', '3', '4', '5', '6', '8', '9', '0' };
            }

            //隨機產生驗證碼
            for (int i = 0; i < length; i++)
            {
                chkCode += character[RandomNumberGenerator.GetInt32(character.Length)];
            }
            return chkCode;
        }

        /// <summary>
        /// 產生圖形驗證碼
        /// </summary>
        /// <param name="chkCode">驗證碼</param>
        /// <returns>MemoryStream 記憶體串流</returns>
        public static MemoryStream Create(string chkCode)
        {
            //躁點顏色
            Color[] DotColor = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.DarkBlue };

            //字體列表(去掉粗體字型避免字元縮在一起)
            string[] font = { "Times New Roman", "MS Mincho", "Book Antiqua", "Gungsuh", "PMingLiU" }; //"Impact"

            //圖片寬高
            //Random rnd = new Random();
            int ImageWidth = 125;
            int ImageHeight = 25;

            using (MemoryStream ms = new MemoryStream())
            using (Bitmap bmp = new Bitmap(ImageWidth, ImageHeight))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                //圖片背景為透明
                g.Clear(Color.Transparent);

                //畫雜訊線
                for (int i = 0; i < 5; i++)
                {
                    int x1 = RandomNumberGenerator.GetInt32(ImageWidth);
                    int y1 = RandomNumberGenerator.GetInt32(ImageHeight);
                    int x2 = RandomNumberGenerator.GetInt32(ImageWidth);
                    int y2 = RandomNumberGenerator.GetInt32(ImageHeight);

                    Color clr = Color.FromArgb(155, RandomNumberGenerator.GetInt32(255), RandomNumberGenerator.GetInt32(255), RandomNumberGenerator.GetInt32(255));
                    g.DrawLine(new Pen(clr), x1, y1, x2, y2);
                }

                //畫圓
                for (int i = 0; i < 5; i++)
                {
                    int x = RandomNumberGenerator.GetInt32(ImageWidth);
                    int y = RandomNumberGenerator.GetInt32(ImageHeight);
                    int CircleSize = RandomNumberGenerator.GetInt32(60);
                    int height = CircleSize;
                    int width = CircleSize;

                    Color CircleColor = Color.FromArgb(100, RandomNumberGenerator.GetInt32(255), RandomNumberGenerator.GetInt32(255), RandomNumberGenerator.GetInt32(255));
                    g.FillEllipse(new SolidBrush(CircleColor), x, y, width, height); //實心圓
                }

                //畫驗證碼字元
                for (int i = 0; i < chkCode.Length; i++)
                {
                    string fnt = font[RandomNumberGenerator.GetInt32(font.Length)];
                    Font ft = new Font(fnt, 16, FontStyle.Bold);

                    Color clr = Color.FromArgb(RandomNumberGenerator.GetInt32(100), RandomNumberGenerator.GetInt32(100), RandomNumberGenerator.GetInt32(100)); //使字形顏色分布於暗色系
                    g.DrawString(chkCode[i].ToString(), ft, new SolidBrush(clr), (float)i * 20 + 10, (float)0);
                }

                //畫噪點
                for (int i = 0; i < 100; i++)
                {
                    int x = RandomNumberGenerator.GetInt32(bmp.Width);
                    int y = RandomNumberGenerator.GetInt32(bmp.Height);

                    Color clr = DotColor[RandomNumberGenerator.GetInt32(DotColor.Length)];
                    bmp.SetPixel(x, y, clr);
                }

                //儲存圖片變更
                bmp.Save(ms, ImageFormat.Png);
                return ms; //回傳串流
            }
        }
    }
}
