﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfAppTest;
using WpfApp.Custom;
using Wpf.Base;

namespace WinFormsApp1
{
    /// <summary>
    /// UcWaterWindow.xaml 的互動邏輯
    /// </summary>
    public partial class UcWaterWindow : UserControl
    {
        public UcWaterWindow()
        {
            InitializeComponent();
        }

        bool IsFirstTime = true;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsFirstTime)
            {
                SetListBoxItems();
                IsFirstTime = false;
            }
        }

        /// <summary>
        /// 設定選項
        /// </summary>
        void SetListBoxItems()
        {
            lbType.ItemsSource = Calculation.CreateOption(1, 6, new string[6] { "2星組號系統", "3星組號系統", "4星組號系統", "5星組號系統", "倍投計算器", "大底驗證" });
            if (lbType.Items.Count > 0)
                lbType.SelectedIndex = 0;
        }

        Dictionary<int, UserControl> form;

        /// <summary>
        /// ListBox選取選項事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BaseOptions tmp = (lbType.SelectedItem as BaseOptions);
            if (tmp != null)
            {
                if (form == null)
                    form = new Dictionary<int, UserControl>();

                if (!form.ContainsKey(tmp.ID))
                {
                    if (tmp.ID == 1)
                        form.Add(tmp.ID, new UcTwoStart());
                    else if (tmp.ID == 2)
                        form.Add(tmp.ID, new UcThreeStart());
                    else if (tmp.ID == 3)
                        form.Add(tmp.ID, new UcFourStart());
                    else if (tmp.ID == 4)
                        form.Add(tmp.ID, new UcFiveStart());
                    else if (tmp.ID == 5)
                        form.Add(tmp.ID, new multiple());
                    else if (tmp.ID == 6)
                        form.Add(tmp.ID, new bigbuttom());
                }

                dpContent.Children.Clear();
                dpContent.Children.Add(form[tmp.ID]);
            }
        }
    }
}
