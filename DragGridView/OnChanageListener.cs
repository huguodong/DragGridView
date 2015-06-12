using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DragGridView
{
    public interface OnChanageListener
    {

        /** 
         * 当item交换位置的时候回调的方法，我们只需要在该方法中实现数据的交换即可 
         * @param form 
         *          开始的position 
         * @param to  
         *          拖拽到的position 
         */
        void onChange(int form, int to);
    }
}