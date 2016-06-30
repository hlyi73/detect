//----------------------------------------------------------------------------
//  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Diagnostics;
using Emgu.CV.Util;

namespace ShapeDetection
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            fileNameTextBox.Text = "d:\\test.jpg";
        }

        public void PerformShapeDetection()
        {
            if (fileNameTextBox.Text != String.Empty)
            {
                StringBuilder msgBuilder = new StringBuilder("Performance: ");

                //Load the image from file and resize it for display
                Image<Bgr, Byte> img =
                   new Image<Bgr, byte>(fileNameTextBox.Text);

                //Convert the image to grayscale and filter out the noise
                UMat uimage = new UMat();
                CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

                this.originalImageBox.Image = uimage;


                uimage = filter1(uimage);
                triangleRectangleImageBox.Image = uimage;
                uimage = filter2(uimage);
                circleImageBox.Image = uimage;
                Image<Gray, byte>  image = filter3(uimage);
                 lineImageBox.Image = image;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PerformShapeDetection();
        }

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                fileNameTextBox.Text = openFileDialog1.FileName;
            }
        }

        private UMat filter2(UMat s)
        {
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(s, cannyEdges, 1, 255);
            return cannyEdges;
        }
        private UMat filter1(UMat s)
        {
            UMat cannyEdges = new UMat();
            CvInvoke.Threshold(s, cannyEdges, 180, 255, ThresholdType.BinaryInv);
            return cannyEdges;
        }
        private Image<Gray, byte> filter3(UMat s)
        {
            VectorOfVectorOfPoint con = new VectorOfVectorOfPoint();
            Image<Gray, byte> c = new Image<Gray, byte>(s.Size.Width, s.Size.Height);
            Image<Gray, byte> d = new Image<Gray, byte>(s.Size.Width, s.Size.Height);
            CvInvoke.FindContours(s, con, c, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < con.Size; i++)
                CvInvoke.DrawContours(d, con, i, new MCvScalar(255, 0, 255, 255), 1);
            return d;
        }
        /// <summary>
        /// Filter the license plate to remove noise
        /// </summary>
        /// <param name="plate">The license plate image</param>
        /// <returns>License plate image without the noise</returns>
        private static UMat FilterPlate(UMat plate)
        {
            UMat thresh = new UMat();
            CvInvoke.Threshold(plate, thresh, 120, 255, ThresholdType.BinaryInv);
            //Image<Gray, Byte> thresh = plate.ThresholdBinaryInv(new Gray(120), new Gray(255));

            Size plateSize = plate.Size;
            using (Mat plateMask = new Mat(plateSize.Height, plateSize.Width, DepthType.Cv8U, 1))
            using (Mat plateCanny = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                plateMask.SetTo(new MCvScalar(255.0));
                CvInvoke.Canny(plate, plateCanny, 100, 50);
                CvInvoke.FindContours(plateCanny, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                int count = contours.Size;
                for (int i = 1; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    {

                        Rectangle rect = CvInvoke.BoundingRectangle(contour);
                        if (rect.Height > (plateSize.Height >> 1))
                        {
                            rect.X -= 1; rect.Y -= 1; rect.Width += 2; rect.Height += 2;
                            Rectangle roi = new Rectangle(Point.Empty, plate.Size);
                            rect.Intersect(roi);
                            CvInvoke.Rectangle(plateMask, rect, new MCvScalar(), -1);
                            //plateMask.Draw(rect, new Gray(0.0), -1);
                        }
                    }

                }

                thresh.SetTo(new MCvScalar(), plateMask);
            }

            CvInvoke.Erode(thresh, thresh, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(thresh, thresh, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

            return thresh;
        }

    }
}
