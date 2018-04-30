﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Collections;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;

namespace WpfAppTest.Base
{
    public static class Calculation
    {
        /// <summary>
        /// 判斷前組合
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cbl"></param>
        /// <param name="type">Type: 1.用code 2.用id</param>
        /// <returns></returns>
        private static string BeforeCheck(List<BaseOptions> data, CheckBoxList cbl, int type = 1)
        {
            if (data == null || data.Count() == 0 ||
                cbl == null || cbl.SelectedValue == "" ||
                cbl.SelectedValue == "".PadRight(cbl.SelectedValue.Length, '0'))
                return "";

            var tmp = cbl.ItemsSource.Cast<BaseOptions>().ToList();
            if (tmp == null)
                return "";

            string condition = "";
            for (int i = 0; i < cbl.SelectedValue.Count(); i++)
            {
                if (cbl.SelectedValue[i] == '1')
                    condition += (condition == "" ? "" : " ") + string.Join(" ", tmp.Where(x => x.ID == i + 1).Select(x => x.Code));
            }
            return condition;
        }

        #region 定位 or 定位殺 or 組合
        /// <summary>
        /// 定位 or 定位殺 or 組合
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="condition"></param>
        /// <param name="pos">定位的index</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> PosNumber(List<BaseOptions> data, string condition, string pos, bool isKeep)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                int posindex = 0;
                bool isexists = false;

                //跑全部資料
                foreach (var item in data)
                {
                    //結果判斷
                    if (int.TryParse(pos, out posindex))
                    {
                        if (conArray.Contains(item.Code[posindex].ToString()))
                            isexists = true;
                        else
                            isexists = false;
                    }
                    else
                    {
                        foreach (var c in item.Code)
                        {
                            if (conArray.Contains(c.ToString()))
                            {
                                isexists = true;
                                break;
                            }
                            else
                                isexists = false;
                        }
                    }

                    if (isexists)
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 定位 or 定位殺 or 組合-for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl"></param>
        /// <param name="pos">定位的index</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> PosNumber(List<BaseOptions> data, CheckBoxList cbl, string pos, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return PosNumber(data, condition, pos, isKeep);
        }
        #endregion

        #region 直選
        /// <summary>
        /// 直選
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="condition"></param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> AssignNumber(List<BaseOptions> data, string condition, bool isKeep)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');

                //不保留
                if (!isKeep)
                    tmpData = tmpData.Where(x => !conArray.Contains(x.Code)).ToList();
                else
                    tmpData = tmpData.Where(x => conArray.Contains(x.Code)).ToList();
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }
        #endregion

        #region 任n碼
        /// <summary>
        /// 任n碼
        /// 組號裡出現某些數字,任n碼
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="condition"></param>
        /// <param name="n">出n碼</param>
        /// <param name="isKeep">是否保留 true出n碼 false殺n碼</param>
        public static List<BaseOptions> ExistsNumber(List<BaseOptions> data, string condition, int n, bool isKeep)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                conArray = conArray.Where(x => x.ToString().Length == n).ToArray();
                bool match = false;

                foreach (var item in data)
                {
                    foreach (var no in conArray)
                    {
                        foreach (var c in no)
                        {
                            match = item.Code.Contains(c.ToString());
                            if (!match)
                                break;
                        }
                    }

                    if (match)
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }
        #endregion

        #region 奇偶判斷
        /// <summary>
        /// 奇偶數判斷-for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="condition"></param>
        /// <param name="isKeep">是否保留</param>
        private static List<BaseOptions> OddEvenNumber(List<BaseOptions> data, string condition, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                int number = 0;
                string checkvalue = "";

                //跑全部資料
                foreach (var item in data)
                {
                    checkvalue = "";
                    //每一筆資料的數字
                    foreach (var c in item.Code)
                    {
                        int.TryParse(c.ToString(), out number);
                        checkvalue += (number % 2).ToString();
                    }

                    //結果判斷
                    if (conArray.Contains(checkvalue))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 奇偶數判斷-for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> OddEvenNumber(List<BaseOptions> data, CheckBoxList cbl, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return OddEvenNumber(data, condition, isKeep);
        }
        #endregion

        #region 大中小判斷
        /// <summary>
        /// 大中小判斷
        /// 大0 中1 小2 ; 大0 小1
        /// </summary>
        private static List<BaseOptions> CheckValueNumber(List<BaseOptions> data, string condition, int type, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                int number = 0;
                string checkvalue = "";
                double lenth = (double)9 / (type == 1 ? 2 : 3);

                //跑全部資料
                foreach (var item in data)
                {
                    checkvalue = "";
                    //每一筆資料的數字
                    foreach (var c in item.Code)
                    {
                        int.TryParse(c.ToString(), out number);
                        checkvalue += ((int)(number / lenth) - (number > 0 && number % lenth == 0 ? 1 : 0)).ToString();
                    }

                    //結果判斷
                    if (conArray.Contains(checkvalue))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 大中小判斷 for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="type">Type=1:判斷大小 Type=2:判斷大中小</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> CheckValueNumber(List<BaseOptions> data, CheckBoxList cbl, int type, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return CheckValueNumber(data, condition, type, isKeep);
        }
        #endregion

        #region 012路判斷 or 除三餘數
        /// <summary>
        /// 012路判斷 or 除三餘數
        /// </summary>
        private static List<BaseOptions> DivThreeRemainder(List<BaseOptions> data, string condition, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                int number = 0;
                string checkvalue = "";

                //跑全部資料
                foreach (var item in data)
                {
                    checkvalue = "";
                    //每一筆資料的數字
                    foreach (var c in item.Code)
                    {
                        int.TryParse(c.ToString(), out number);
                        checkvalue += (number % 3).ToString();
                    }

                    //結果判斷
                    if (conArray.Contains(checkvalue))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 012路判斷 or 除三餘數 for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> DivThreeRemainder(List<BaseOptions> data, CheckBoxList cbl, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return DivThreeRemainder(data, condition, isKeep);
        }
        #endregion

        #region 質合判斷
        /// <summary>
        /// 質合判斷
        /// </summary>
        private static List<BaseOptions> PrimeNumber(List<BaseOptions> data, string condition, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                string checkvalue = "";
                string[] prime = new string[5] { "1", "2", "3", "5", "7" };

                //跑全部資料
                foreach (var item in data)
                {
                    checkvalue = "";
                    //每一筆資料的數字
                    foreach (var c in item.Code)
                    {
                        checkvalue += (prime.Contains(c.ToString()) ? "1" : "0");
                    }

                    //結果判斷
                    if (conArray.Contains(checkvalue))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 質合判斷 for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> PrimeNumber(List<BaseOptions> data, CheckBoxList cbl, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return PrimeNumber(data, condition, isKeep);
        }
        #endregion

        #region 和值判斷
        /// <summary>
        /// 和值判斷
        /// </summary>
        public static List<BaseOptions> SumNumber(List<BaseOptions> data, string condition, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                int number = 0;

                //跑全部資料
                foreach (var item in data)
                {
                    number = 0;
                    //每一筆資料的數字
                    foreach (var c in item.Code)
                    {
                        number += int.Parse(c.ToString());
                    }

                    //結果判斷
                    if (conArray.Contains(number.ToString()))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 和值判斷 for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> SumNumber(List<BaseOptions> data, CheckBoxList cbl, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return SumNumber(data, condition, isKeep);
        }
        #endregion

        #region 合尾判斷
        /// <summary>
        /// 合尾判斷
        /// </summary>
        private static List<BaseOptions> SumLastNumber(List<BaseOptions> data, string condition, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                int number = 0;

                //跑全部資料
                foreach (var item in data)
                {
                    number = 0;
                    //每一筆資料的數字
                    foreach (var c in item.Code)
                    {
                        number += int.Parse(c.ToString());
                    }

                    //結果判斷
                    if (conArray.Contains(number.ToString().Substring(number.ToString().Length - 1)))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 合尾判斷 for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> SumLastNumber(List<BaseOptions> data, CheckBoxList cbl, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return SumLastNumber(data, condition, isKeep);
        }
        #endregion

        #region 跨度判斷
        /// <summary>
        /// 跨度判斷
        /// </summary>
        private static List<BaseOptions> CrossNumber(List<BaseOptions> data, string condition, bool isKeep = true)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (!string.IsNullOrEmpty(condition))
            {
                var conArray = condition.Split(' ');
                char[] tmp;
                int number = 0;

                //跑全部資料
                foreach (var item in data)
                {
                    tmp = item.Code.ToArray();
                    number = int.Parse(tmp.Max().ToString()) - int.Parse(tmp.Min().ToString());

                    //結果判斷
                    if (conArray.Contains(number.ToString()))
                    {
                        if (!isKeep)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (isKeep)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }

        /// <summary>
        /// 跨度判斷 for CheckBoxList使用
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="cbl">CheckBoxList</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> CrossNumber(List<BaseOptions> data, CheckBoxList cbl, bool isKeep = true)
        {
            string condition = BeforeCheck(data, cbl);
            return CrossNumber(data, condition, isKeep);
        }
        #endregion

        #region 二星-不定型態-類型
        /// <summary>
        /// 二星-不定型態-類型
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="value">選項值</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> TwoStartType(List<BaseOptions> data, string value, bool isKeep)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (tmpData != null && value.Contains("1"))
            {
                bool match = false;
                foreach (var item in data)
                {
                    match = (value[0] == '1' && PairNumber(item)) ||
                            (value[1] == '1' && ContinueNumber(item)) || 
                            (value[3] == '1' && FalsePair(item, 5)) ||
                            (value[4] == '1' && FalsePair(item, 6));

                    //散號
                    if (value[2] == '1')
                        match = (match || (!PairNumber(item) && !ContinueNumber(item)) && !FalsePair(item, 5) && !FalsePair(item, 6));

                    if (isKeep)
                    {
                        if (!match)
                            tmpData.Remove(item);
                    }
                    else
                    {
                        if (match)
                            tmpData.Remove(item);
                    }
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }
        #endregion

        #region 四星-殺特殊型態
        /// <summary>
        /// 四星-殺特殊型態
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="ht">資料集</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> FourSpecialData(List<BaseOptions> data, string value)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (tmpData != null && value.Contains("1"))
            {
                bool match = true;
                foreach (var item in data)
                {
                    match = (value[0] == '1' && ASCNumber(item)) ||
                            (value[1] == '1' && DESCNumber(item)) ||
                            (value[2] == '1' && ConvexNumber(item)) ||
                            (value[3] == '1' && ConcaveNumber(item)) ||
                            (value[4] == '1' && NNumber(item)) ||
                            (value[5] == '1' && ReverseNNumber(item));

                    if (match)
                        tmpData.Remove(item);
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }
        #endregion

        #region 四星-特別排除
        /// <summary>
        /// 四星-特別排除
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="ht">資料集</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> FourSpecial2Data(List<BaseOptions> data, string value)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (tmpData != null && value.Contains("1"))
            {
                bool match = true;
                foreach (var item in data)
                {
                    match = (value[0] == '1' && SameNumber(item)) ||
                            (value[1] == '1' && DisContinueNumber(item)) ||
                            (value[2] == '1' && SameNumber(item)) ||
                            (value[3] == '1' && SameNumber(item)) ||
                            (value[4] == '1' && SameNumber(item)) ||
                            (value[5] == '1' && SameNumber(item)) ||
                            (value[6] == '1' && PairNumber(item)) ||
                            (value[7] == '1' && ThreeSameNumber(item)) ||
                            (value[8] == '1' && TwoPairNumber(item));

                    if (match)
                        tmpData.Remove(item);
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }
        #endregion

        #region 五星-特別排除
        /// <summary>
        /// 五星-特別排除
        /// </summary>
        /// <param name="data">全部組合資料</param>
        /// <param name="ht">資料集</param>
        /// <param name="isKeep">是否保留</param>
        public static List<BaseOptions> FiveSpecialData(List<BaseOptions> data, string value)
        {
            List<BaseOptions> tmpData = (data == null ? null : data.ToList());
            if (tmpData != null && value.Contains("1"))
            {
                bool match = true;
                foreach (var item in data)
                {
                    match = (value[0] == '1' && ASCNumber(item)) ||
                            (value[1] == '1' && DESCNumber(item)) ||
                            (value[2] == '1' && SameNumber(item)) ||
                            (value[3] == '1' && SameNumber(item)) ||
                            (value[4] == '1' && SameNumber(item)) ||
                            (value[5] == '1' && SameNumber(item)) ||
                            (value[6] == '1' && SameNumber(item)) ||
                            (value[7] == '1' && AAAAANumber(item)) ||
                            (value[8] == '1' && AABCDNumber(item)) ||
                            (value[9] == '1' && AABBCNumber(item)) ||
                            (value[10] == '1' && AAABBNumber(item)) ||
                            (value[11] == '1' && AAABCNumber(item)) ||
                            (value[12] == '1' && AAAABNumber(item)) ||
                            (value[13] == '1' && ABCDENumber(item));

                    if (match)
                        tmpData.Remove(item);
                }
            }
            return (tmpData == null ? null : tmpData.OrderBy(x => x.Code).ToList());
        }
        #endregion

        #region 特殊處理

        #region 上山
        /// <summary>
        /// 上山
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合上山 false:未符合上山</returns>
        private static bool ASCNumber(BaseOptions data)
        {
            bool match = false;

            if (data != null)
            {
                int tmpNumber = -1;
                int codeChar = 0;
                if (data.Code != "".PadRight(data.Code.Count(), data.Code[0]))
                {
                    foreach (var c in data.Code)
                    {
                        int.TryParse(c.ToString(), out codeChar);

                        if (codeChar >= tmpNumber)
                        {
                            tmpNumber = codeChar;
                            match = true;
                        }
                        else
                            match = false;

                        if (!match)
                            break;
                    }
                }
            }
            return match;
        }
        #endregion

        #region 下山
        /// <summary>
        /// 下山
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合下山 false:未符合下山</returns>
        private static bool DESCNumber(BaseOptions data)
        {
            bool match = false;

            if (data != null)
            {
                if (data.Code != "".PadRight(data.Code.Count(), data.Code[0]))
                {
                    int tmpNumber = 10;
                    int codeChar = 0;
                    foreach (var c in data.Code)
                    {
                        int.TryParse(c.ToString(), out codeChar);
                        if (codeChar <= tmpNumber)
                        {
                            tmpNumber = codeChar;
                            match = true;
                        }
                        else
                            match = false;

                        if (!match)
                            break;
                    }
                }
            }
            return match;
        }
        #endregion

        #region 凸型
        /// <summary>
        /// 凸型
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合凸型 false:未符合凸型</returns>
        private static bool ConvexNumber(BaseOptions data)
        {
            bool match = true;

            if (data != null)
            {
                var s = data.Code.Substring(1, data.Code.Length - 2);

                int codeChar = 0;

                //第一碼
                int firstNo = 0;
                int.TryParse(data.Code[0].ToString(), out firstNo);

                //最後一碼
                int lastNo = 0;
                int.TryParse(data.Code[data.Code.Length - 1].ToString(), out lastNo);
                foreach (var c in s)
                {
                    int.TryParse(c.ToString(), out codeChar);
                    if (codeChar < firstNo || codeChar < lastNo)
                        match = false;

                    if (!match)
                        break;
                }
            }
            return match;
        }
        #endregion

        #region 凹型
        /// <summary>
        /// 凹型
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合凹型 false:未符合凹型</returns>
        private static bool ConcaveNumber(BaseOptions data)
        {
            bool match = true;

            if (data != null)
            {
                int codeChar = 0;

                //第一碼
                int firstNo = 0;
                int.TryParse(data.Code[0].ToString(), out firstNo);

                //最後一碼
                int lastNo = 0;
                int.TryParse(data.Code[data.Code.Length - 1].ToString(), out lastNo);

                foreach (var c in data.Code.Substring(1, data.Code.Length - 2))
                {
                    int.TryParse(c.ToString(), out codeChar);
                    if (codeChar < firstNo || codeChar > lastNo)
                        match = false;

                    if (!match)
                        break;
                }
            }
            return match;
        }
        #endregion

        #region N型
        /// <summary>
        /// N型
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合N型 false:未符合N型</returns>
        private static bool NNumber(BaseOptions data)
        {
            bool match = true;

            if (data != null)
            {
                int codeChar = 0;

                //暫存數字
                int tmpNumber = 10;

                int count = 1;
                foreach (var c in data.Code)
                {
                    int.TryParse(c.ToString(), out codeChar);
                    if (count % 2 == 1)
                    {
                        //奇數
                        if (codeChar >= tmpNumber)
                            match = false;
                        else
                            tmpNumber = codeChar;
                    }
                    else
                    {
                        //偶數
                        if (codeChar <= tmpNumber)
                            match = false;
                        else
                            tmpNumber = codeChar;
                    }

                    if (!match)
                        break;
                    count++;
                }
            }
            return match;
        }
        #endregion

        #region 反N型
        /// <summary>
        /// 反N型
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合反N型 false:未符合反N型</returns>
        private static bool ReverseNNumber(BaseOptions data)
        {
            bool match = true;

            if (data != null)
            {
                int codeChar = 0;

                //暫存數字
                int tmpNumber = -1;

                int count = 1;
                foreach (var c in data.Code)
                {
                    int.TryParse(c.ToString(), out codeChar);
                    if (count % 2 == 1)
                    {
                        //奇數
                        if (codeChar <= tmpNumber)
                            match = false;
                        else
                            tmpNumber = codeChar;
                    }
                    else
                    {
                        //偶數
                        if (codeChar >= tmpNumber)
                            match = false;
                        else
                            tmpNumber = codeChar;
                    }

                    if (!match)
                        break;
                    count++;
                }
            }
            return match;
        }
        #endregion

        #region 豹子
        /// <summary>
        /// 豹子
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合豹子 false:未符合豹子</returns>
        private static bool SameNumber(BaseOptions data)
        {
            return data.Code.ToCharArray().Distinct().Count() == 1;
        }
        #endregion

        #region 假對/假連
        /// <summary>
        /// 假對/假連-二星使用
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:假對 false:非假對</returns>
        private static bool FalsePair(BaseOptions data, int value)
        {
            int i = int.Parse(data.Code[0].ToString()) - int.Parse(data.Code[1].ToString());
            return (i < 0 ? -1 : 1) * i == value;
        }
        #endregion

        #region 連號-二星使用
        /// <summary>
        /// 連號-二星使用
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:連號 false:不連號</returns>
        private static bool ContinueNumber(BaseOptions data)
        {
            int last = -99;
            int tmpNumber = 0;
            foreach (var c in data.Code)
            {
                int.TryParse(c.ToString(), out tmpNumber);
                if (tmpNumber - 1 == last || tmpNumber + 1 == last)
                    return true;

                last = tmpNumber;
            }
            return false;
        }
        #endregion

        #region 不連
        /// <summary>
        /// 不連
        /// remark:0 跟 9 是連號
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:不連號 false:有連號</returns>
        private static bool DisContinueNumber(BaseOptions data)
        {
            //最後一碼
            int last = 0;
            int.TryParse(data.Code[data.Code.Length - 1].ToString(), out last);
            int tmpNumber = 0;
            foreach (var c in data.Code)
            {
                int.TryParse(c.ToString(), out tmpNumber);
                if ((tmpNumber - 1 == -1 ? 9 : tmpNumber - 1) == last ||
                    (tmpNumber + 1 == 10 ? 0 : tmpNumber + 1) == last)
                    return false;
                int.TryParse(c.ToString(), out last);
            }
            return true;
        }
        #endregion

        #region 二連
        #endregion

        #region 三連
        #endregion

        #region 四連
        #endregion

        #region 五連
        #endregion

        #region 散號
        #endregion

        #region 對子號
        /// <summary>
        /// 對子號
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合對子號 false:未符合對子號</returns>
        private static bool PairNumber(BaseOptions data)
        {
            return data.Code.ToCharArray().Distinct().Count() <= (data.Code.Length - 1);
        }
        #endregion

        #region 三同號
        /// <summary>
        /// 三同號
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合三同號 false:未符合三同號</returns>
        private static bool ThreeSameNumber(BaseOptions data)
        {
            for (int i = 0; i <= 9; i++)
            {
                if (data.Code.Where(x => x.ToString() == i.ToString()).Count() >= 3)
                    return true;
            }
            return false;
        }
        #endregion

        #region 兩個對子
        /// <summary>
        /// 兩個對子
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合兩個對子 false:未符合兩個對子</returns>
        private static bool TwoPairNumber(BaseOptions data)
        {
            return data.Code.ToCharArray().Distinct().Count() == 2;
        }
        #endregion

        #region AAAAA
        /// <summary>
        /// 豹子
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:有符合豹子 false:未符合豹子</returns>
        private static bool AAAAANumber(BaseOptions data)
        {
            return data.Code.ToCharArray().Distinct().Count() == 1;
        }
        #endregion

        #region AABCD
        /// <summary>
        /// AABCD
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:符合 false:未符合</returns>
        private static bool AABCDNumber(BaseOptions data)
        {
            if (data.Code.ToCharArray().Distinct().Count() == 4)
            {
                if (data.Code[0] == data.Code[1])
                    return true;
            }
            return false;
        }
        #endregion

        #region AABBC
        /// <summary>
        /// AABBC
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:符合 false:未符合</returns>
        private static bool AABBCNumber(BaseOptions data)
        {
            if (data.Code.ToCharArray().Distinct().Count() == 3)
            {
                if (data.Code[0] == data.Code[1] &&
                    data.Code[2] == data.Code[3])
                    return true;
            }
            return false;
        }
        #endregion

        #region AAABB
        /// <summary>
        /// AAABB
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:符合 false:未符合</returns>
        private static bool AAABBNumber(BaseOptions data)
        {
            if (data.Code.ToCharArray().Distinct().Count() == 2)
            {
                if (data.Code[0] == data.Code[1] &&
                    data.Code[0] == data.Code[2] &&
                    data.Code[3] == data.Code[4])
                    return true;
            }
            return false;
        }
        #endregion

        #region AAABC
        /// <summary>
        /// AAABC
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:符合 false:未符合</returns>
        private static bool AAABCNumber(BaseOptions data)
        {
            if (data.Code.ToCharArray().Distinct().Count() == 3)
            {
                if (data.Code[0] == data.Code[1] &&
                    data.Code[0] == data.Code[2])
                    return true;
            }
            return false;
        }
        #endregion

        #region AAAAB
        /// <summary>
        /// AAAAB
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:符合 false:未符合</returns>
        private static bool AAAABNumber(BaseOptions data)
        {
            if (data.Code.ToCharArray().Distinct().Count() == 2)
            {
                if (data.Code[0] == data.Code[1] &&
                    data.Code[0] == data.Code[2] &&
                    data.Code[0] == data.Code[3])
                    return true;
            }
            return false;
        }
        #endregion

        #region ABCDE
        /// <summary>
        /// ABCDE
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true:符合 false:未符合</returns>
        private static bool ABCDENumber(BaseOptions data)
        {
            return data.Code.ToCharArray().Distinct().Count() == 5;
        }
        #endregion

        #endregion
    }
}
