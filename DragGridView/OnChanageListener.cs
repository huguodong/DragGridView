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
         * ��item����λ�õ�ʱ��ص��ķ���������ֻ��Ҫ�ڸ÷�����ʵ�����ݵĽ������� 
         * @param form 
         *          ��ʼ��position 
         * @param to  
         *          ��ק����position 
         */
        void onChange(int form, int to);
    }
}