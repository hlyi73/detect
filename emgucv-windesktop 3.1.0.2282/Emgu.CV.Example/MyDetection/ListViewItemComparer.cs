using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShapeDetection
{
    class ListViewItemComparer : IComparer

    {

        bool dir;

        private int col;

        public ListViewItemComparer()

        {

            col = 0;
            dir = true;

        }

        public ListViewItemComparer(int col)

        {

            this.col = col;
            dir = true;

        }
        public ListViewItemComparer(int col,bool dir)

        {

            this.col = col;
            this.dir = dir;

        }

        public int Compare(object x, object y)

        {

            if (this.dir)
            {
                
                return (int)(double.Parse(((ListViewItem)y).SubItems[col].Text) - double.Parse(((ListViewItem)x).SubItems[col].Text)) * 1000;
            }
            else
            {
                return (int)(double.Parse(((ListViewItem)x).SubItems[col].Text)  - double.Parse(((ListViewItem)y).SubItems[col].Text)) * 1000;
            }

        }

    }

}
