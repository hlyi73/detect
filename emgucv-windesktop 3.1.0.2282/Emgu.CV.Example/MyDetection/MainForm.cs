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
        private bool dir;
        public MainForm()
        {
            InitializeComponent();
            dir = true;

            //fileNameTextBox.Text = "d:\\test.jpg";
        }

        public void Compute()
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

                this.imageBox1.Image = uimage;


                uimage = filter1(uimage);
                imageBox2.Image = uimage;
                //uimage = filter2(uimage);
                //circleImageBox.Image = uimage;
                //Image<Gray, byte>  image = filter3(uimage);
                // lineImageBox.Image = image;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Compute();
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
           // UMat cannyEdges = new UMat();
            //CvInvoke.AdaptiveThreshold(s, cannyEdges, 1,AdaptiveThresholdType.MeanC, ThresholdType.Binary, 1, 1);
            return cannyEdges;
        }


        private UMat filter1(UMat s)
        {
            UMat cannyEdges = new UMat();
            CvInvoke.Threshold(s, cannyEdges, (int)this.numericUpDown_limitDown.Value, 255, ThresholdType.BinaryInv);
            //CvInvoke.BitwiseNot(cannyEdges, cannyEdges);
            //cannyEdges= filter2(cannyEdges);


            cannyEdges = filter3(cannyEdges).ToUMat();

            return cannyEdges;
        }
        private Image<Gray, byte> filter3(UMat s)
        {
            this.listView1.Items.Clear();
            double rate = (double)this.numericUpDown_rate.Value;
            VectorOfVectorOfPoint con = new VectorOfVectorOfPoint();
            //Image<Gray, byte> c = new Image<Gray, byte>(s.Size.Width, s.Size.Height);
            Image<Gray, byte> d = new Image<Gray, byte>(s.Size.Width, s.Size.Height);
            CvInvoke.FindContours(s, con, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < con.Size; i++)
            {
                using (VectorOfPoint contour = con[i])
                {
                    double area = CvInvoke.ContourArea(contour);

                    if (area <=0) continue;

                    double radius = Math.Sqrt(area / 3.14) * 2 * rate;
                    if (radius <= 0) continue;

                    {
                        Point[] ps = contour.ToArray();
                        Point pt = new Point(0,0);
                        foreach(Point p in ps)
                        {
                            if (pt.X == 0)
                            {
                                pt.X = p.X;
                            }
                            if (pt.Y == 0)
                            {
                                pt.Y = p.Y;
                            }
                            if (pt.X > p.X)
                            {
                                pt.X = p.X;
                            }
                            if (pt.Y > p.Y)
                            {
                                pt.Y = p.Y;
                            }

                        }
                        if (pt.Y <= 10)
                        {
                            foreach (Point p in ps)
                            {
                                if (pt.Y == 0)
                                {
                                    pt.Y = p.Y;
                                }
                                if (pt.Y < p.Y)
                                {
                                    pt.Y = p.Y;
                                }

                            }
                            pt.Y = pt.Y + 10;
                        }
                        //
                        CvInvoke.PutText(d, String.Format("{0:d}", i), pt, FontFace.HersheySimplex, 0.3, new Bgr(255, 255, 255).MCvScalar);
                        CvInvoke.DrawContours(d, con, i, new MCvScalar(255, 0, 255, 255), 1);
                        addItem(String.Format("{0:d}", i) , String.Format("{0:f}", radius) );
                    }
                }

            }
            return d;
        }
        private void addItem(String no,String data)
        {
            this.listView1.BeginUpdate();   //���ݸ��£�UI��ʱ����ֱ��EndUpdate���ƿؼ���������Ч������˸�������߼����ٶ� 

            ListViewItem lvi = new ListViewItem();

            lvi.Text = no;// String.Format("{0:d}", this.listView1.Items.Count);
                //lvi.ImageIndex = i;     //ͨ����imageList�󶨣���ʾimageList�е�i��ͼ�� 


                lvi.SubItems.Add(data);

            this.listView1.Items.Add(lvi);

            this.listView1.EndUpdate();  //�������ݴ���UI����һ���Ի��ơ� 
        }
        private Image<Gray, byte> filter4(UMat s)
        {
            VectorOfVectorOfPoint con = new VectorOfVectorOfPoint();
            //Image<Gray, byte> c = new Image<Gray, byte>(s.Size.Width, s.Size.Height);
            Image<Gray, byte> d = new Image<Gray, byte>(s.Size.Width, s.Size.Height);
            CvInvoke.FindContours(s, con, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < con.Size; i++)
            {
                CvInvoke.DrawContours(d, con, i, new MCvScalar(255, 0, 255, 255), 1);
            }
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown_limitDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Compute();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "csv�ļ�|*.csv";//�����ļ�������
                                                  //saveFileDialog.FileName = "Lanny.raw";//Ĭ���ļ���

            DialogResult result = saveFileDialog.ShowDialog();
            string localFilePath = "";
            if (result == DialogResult.OK)
            {
                //����ļ�·��
                localFilePath = saveFileDialog.FileName.ToString();
                ListViewToCSV.WriteListViewToCSV(this.listView1, localFilePath, true);
                MessageBox.Show("����ɹ�!", "��ʾ");
                //           System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();//����ļ�
                //��ʼд�����֡�����
                //         fs.Write(Encoding.ASCII.GetBytes("Hello222"), 0, "Hello222".Length);
            }
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpg�ļ�|*.jpg";//�����ļ�������
                                                  //saveFileDialog.FileName = "Lanny.raw";//Ĭ���ļ���

            DialogResult result = saveFileDialog.ShowDialog();
            string localFilePath = "";
            if (result == DialogResult.OK)
            {
                //����ļ�·��
                localFilePath = saveFileDialog.FileName.ToString();
                imageBox2.Image.Save(localFilePath);
                MessageBox.Show("����ɹ�!", "��ʾ");
                //           System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();//����ļ�
                //��ʼд�����֡�����
                //         fs.Write(Encoding.ASCII.GetBytes("Hello222"), 0, "Hello222".Length);
            }

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            dir = !dir;
            this.listView1.ListViewItemSorter =new ListViewItemComparer(e.Column, dir);
        }
    }
}
