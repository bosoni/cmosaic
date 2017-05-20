/*
CMosaic - (c) mjt, 2015-2016
 
* valkataan joku kuva joka luodaan muista kuvista
* valkataan läjä kuvia
  * otetaan kuvien keskeltä esim 512x512 kokoinen kuva
  * skaalataan se pienemmäks esim 64x64 kokoiseksi
nyt pitäis luoda uusi kuva joka näyttää ekalta valitulta kuvalta,
ja mikä on rakennettu WxH kuvista.


 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace CMosaic
{
    class Mosaic
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("CMosaic v0.1 (c) mjt, 2015-2016 [mixut@hotmail.com]");

            int W = 32, H = 32;
            string DIRNAME = "temp" + W;
            int pixelSize = 3;

            String[] ffiles = null;

            if (args.Length > 1) // jos annettu hakemisto
            {
                ffiles = Directory.GetFiles(args[1]);
                if (ffiles.Length == 0) // jos kuvahakemisto on tyhjä
                {
                    Console.Out.WriteLine("Note: put some .jpg images in " + args[1] + " directory.");
                    return;
                }
            }

            if (args.Length < 2)
            {
                Console.Out.WriteLine("usage:  mainImage  [imagesDir]");
                return;
            }

            Directory.CreateDirectory(DIRNAME);
            Console.Out.WriteLine("Converting images...");

            foreach (String f in ffiles)
            {
                if (f.ToLower().Contains(".jpg") == false)
                    continue;

                String tf = f;
                tf = tf.Replace('\\', '/');
                if (tf.Contains("/"))
                {
                    tf = tf.Substring(tf.LastIndexOf('/'));
                }

                Bitmap resized = null;
                try
                {
                    if (File.Exists(DIRNAME + "/" + tf) == false)
                    {
                        resized = new Bitmap((Bitmap)Image.FromFile(f), new Size(W, H));
                        resized.Save(DIRNAME + "/" + tf);
                        resized.Dispose();

                        Console.Out.Write(".");
                    }
                }
                catch (Exception e)
                {
                    if (resized != null)
                        resized.Dispose();

                    Console.Out.Write("x");
                }
            }

            System.GC.Collect();

            String mainImageName = args[0];  // tähän malliin koitetaan luoda kuva muista kuvista
            Bitmap mainImage = (Bitmap)Image.FromFile(mainImageName);
            BitmapData mainBmpData = mainImage.LockBits(new Rectangle(0, 0, mainImage.Width, mainImage.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, mainImage.PixelFormat);

            Console.Out.WriteLine("Creating mosaic...");

            String[] files = Directory.GetFiles(DIRNAME); // pikkukuvien paikka
            byte[,,] area = new byte[W, H, 3];
            int x = 0, y = 0;
            int cc = 0;

            for (x = 0; x < mainImage.Width - W; x += W)
            {
                for (y = 0; y < mainImage.Height - H; y += H)
                {
                    // W,H kokoinen alue taulukkoon talteen
                    for (int sx = 0; sx < W; sx++)
                    {
                        for (int sy = 0; sy < H; sy++)
                        {
                            unsafe
                            {
                                //Color c = mainImage.GetPixel(sx + x, sy + y);
                                byte* row = (byte*)mainBmpData.Scan0 + (y * mainBmpData.Stride);
                                area[sx, sy, 0] = row[(sx + x) * pixelSize];
                                area[sx, sy, 1] = row[(sx + x) * pixelSize + 1];
                                area[sx, sy, 2] = row[(sx + x) * pixelSize + 2];
                            }
                        }
                    }

                    // käy läpi kaikki pikkukuvat ja valitaan paras
                    float lastDiff = 100000;
                    int bestIndex = 0, index = 0;
                    //Color c = new Color();
                    foreach (String f in files)
                    {
                        float diff = 0.0f;
                        Bitmap bmp = (Bitmap)Image.FromFile(f);
                        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly, mainImage.PixelFormat);

                        for (int sx = 0; sx < W; sx++)
                        {
                            for (int sy = 0; sy < H; sy++)
                            {
                                unsafe
                                {
                                    //Color c = bmp.GetPixel(sx, sy);
                                    byte* row = (byte*)bmpData.Scan0 + (sy * bmpData.Stride);
                                    diff += (float)Math.Abs(area[sx, sy, 0] - row[sx * pixelSize]) / 255.0f;
                                    diff += (float)Math.Abs(area[sx, sy, 1] - row[sx * pixelSize + 1]) / 255.0f;
                                    diff += (float)Math.Abs(area[sx, sy, 2] - row[sx * pixelSize + 2]) / 255.0f;
                                }
                            }
                        }
                        if (diff < lastDiff)
                        {
                            lastDiff = diff;
                            bestIndex = index;
                        }

                        bmp.UnlockBits(bmpData);
                        index++;
                    }

                    // piirretään mainImageen paras kuvavaihtoehto
                    Bitmap bbmp = (Bitmap)Image.FromFile(files[bestIndex]);
                    BitmapData bbmpData = bbmp.LockBits(new Rectangle(0, 0, bbmp.Width, bbmp.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly, mainImage.PixelFormat);
                    for (int sx = 0; sx < W; sx++)
                    {
                        for (int sy = 0; sy < H; sy++)
                        {
                            unsafe
                            {
                                //mainImage.SetPixel(sx + x, sy + y, bbmp.GetPixel(sx, sy));
                                byte* row = (byte*)mainBmpData.Scan0 + ((sy + y) * mainBmpData.Stride);
                                byte* row2 = (byte*)bbmpData.Scan0 + (sy * bbmpData.Stride);
                                row[(sx + x) * pixelSize] = row2[(sx) * pixelSize];
                                row[(sx + x) * pixelSize + 1] = row2[(sx) * pixelSize + 1];
                                row[(sx + x) * pixelSize + 2] = row2[(sx) * pixelSize + 2];
                            }
                        }
                    }
                    bbmp.UnlockBits(bbmpData);

                    Console.Out.Write(".");
                    try
                    {
                        cc++;
                        if (cc % 10 == 0)
                            mainImage.Save("out" + W + ".jpg");
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            mainImage.UnlockBits(mainBmpData);

            mainImage.Save("out" + W + ".jpg");
        }
    }
}
